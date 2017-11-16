// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
#include "UnityCG.cginc"

struct VS_INPUT
{
	float4 Pos		: POSITION;
	float3 Norm		: NORMAL;
	float2 Tex		: TEXCOORD0;
};


struct FS_INPUT
{
	float4	Pos		: POSITION;
	float3	Norm	: NORMAL;
	float4	Col		: COLOR0;
	float2  Tex		: TEXCOORD0;
	float2	ShouldClip : COLOR1;
};


half4 __FillColor;

sampler2D _ColorTex;
float4 _ColorTex_ST;

FS_INPUT vert(VS_INPUT input)
{
	FS_INPUT output = (FS_INPUT)0;
	output.Col.xyz = (__FillColor.xyz * __FillColor.a);
	output.Norm = normalize(mul((float3x3)unity_ObjectToWorld, input.Norm));
	output.Pos = UnityObjectToClipPos(input.Pos);
	output.Tex = input.Tex;
	output.ShouldClip.x = (__FillColor.r > 0 && __FillColor.g > 0 && __FillColor.b > 0 ) ? 1 : -1;
	
	return output;
}


float4 frag(FS_INPUT IN) : COLOR
{
	clip(IN.ShouldClip.x);
	
	float3 finalColor = tex2D(_ColorTex, IN.Tex).xyz * IN.Col.xyz;
	return float4(finalColor, 1);
}
