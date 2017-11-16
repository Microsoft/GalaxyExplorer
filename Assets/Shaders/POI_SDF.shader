// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "POI_SDF"
{
	Properties
	{
		_MainTex("Base (RGB), Alpha (A)", 2D) = "white" {}
	    _TextColor("Text Color", Color) = (1,1,1,1)
	    _BorderColor("Border Color", Color) = (1,1,1,1)
	    _AccentColor("Accent Color", Color) = (1,1,1,1)
		_Color("Global Tint", Color) = (1,1,1,1)
		_Threshold("Threshold", vector) = (0.45,0.5,0,0)
		_Scale("Scale", vector) = (10,10,0,0)
		_TransitionAlpha("Transition Alpha", Float) = 1
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}
			
		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "cginc/NearClip.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				half2 texcoord  : TEXCOORD0;
				float  clipAmount : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _TransitionAlpha;
			fixed4 _TextColor;
			fixed4 _BorderColor;
			fixed4 _AccentColor;
			float4 _ClipRect;
			float2 _Threshold;
			float2 _Scale;
			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				float3 wPos = mul(unity_ObjectToWorld, IN.vertex);
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.clipAmount = CalcVertClipAmount(wPos);
				OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);

				OUT.color = float4(IN.color.xyz, _Color.a);
				return OUT;
			}


			fixed4 frag(v2f IN) : SV_Target
			{
			    min16float4 color = (tex2D(_MainTex, IN.texcoord));
				min16float alpha1 = saturate((color.r - _Threshold.x) * _Scale.x);
				min16float alpha2 = saturate((color.r - _Threshold.y) * _Scale.y);

				alpha1 = color.g > 0 ? alpha2 : alpha1;

				min16float4 tint;
				tint = lerp(_TextColor, _AccentColor, color.g);
				tint = lerp(_BorderColor, tint, alpha2);

				min16float4 finalColor = tint;
				finalColor.a *= alpha1 * _TransitionAlpha * IN.color.a;

				return ApplyVertClipAmount(finalColor, IN.clipAmount);
			}
		ENDCG
		}
	}
}

