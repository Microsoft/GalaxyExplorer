// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "Planets/Clouds"
{
	Properties
	{
		_Noise ("Noise", 2D) = "white" {}
		_NormalAlpha ("Normal Alpha", 2D) = "white" {}
		[HDR]_Color("Color", Color) = (1,1,1,1)
		_FresnelTerm("Fresnel (Term, Offset)", Vector) = (0,0,0,0)
		_SunDirection("Sun Direction", Vector) = (1,1,1,0)
		_LightAmount ("Light amount (LightSide, DarkSide, Ambient Dark)", Vector) = (1,.2, 0, 0)
		_NoiseAmount ("Noise Amount", Float) = 1
			
		[HDR]_AmbientColor("Ambient Color", Color) = (1,1,1,1)

		_TransitionAlpha("TransitionAlpha", Float) = 1
	}
	SubShader
	{
		Tags 
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			
			#include "UnityCG.cginc"
			#include "/./../../Shaders/cginc/NearClip.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL0;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 uv : TEXCOORD0;
				float lightAmount : TEXCOORD1;
				float clipAmount : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			sampler2D _Noise;
			float4 _Noise_ST;

			sampler2D _NormalAlpha;
			float4 _NormalAlpha_ST;

			float4 _Color;
			float4 _FresnelTerm;

			float3 _SunDirection;
			float4 _LightAmount;

			float _NoiseAmount;

			float3 _AmbientColor;
			float _TransitionAlpha;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = float4(TRANSFORM_TEX(v.uv, _NormalAlpha), TRANSFORM_TEX(v.uv, _Noise));

				float3 wPos = mul(unity_ObjectToWorld, v.vertex);
				min16float3 normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));

				min16float camToPixelDotNormal = dot(normalize(wPos - _WorldSpaceCameraPos), normal);
				min16float3 ndotl = saturate(-dot(normal, (min16float3)_SunDirection));

				min16float fresnel = saturate(camToPixelDotNormal * _FresnelTerm.x + _FresnelTerm.y);
				fresnel *= fresnel;
				fresnel = (min16float)1.0 - fresnel;

				min16float lightAmount = lerp((min16float)_LightAmount.x, (min16float)_LightAmount.y, ndotl);
				lightAmount = max(lightAmount, (min16float)_LightAmount.z) * fresnel;

				o.lightAmount = lightAmount;
				o.clipAmount = CalcVertClipAmount(wPos);

				return o;
			}
			
			min16float4 frag (v2f i) : SV_Target
			{
				min16float normalAlpha = tex2D(_NormalAlpha, i.uv.xy).a;
				min16float noise = lerp((min16float)1.0f, (min16float)tex2D(_Noise, i.uv.zw).a, (min16float)_NoiseAmount);

				min16float4 finalColor = (min16float)i.lightAmount * min16float4((min16float3)_Color.xyz * noise + (min16float3)_AmbientColor, normalAlpha * (min16float)_TransitionAlpha);

				if (finalColor.a <= (min16float)0.01)
				{
					clip(-1);
				}

				return ApplyVertClipAmount(finalColor, i.clipAmount);
			}
			ENDCG
		}
	}
}
