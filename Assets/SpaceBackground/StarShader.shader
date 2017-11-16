// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "Playspace/Stars"
{
	Properties
	{
		_MainTex("Base (RGB), Alpha (A)", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[HideInInspector]_ContentWorldPos("Content World Position", Vector) = (0,0,0,0)
		_ContentRadius("Content Radius", float) = 0.5

		_TransitionAlpha("Transition Alpha", Float) = 1
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Background"
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
			#include "../Shaders/cginc/NearClip.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float3 worldPos : TEXCOORD1;
				fixed4 color : COLOR;
				half2 texcoord  : TEXCOORD0;
				float  clipAmount : TEXCOORD2;
			};

			float _TransitionAlpha;
			fixed4 _Color;
			float4 _ClipRect;
			float4 _ContentWorldPos;
			float _ContentRadius;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex).xyz;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.clipAmount = CalcVertClipAmount(OUT.worldPos);
				OUT.texcoord = IN.texcoord;

				OUT.color = IN.color * _Color;
				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				// if the star is inside the radius of the content, don't draw it
				if (distance(IN.worldPos.xyz, _ContentWorldPos.xyz) < _ContentRadius)
				{
					return min16float4(0, 0, 0, 0);
				}
				half4 color = (tex2D(_MainTex, IN.texcoord)) * IN.color;
				
				min16float4 finalColor = min16float4(color.xyz, color.a * _TransitionAlpha);
				return ApplyVertClipAmount(finalColor, IN.clipAmount);
			}
		ENDCG
		}
	}
}

