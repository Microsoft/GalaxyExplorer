// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "Custom/SurfaceCubemap"
{
	Properties
	{
		[HDR] _Tint("Tint Color", Color) = (.5, .5, .5, .5)
		[Gamma] _Exposure("Exposure", Range(0, 8)) = 1.0
		_Rotation("Rotation", Range(0, 360)) = 0
		[NoScaleOffset] _Tex("Cubemap   (HDR)", Cube) = "grey" {}
		_TransitionAlpha("TransitionAlpha", Float) = 1
	}

		SubShader
	{
		Tags{ "Queue" = "Geometry+50" "IgnoreProjector" = "True" "RenderType" = "Opaque" "PreviewType" = "Skybox" }
		Cull Back ZWrite Off
		Offset 1, 1

		Pass
		{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			samplerCUBE _Tex;
			half4 _Tex_HDR;
			half4 _Tint;
			half _Exposure;
			float _Rotation;
			float3 _Color;
			float _TransitionAlpha;

			struct appdata_t 
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f 
			{
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float3 wPos : TEXCOORD1;
				float3 dir : TEXCOORD2;
				float4 tint : COLOR0;
			};

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.texcoord = v.uv;

				float cosT = cos(_Rotation * UNITY_PI / 180.0);
				float sinT = sin(_Rotation * UNITY_PI / 180.0);

				min16float3 toCam = _WorldSpaceCameraPos - o.wPos.xyz;
				min16float3 dir = toCam;

				min16float nx, nz;
				nx = dir.x * cosT - dir.z * sinT;
				nz = dir.x * sinT + dir.z * cosT;

				dir.x = nx;
				dir.z = nz;

				o.tint = float4(_Tint.xyz * unity_ColorSpaceDouble.rgb * _Exposure, 1.0) * _TransitionAlpha;
				o.dir = dir;

				return o;
			}

			min16float4 frag(v2f i) : SV_Target
			{
				return min16float4(texCUBE(_Tex, i.dir) * (min16float4)i.tint);
			}
			ENDCG
		}
	}
	Fallback Off
}