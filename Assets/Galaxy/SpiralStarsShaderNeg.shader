// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "Galaxy/StarsNeg"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_WSScale ("World Space Scale", Float) = 1
		_TxScale ("TX Scale", Vector) = (0,0,0,0)
		[HDR] _Color ("Color", Color) = (1,1,1,1)
		_TransitionAlpha("Transition Alpha", Float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100


		Pass
		{
			Blend One OneMinusSrcAlpha
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geo
			
			#include "UnityCG.cginc"
			#include "StarVertDescriptor.cginc"
			#include "StarPositionCompute.cginc"

			struct v2g
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float size : TEXCOORD1;
				float4 color : COLOR0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float _WSScale;
			float4 _TxScale;

			float3 _LocalCamDir;

			float4 _Color;
			float _TransitionAlpha;

			StructuredBuffer<StarVertDescriptor> _Stars;
			float4x4 _GalaxyWorld;

			v2g vert (uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				StarVertDescriptor star = _Stars[id];

				v2g o;

				float3 pos = float4(ComputeStarPosition(star), 1);
				float fade = saturate(dot(normalize(pos), _LocalCamDir));

				o.vertex = mul(_GalaxyWorld, float4(pos, 1));
				o.color = float4(star.color, 1) * fade * _TransitionAlpha;
				o.uv = star.uv;
				o.size = star.size;

				return o;
			}

			[maxvertexcount(4)]
			void geo(point v2g p[1], inout TriangleStream<v2f> triStream)
			{
				float4 pos = p[0].vertex;
				float3 mvPos = UnityObjectToViewPos(pos);

				float3 up = float3(0, 1, 0);
				float3 look = mvPos.xyz;
				float3 right = normalize(cross(up, look));

				float halfS = p[0].size * _WSScale;

				if (halfS > 0)
				{
					float4 v[4];
					v[0] = mul(UNITY_MATRIX_P, float4(mvPos - halfS * up, 1.0f));
					v[1] = mul(UNITY_MATRIX_P, float4(mvPos + halfS * right, 1.0f));
					v[2] = mul(UNITY_MATRIX_P, float4(mvPos - halfS * right, 1.0f));
					v[3] = mul(UNITY_MATRIX_P, float4(mvPos + halfS * up, 1.0f));

					v2f pIn;
					pIn.color = p[0].color;

					pIn.vertex = v[0];
					pIn.uv = float2(1.0f, 0.0f) * 0.5 + p[0].uv;
					triStream.Append(pIn);

					pIn.vertex = v[1];
					pIn.uv = float2(1.0f, 1.0f) * 0.5 + p[0].uv;
					triStream.Append(pIn);

					pIn.vertex = v[2];
					pIn.uv = float2(0.0f, 0.0f) * 0.5 + p[0].uv;
					triStream.Append(pIn);

					pIn.vertex = v[3];
					pIn.uv = float2(0.0f, 1.0f) * 0.5 + p[0].uv;
					triStream.Append(pIn);

					triStream.RestartStrip();
				}
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 color = tex2D(_MainTex, i.uv + float2(0,.5)).a * i.color;
				color.a = dot(color.xyz, 1) / 3.0;

				clip(color.a - 0.05);

				return color * _Color;
			}
			ENDCG
		}
	}
}