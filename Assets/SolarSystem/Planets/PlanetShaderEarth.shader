// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "Planets/Earth"
{
	Properties
	{
		[HDR] _NightLightColor ( "Night Light Color", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_NormalAlpha("Normal Texture", 2D) = "white" {}
		_SunDirection("Sun Direction", Vector) = (1,1,1,0)
		_LightAmount("Light amount (LightSide)", Vector) = (1,.2, 0, 0)
		[HDR]_LightTint("Sunlight Tint", Color) = (1,1,1,1)
		[HDR]_AmbientColor("Ambient Color", Color) = (1,1,1,1)
		[HDR]_AlbedoMultiplier("Albedo Multiplier", Color) = (1,1,1,1)

		[HDR]_FresnelColor("Fresnel Color", Color) = (1,1,1,1)
		[HDR]_FresnelDarkSideColor("Fresnel Dark Side Color", Color) = (1,1,1,1)
		_FresnelTerm("Fresnel (Term, Offset)", Vector) = (0,0,0,0)

		_SpecParams("Specular (Power, Offset, Mask Multiplier)", Vector) = (60,0,0,0)
		_NormalScale("Normal Scale", Vector) = (1,1,1,1)

		_LightGammaCorrection("Light Gamma Correction (Multiplier, Power)", Vector) = (1,1,1,1)

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
				#pragma multi_compile _ LOD_FADE_CROSSFADE

				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				#include "/./../../Shaders/cginc/NearClip.cginc"

				struct appdata
				{
					float4 vertex : POSITION;

					float3 normal : NORMAL0;
					float2 uv : TEXCOORD0;
					float4 tangent : TANGENT0;
				};

				struct v2f
				{
					float4 uv : TEXCOORD0;
					float  clipAmount : TEXCOORD1;

					float4 vertex : SV_POSITION;

					float3 normal : NORMAL0;
					float3 tangent : TANGENT0;
					float3 binormal : TANGENT1;

					float3 camToPixel : TEXCOORD2;
					float3 camToSunDirection : TEXCOORD3;

					float4 fresnel : TEXCOORD4;

					float specAmount : COLOR0;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;

				sampler2D _NormalAlpha;
				float4 _NormalAlpha_ST;

				float4 _SunDirection;
				float4 _LightAmount;
				float4 _NightLightColor;

				float3 _AmbientColor;
				float3 _AlbedoMultiplier;

				float4 _FresnelTerm;

				float3 _FresnelColor;
				float3 _FresnelDarkSideColor;

				float3 _SpecParams;
				float3 _NormalScale;

				float4 _LightGammaCorrection;
				float _TransitionAlpha;
				float3 _LightTint;

				v2f vert(appdata v)
				{
					v2f o;
					float3 wPos = mul(unity_ObjectToWorld, v.vertex);
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = float4(TRANSFORM_TEX(v.uv, _NormalAlpha), TRANSFORM_TEX(v.uv, _MainTex));
					o.clipAmount = CalcVertClipAmount(wPos);

					o.normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
					o.tangent = normalize(mul((float3x3)unity_ObjectToWorld, v.tangent.xyz));
					o.binormal = normalize(cross(o.normal, o.tangent) * v.tangent.w);

					o.camToPixel = normalize(wPos - _WorldSpaceCameraPos);
					o.camToSunDirection = normalize(_SunDirection.xyz - o.camToPixel);

					// We can do per vertex fresnel because this is a high'ish poly shpere :)
					min16float fresnel = saturate(dot(o.camToPixel, o.normal) * _FresnelTerm.x + _FresnelTerm.y);
					fresnel *= fresnel;

					o.fresnel.a = fresnel;

					min16float ndotl = -dot(o.normal, _SunDirection);
					min16float3 fresnelSideColor = lerp(_FresnelColor, _FresnelDarkSideColor, ndotl * .5f + .5f);

					o.fresnel.xyz = fresnelSideColor;

					min16float ndoth = saturate(dot(o.normal, o.camToSunDirection));
					o.specAmount = (pow(ndoth, (min16float)_SpecParams.x) + (min16float)_SpecParams.y) * (min16float)_SpecParams.z;

					return o;
				}

				min16float4 frag(v2f i) : SV_Target
				{
					min16float3 camToPixel = i.camToPixel;
					min16float3 camToSunDirection = i.camToSunDirection;

					min16float4 albedoSpec = tex2D(_MainTex, i.uv.zw);
					min16float4 normalAlpha = tex2D(_NormalAlpha, i.uv.xy);

					min16float3 textureNormal = (normalAlpha.xyz * (min16float)2.0f - (min16float)1.0f) * (min16float3)_NormalScale;

					// Effectively the same as:
					//	min16float3 tn = mul(textureNormal, min16float3x3(i.tangent, i.binormal, i.normal));
					// but forces the right thing (mads) rather than a ton of movs & dp3s
					min16float3 tn = textureNormal.xxx * (min16float3)i.tangent;
					tn += textureNormal.yyy * (min16float3)i.binormal;
					tn += textureNormal.zzz * (min16float3)i.normal;
					min16float3 worldNormal = normalize(tn);

					min16float ndotl = saturate(dot(worldNormal, (min16float3)_SunDirection.xyz));

					min16float gammaIntensity = pow(ndotl * _LightGammaCorrection.x, _LightGammaCorrection.y);

					min16float3 lightAmount = gammaIntensity * lerp((min16float3)_LightAmount.x, 0, ndotl * (min16float).5 - (min16float).5) * _LightTint;
					lightAmount = (max(0, lightAmount) + (min16float3)_AmbientColor.xyz);


					min16float3 baseColor = albedoSpec.xyz * (min16float3)_AlbedoMultiplier * lightAmount;
					min16float3 lightsColor = saturate(normalAlpha.a * (min16float3)_NightLightColor * ((min16float)1.0f - lightAmount));

					min16float fresnel = i.fresnel.a;
					
#if LOD_FADE_CROSSFADE
					fresnel *= (min16float)unity_LODFade.x;
#endif
					min16float3 fresnelSideColor = i.fresnel.xyz;

					min16float specAmount = i.specAmount * albedoSpec.a;
					
					min16float4 finalColor = min16float4(specAmount.xxx + lerp(baseColor + lightsColor, fresnelSideColor, fresnel), (min16float)_TransitionAlpha);

					return ApplyVertClipAmount(finalColor, i.clipAmount);
				}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
