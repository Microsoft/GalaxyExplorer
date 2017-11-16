// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "Planets/PlanetTrail"
{
	Properties
	{
		[HDR] _Color("Color", Color) = (1,1,1,1)
		_MainTex("Falloff Texture", 2D) = "white" {}
		_TransitionAlpha("Transition Alpha", Float) = 1
		_SRCBLEND("Source Blend", float) = 1
		_DSTBLEND("Destination Blend", float) = 1
	}

	SubShader
	{
		Tags
		{ 
			"Queue" = "Transparent" 
			"RenderType" = "Transparent" 
		}
			LOD 100

			ZWrite Off
			Blend[_SRCBLEND][_DSTBLEND]
			Cull Off

			Pass{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog

				#include "UnityCG.cginc"
				#include "/./../../Shaders/cginc/NearClip.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texCoord : TEXCOORD0;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					float2 texCoord : TEXCOORD1;
					float clipAmount : TEXCOORD0;
				};

				float4 _Color;

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float _TransitionAlpha;

				v2f vert(appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.texCoord = v.texCoord;
					float3 wPos = mul(unity_ObjectToWorld, v.vertex);
					o.clipAmount = CalcVertClipAmount(wPos);
					return o;
				}

				fixed4 frag(v2f i) : COLOR
				{			
					float4 col = tex2D(_MainTex, i.texCoord).aaaa;
					
					min16float4 finalColor = col * col.a * _Color * _Color.a * _TransitionAlpha;

					return (fixed4)ApplyVertClipAmount(finalColor, i.clipAmount);
				}
				ENDCG
			}
		}
	FallBack "Diffuse"
}
