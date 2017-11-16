// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "Custom/EdgeShader"
{
	Properties
	{
		[HDR] _Color("Color", Color) = (.5, .5, .5, .5)
		_TransitionAlpha("Transition Alpha", Float) = 1
	}

	SubShader
	{
		Tags{ "Queue" = "Geometry+100" "RenderType" = "Transparent" }
		Cull Back 
		Blend SrcAlpha OneMinusSrcAlpha
		//Offset 1, 1

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			//#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			float4 _Color;

			struct a2v
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL0;
				float3 tangent : TANGENT0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 fresnel : TEXCOORD3;
			};

			float _TransitionAlpha;

			v2f vert(a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				
				float3 normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
				float3 tangent = normalize(mul((float3x3)unity_ObjectToWorld, v.tangent));
				float3 bitangent = cross(normal, tangent);
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

				float3 posFromCamera = normalize(worldPos - _WorldSpaceCameraPos);
				float alongBiTan = dot(posFromCamera, bitangent);
				posFromCamera -= alongBiTan * bitangent;
				posFromCamera = normalize(posFromCamera);

				float posAlongNormal = -dot(normal, posFromCamera);
				float fresnel = posAlongNormal;

				o.fresnel = fresnel * _Color;

				return o;
			}

			min16float4 frag(v2f i) : SV_Target
			{
				return min16float4(i.fresnel.xyz, (min16float)_TransitionAlpha);
			}
			ENDCG
		}
	}
	Fallback Off
}