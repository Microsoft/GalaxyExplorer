// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "Planets/RingsBasic"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TransitionAlpha("TransitionAlpha", Float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Geometry" "Queue"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "/./../../Shaders/cginc/NearClip.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float  clipAmount : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float _TransitionAlpha;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				float3 wPos = mul(unity_ObjectToWorld, v.vertex);
				o.clipAmount = CalcVertClipAmount(wPos);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float alpha = tex2D(_MainTex, i.uv).a;

				min16float4 finalColor = min16float4(1, 1, 1, alpha * _TransitionAlpha);

				return ApplyVertClipAmount(finalColor, i.clipAmount);
			}
			ENDCG
		}
	}
}
