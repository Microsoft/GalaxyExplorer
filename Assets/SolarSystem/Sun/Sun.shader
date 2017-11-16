// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "Planets/Sun"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}

		[HDR]_AlbedoMultiplier("Albedo Multiplier", Color) = (1,1,1,1)

		[HDR]_FresnelColor("Fresnel Color", Color) = (1,1,1,1)
		_FresnelTerm("Fresnel (Term, Offset)", Vector) = (0,0,0,0)

		_TintColor("Tint", Color) = (1,1,1,1)

		_AddCycleParams("AddCycle(X, X, Speed, PhaseOffset", vector) = (0,1,1,0)
		_CycleParams("(Add Offset, Add Scale, Mult Offset, Mult Scale)", vector) = (0,0,0,0)

		_TransitionAlpha("TransitionAlpha", Float) = 1
		_SRCBLEND("Source Blend", float) = 1
		_DSTBLEND("Destination Blend", float) = 0
	}

		SubShader
		{
			Tags
			{
				"RenderType" = "Opaque"
				"Queue" = "Geometry"
				"LightMode" = "ForwardBase"
			}

			Pass
			{
				Fog { Mode Off }
				Lighting Off
				Blend [_SRCBLEND] [_DSTBLEND]

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
					float3 normal : NORMAL0;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;

					float2 uv : TEXCOORD0;
					float3 fresnel : TEXCOORD1;
					float timeParameters : TEXCOORD2;
					float  clipAmount : TEXCOORD3;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float3 _AlbedoMultiplier;
				float4 _FresnelTerm;
				float3 _FresnelColor;
				float4 _AddCycleParams;
				float4 _TintColor;
				float _TransitionAlpha;
				float4 _CycleParams;

#define TWOPI ((min16float)6.2831853)
				
//I have forgotten how I arrived at this. (Alex)
#define PRETTYMAGIC ((min16float)(6.6165186333333333333333333333333))

				v2f vert(appdata v)
				{
					v2f o;
					float3 wPos = mul(unity_ObjectToWorld, v.vertex);
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					o.clipAmount = CalcVertClipAmount(wPos);

					float3 normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
					float3 camToPixel = normalize(wPos - _WorldSpaceCameraPos);
					
					// We can do per vertex fresnel because this is a high'ish poly sphere :)
					min16float fresnel = saturate(dot(camToPixel, normal) * _FresnelTerm.x + _FresnelTerm.y);
					fresnel *= fresnel;

					min16float ndotl = -dot(normal, _WorldSpaceCameraPos);
					min16float3 fresnelSideColor = _FresnelColor; 

					o.fresnel.xyz = fresnelSideColor;

					o.fresnel.xyz *= fresnel * _TintColor;

					o.timeParameters =						
						(min16float)_AddCycleParams.w + frac((min16float)_Time.y * (min16float)_AddCycleParams.z) * TWOPI;

					return o;
				}

				min16float4 frag(v2f i) : SV_Target
				{
					min16float4 albedoSpec = tex2D(_MainTex, i.uv);

					min16float3 baseColor = albedoSpec.xyz * (min16float3)_AlbedoMultiplier;

					min16float sinFactor = i.timeParameters.x + albedoSpec.w * PRETTYMAGIC;

					min16float2 cycles;
					cycles.x = sin(sinFactor) * _CycleParams.y + _CycleParams.x;
					cycles.y = cos(sinFactor) * _CycleParams.w + _CycleParams.z;

					min16float4 finalColor = min16float4(baseColor * saturate(cycles.y) + cycles.x * (min16float3)i.fresnel.xyz, (min16float)_TransitionAlpha);

					return ApplyVertClipAmount(finalColor, i.clipAmount);
				}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
