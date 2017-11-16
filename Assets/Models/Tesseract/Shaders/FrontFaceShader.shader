// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "Custom/FrontFaceShader"
{
	Properties
	{
		[HDR] _Color("Color", Color) = (.5, .5, .5, .5)
		_GradiantParams("Gradiant (uCenter, vCenter, radius, smoothness)", vector) = (0.5,0.5,1, 1)
		_DotParams("DotParams (spacing, scale, -, -)", vector) = (1,1,1, 1)
		[HDR] _DotColor("DotColor", Color) = (1,1,1,1)
		_TransitionAlpha("Transition Alpha", Float) = 1
	}

	SubShader
	{
		Tags{ "Queue" = "Geometry+280" "RenderType" = "Opaque" }
		Cull Back 
		ZWrite Off
		Blend One One
		//Offset 1, 1

		Pass
		{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			float4 _Color;
			float4 _DotColor;
			float4 _GradiantParams;
			float4 _DotParams;
			float _TransitionAlpha;

			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float gradiant : TEXCOORD1;
			};

			float CircularGradiant(float2 uv, float2 center, float dist, float smoothness)
			{
				return smoothstep(0, smoothness, (1 - distance(uv.xy, center.xy) * dist));
			}

			v2f vert(a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				o.gradiant = saturate(1 - CircularGradiant(o.uv, _GradiantParams.xy, _GradiantParams.z, _GradiantParams.w));

				return o;
			}

			min16float4 frag(v2f i) : SV_Target
			{
				min16float su = frac(i.uv.x * _DotParams.x);
				min16float sv = frac(i.uv.y * _DotParams.x);
				
				bool sup = (su > 1.0 - _DotParams.y || su < _DotParams.y) && (i.uv.x > 0.01);
				bool svp = (sv > 1.0 - _DotParams.y || sv < _DotParams.y) && (i.uv.y > 0.01);

				float3 myDots = (sup && svp) * i.gradiant * _DotColor;

				float3 finalColor = _Color.xyz * i.gradiant + myDots;

				//Remember to return alpha even if it's additive so BEV works
				return min16float4(finalColor.xyz, i.gradiant) * (min16float)_TransitionAlpha;
			}
			ENDCG
		}
	}
	Fallback Off
}