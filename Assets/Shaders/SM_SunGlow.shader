// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "SurfaceMapping/SunGlow"
{
	Properties
	{
		[HDR] _Color("Color", Color) = (1,1,1,1)
		_SpecPower("Specular (power, multiplier, offset)", vector) = (30, 1, 0, 0)

		_TransitionAlpha("TransitionAlpha", Float) = 1
		_SRCBLEND("Source Blend", float) = 5
		_DSTBLEND("Destination Blend", float) = 1
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
			Blend[_SRCBLEND][_DSTBLEND]
			ZWrite Off


			CGPROGRAM
			#pragma target 5.0
			#pragma fragmentoption ARB_precision_hint_fastest

            #pragma multi_compile SQUARED_FALLOFF LINEAR_FALLOFF
            #pragma multi_compile FLAT DIFFUSE DIFFUSE_SPEC

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL0;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
			    float4 position: POSITION;
			    float3 normal : NORMAL;
			    float3 dir : TEXCOORD0;
			    float3 pixelDir : TEXCOORD1;
				float2 uv : TEXCOORD2;
			};

			float4 _Color;
			float3 _SunWorldPos;
			float3 _SpecPower;
			float2 _InvRadius;
			float _TransitionAlpha;

			v2f vert(appdata v)
			{
				v2f o;
			    o.position = UnityObjectToClipPos(v.vertex);
			    o.normal = mul(unity_ObjectToWorld, v.normal);
			    float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			    o.dir = _SunWorldPos - worldPos;
			    o.pixelDir = worldPos - _WorldSpaceCameraPos;
				o.uv = v.uv;
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				float edgeFalloff = 1 - (i.uv.x * i.uv.x * i.uv.x);

                #if SQUARED_FALLOFF
                    float falloff = 1 - saturate(dot(i.dir, i.dir) * _InvRadius.y);
                #else
                    float falloff = 1 - saturate(length(i.dir) * _InvRadius.x);
                #endif

                #if FLAT
                    float4 color = _Color;
                #elif DIFFUSE
                    float4 color = _Color * saturate(dot(normalize(i.dir), i.normal));
                #elif DIFFUSE_SPEC
                    min16float3 lightDir = normalize(i.dir);
                    min16float ndotl = dot(i.normal, lightDir);
                    min16float ndoth = saturate(dot(i.normal, normalize(lightDir - normalize(i.pixelDir))));
                    float specular = pow(ndoth, _SpecPower.x) * _SpecPower.y + _SpecPower.z;

                    float4 color = _Color * (saturate(dot(normalize(i.dir), i.normal)) + specular);
                #endif

                float4 finalColor = color * falloff * edgeFalloff;
				return float4(finalColor.xyz, finalColor.a * _TransitionAlpha);
			}

		ENDCG
		}
	}
	FallBack "Diffuse"
}
