// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
#include "UnityCG.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;

float __Size;
half4 __NoiseScale;
half4 __PointPushScale;
half4 __PointColor;
float __Hydrate;

struct VS_INPUT
{
	float4 pos		: POSITION;
	float3 norm		: NORMAL;
	float2 uv		: TEXCOORD0;
	uint   id		: SV_VertexID;
};

struct GS_INPUT
{
	float4 pos		: POSITION;
	float3 normal	: NORMAL;
	float2 uv		: TEXCOORD0;
	float4 col		: COLOR0;
	float  id		: TEXCOORD1;
};

struct FS_INPUT
{
	float4 pos		: POSITION;
	float4 col		: COLOR0;
	float2 uv		: TEXCOORD0;
};

GS_INPUT vert(VS_INPUT input)
{
	float4 worldPos = mul(unity_ObjectToWorld, input.pos);

	GS_INPUT output = (GS_INPUT)0;
	output.uv = TRANSFORM_TEX(input.uv, _MainTex);
	output.normal = input.norm;
	output.pos = input.pos;
	output.col.rgb = (__PointColor.xyz *__PointColor.a) * (1 - __Hydrate);
	output.id = input.id;

	return output;
}

[maxvertexcount(4)]
void geo(triangle GS_INPUT p[3], inout TriangleStream<FS_INPUT> triStream)
{
	float4 pos = p[0].pos;
	float4 noisePos = pos * __NoiseScale.w;

	float3 n = p[0].normal.xyz;
	float noise = ((noisePos.x + n.x) * (noisePos.y + n.y) * (noisePos.z + n.z));
	
	float t = noise * __Hydrate;
	float tt = __PointPushScale.z;
		
	float xd = sin(1.1758 - t * tt + ((5 + noisePos.y) * __NoiseScale.x));
	float yd = sin(5.2 + t - (noisePos.x * __NoiseScale.y));
	float zd = cos(7.3 + t * tt + ((6 - noisePos.y) * __NoiseScale.z));
	
	float nx = t * xd + n.y * t;
	float ny = t * yd;
	float nz = t * zd + n.x * t;

	float3 newpos = float3(nx, ny, nz);
	pos.xyz += newpos;

	float3 mvPos = UnityObjectToViewPos(pos);

	float3 up = float3(0, 1, 0);
	float3 look = mvPos.xyz;
	float3 right = normalize(cross(up, look));

	float halfS = 0.5f * __Size;

	float4 v[4];
	v[0] = mul(UNITY_MATRIX_P, float4(mvPos + halfS * right - halfS * up, 1.0f));
	v[1] = mul(UNITY_MATRIX_P, float4(mvPos + halfS * right + halfS * up, 1.0f));
	v[2] = mul(UNITY_MATRIX_P, float4(mvPos - halfS * right - halfS * up, 1.0f));
	v[3] = mul(UNITY_MATRIX_P, float4(mvPos - halfS * right + halfS * up, 1.0f));

	float i = (p[0].id % 4.0);
	float2 offset = (float2)0;

	[flatten]
	switch (i)
	{
		case 0:
			offset = float2(0, 0);
		break;

		case 1:
			offset = float2(0.5, 0);
		break;

		case 2 :
			offset = float2(0, 0.5);
		break;

		case 3:
			offset = float2(0.5, 0.5);
		break;
	}

	bool shouldRender = ( __Hydrate < 1);

	if (shouldRender)
	{
		FS_INPUT pIn;
		pIn.pos = v[0];
		pIn.uv = float2(0.5f, 0.0f) + offset;
		pIn.col = p[0].col;
		triStream.Append(pIn);

		pIn.pos = v[1];
		pIn.uv = float2(0.5f, 0.5f) + offset;
		pIn.col = p[0].col;
		triStream.Append(pIn);

		pIn.pos = v[2];
		pIn.uv = float2(0.0f, 0.0f) + offset;
		pIn.col = p[0].col;
		triStream.Append(pIn);

		pIn.pos = v[3];
		pIn.uv = float2(0.0f, 0.5f) + offset;
		pIn.col = p[0].col;
		triStream.Append(pIn);

		triStream.RestartStrip();
	}
}

float4 frag(FS_INPUT IN) : COLOR
{
	float4 albedo = tex2D(_MainTex, IN.uv) * IN.col;

	return float4(albedo.xyz, 1);
}
