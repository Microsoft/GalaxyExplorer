// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "Sun/GlowCard"
{
	Properties 
	{
		[HDR] _ColorA ("Color", Color) = (1,1,1,1)
		_ColorAParams("ColorA (Distance, Smoothness, -, -)", vector) = (1,1,0.5,0.5)

		[HDR] _ColorB("Color", Color) = (1,1,1,1)
		_ColorBParams("ColorB (Distance, Smoothness, -, -)", vector) = (1,1,0.5,0.5)

		_ClipRadius("ClipRadius", float) = 0

		_TransitionAlpha("TransitionAlpha", Float) = 1
		_SRCBLEND("Source Blend", float) = 1
		_DSTBLEND("Destination Blend", float) = 1
	}
	
	SubShader 
	{
		Tags { "RenderType"="Transparent" "Queue"="Geometry+100" }
		Blend [_SRCBLEND] [_DSTBLEND]
		LOD 200
		ZWrite Off
		
		Pass
		{
			CGPROGRAM
			#pragma target 5.0
			#pragma fragmentoption ARB_precision_hint_fastest

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "/./../../Shaders/cginc/NearClip.cginc"

			#undef _CLIP_BIAS
			#define _CLIP_BIAS ((min16float)-0.1)

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 wPos : TEXCOORD1;
			};

			float4 _ColorA;
			float4 _ColorAParams;

			float4 _ColorB;
			float4 _ColorBParams;

			float _ClipRadius;

			float _TransitionAlpha;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv - (min16float).5;
				o.wPos = mul(unity_ObjectToWorld, v.vertex);

				return o;
			}

			min16float CircularGradiant(min16float2 toEdge, min16float dist, min16float smoothness)
			{			
				min16float x = saturate(((min16float)1.0 - toEdge * dist) * smoothness);
				return x*x*((min16float)2.0); // rough approximation of smoothstep(0, 1, x);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				min16float distToEdge = length(i.uv);
				
				if (distToEdge < _ClipRadius) 
					discard;

				min16float4 col = (min16float4)_ColorA * CircularGradiant(distToEdge, _ColorAParams.x, _ColorAParams.y);

				col += (min16float4)_ColorB * CircularGradiant(distToEdge, _ColorBParams.x, _ColorBParams.y);

				min16float alpha = dot(col, (min16float)0.33333);

				min16float4 finalColor = min16float4(col.xyz, alpha * (min16float)_TransitionAlpha);

				PixelFromCamera pfc = (PixelFromCamera)0;
				CalcPixelFromCamera(i.wPos, pfc);

				//Most of the shaders use the vertex version of this, but the glow card verts
				//are not in the right place for this - you are much closer to the center of the glowcard
				//than its corners, so it doesn't actually fade out when you look at the sun unless
				//this calculation is done per pixel!
				return ApplyNearClip(finalColor, pfc);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
