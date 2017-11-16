// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "Planets/LowRes"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SunPosition("Sun Position", Vector) = (1,0,0,0)
		[HDR] _Tint ("Light Tint", Color) = (1,1,1,1)
		_Ambient ("Ambient", Color) = (0,0,0,0)
		_LightGammaCorrection("Light Gamma Correction (Multiplier, Power)", Vector) = (1,1,1,1)
		_TransitionAlpha("TransitionAlpha", Float) = 1
		_SRCBLEND("Source Blend", float) = 1
		_DSTBLEND("Destination Blend", float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		Offset 10,1
		Blend [_SRCBLEND] [_DSTBLEND]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "/../../../Shaders/cginc/NearClip.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;

				float3 normal : NORMAL0;				
				float3 wPos : TEXCOORD1;
				float clipAmount : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float4 _SunPosition;
			float4 _Tint;
			float4 _Ambient;

			float4 _LightGammaCorrection;

			float _TransitionAlpha;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				float3 worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
				
				o.normal = worldNormal;
				o.wPos = mul(unity_ObjectToWorld, v.vertex);
				o.clipAmount = CalcVertClipAmount(o.wPos);
				return o;
			}
			
			min16float4 frag (v2f i) : SV_Target
			{
				float3 wPos = i.wPos;
				float3 sunDirection = normalize(_SunPosition - wPos);

				min16float ndotl = saturate(dot(i.normal, sunDirection));

				min16float4 light = pow(ndotl * (min16float)_LightGammaCorrection.x, (min16float)_LightGammaCorrection.y) * (min16float4)_Tint;

				min16float4 albedo = tex2D(_MainTex, i.uv);

				min16float4 color = albedo * (light + (min16float4)_Ambient);
				
				min16float4 finalColor = min16float4(color.xyz, (min16float)_TransitionAlpha);

				return ApplyVertClipAmount(finalColor, i.clipAmount);
			}
			ENDCG
		}
	}
}
