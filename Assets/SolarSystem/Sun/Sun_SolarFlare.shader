// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "Custom/Sun_SolarFlare"
{
	Properties 
	{
		[HDR] _Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_FresnelAlphaParams("Fresnel Alpha (Offset, Exponent, Scale)", vector) = (0,1,1,1)
		_TransitionAlpha("TransitionAlpha", Float) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		Blend One One
		ZWrite Off
		Cull Off

		Pass
		{
			Fog{ Mode Off }
			Lighting Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "/./../../Shaders/cginc/NearClip.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float  clipAmount : TEXCOORD1;
				float3 camToPixel : TEXCOORD2;
				float3 normal : NORMAL0;
				float4 color : COLOR0;
			};

			float4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _FresnelAlphaParams;
			float _TransitionAlpha;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
				float3 wPos = mul(unity_ObjectToWorld, v.vertex);
				o.camToPixel = normalize(wPos - _WorldSpaceCameraPos);
				o.color = v.color;
				o.clipAmount = CalcVertClipAmount(wPos);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv) * _Color;
				col.xyz *= col.a;

				float facingRatio = abs(-dot(normalize(i.camToPixel), normalize(i.normal)));
				facingRatio = _FresnelAlphaParams.x + pow(facingRatio, _FresnelAlphaParams.y) * _FresnelAlphaParams.z;
				float4 result = col * facingRatio * i.color;

				min16float4 finalColor = min16float4(result.xyz, facingRatio * col.a) * _TransitionAlpha;

				return ApplyVertClipAmount(finalColor, i.clipAmount);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
