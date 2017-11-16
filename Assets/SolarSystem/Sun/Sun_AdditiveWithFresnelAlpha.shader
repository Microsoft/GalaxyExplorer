// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "Planets/Sun_AdditiveWithFresnelAlpha"
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
			#pragma target 5.0
			#pragma fragmentoption ARB_precision_hint_fastest

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
				float4 color : COLOR0;

				float clipAmount : TEXCOORD1;
				float facingRatio : TEXCOORD2;
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
				
				float3 normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
				float3 wPos = mul(unity_ObjectToWorld, v.vertex);
				float3 camToPixel = normalize(wPos - _WorldSpaceCameraPos);
				o.color = v.color;
				o.clipAmount = CalcVertClipAmount(wPos);


				float facingRatio = -dot(normalize(camToPixel), normal);
				facingRatio = _FresnelAlphaParams.x + pow(facingRatio, _FresnelAlphaParams.y) * _FresnelAlphaParams.z;
				o.color *= facingRatio * _Color;
				o.color.a = facingRatio;

				o.facingRatio = facingRatio;

				return o;
			}

			min16float4 frag(v2f i) : SV_Target
			{
				min16float4 col = tex2D(_MainTex, i.uv);
				col *= col.a;

				min16float4 result = col * min16float4((min16float3)i.color.xyz, (min16float)1.0);
				min16float4 finalColor = min16float4(result.xyz, col.a) * _TransitionAlpha;
				
				return ApplyVertClipAmount(finalColor, (min16float)i.clipAmount);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
