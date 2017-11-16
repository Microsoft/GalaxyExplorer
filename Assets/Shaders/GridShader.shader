// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "Playspace/GridShader"
{
	Properties
	{
		_LineScale("LineScale", Float) = 0.1
		_LinesPerMeter("LinesPerMeter", Float) = 4
		_TransitionAlpha("Transition Alpha", Range(0,1)) = 1
		_Color("Color", Color) = (0,0,1,0)
		[HideInInspector]_WorldScale("WorldScale", Vector) = (3.0, 3.0, 1.0, 0)
		[Toggle] _DrawGrid("DrawGrid", Range(0,1)) = 1
		[Toggle] _DrawBorder("DrawBorder", Range(0,1)) = 1
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Background+1"
			"PreviewType" = "Plane"
		}

		Blend One One
		ZTest Off
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "../Shaders/cginc/NearClip.cginc"

			// These values map from the properties block at the beginning of the shader file.
			// They can be set at run time using renderer.material.SetFloat()
			float _LineScale;
			float _LinesPerMeter;
			float _TransitionAlpha;
			float4 _Color;
			float4 _WorldScale;
			int _DrawGrid;
			int _DrawBorder;
			
			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 color : COLOR;
			};
			// This is the data structure that the vertex program provides to the fragment program.
			struct VertToFrag
			{
				float4 viewPos : SV_POSITION;
				float4 uv : TEXCOORD0;
				float4 objectPos: TEXCOORD1;
				fixed4 color : COLOR;
				float clipAmount : TEXCOORD2;
			};


			// This is the vertex program.
			VertToFrag vert (appdata v)
			{
				VertToFrag o;

				// Calculate where the vertex is in view space.
				o.viewPos = UnityObjectToClipPos(v.vertex);

				// Pass along the position in object local space
				o.objectPos = v.vertex;

				// Pass along the uv.
				o.uv = v.uv;

				o.clipAmount = CalcVertClipAmount(mul(unity_ObjectToWorld, v.vertex));

				o.color = v.color * _Color;

				return o;
			}

			float CalculateMins(float4 wpmod, float scaledLinesPerMeter)
			{
				float min1 = min(abs((wpmod.x) / (scaledLinesPerMeter)), abs((wpmod.y) / (scaledLinesPerMeter)));
				float min2 = min(((1 - abs(wpmod.x)) / (scaledLinesPerMeter)), ((1 - abs(wpmod.y)) / (scaledLinesPerMeter)));
				return min(min1, min2);
			}

			half4 frag (VertToFrag input) : SV_Target
			{
				float4 wpmod;
				float4 wpmodip; // this get the integer part of the modf operation
				float scaledLinesPerMeter;
				float fact = 0;

				// Draw the grid
				if (_DrawGrid)
				{
					// wpmod is documented on the internet, it's basically a
					// floating point mod function.
					wpmod = modf((_WorldScale * input.objectPos) * _LinesPerMeter, wpmodip);
					scaledLinesPerMeter = _LineScale * _LinesPerMeter;

					if ((abs(wpmod.x) < scaledLinesPerMeter) || (1 - abs(wpmod.x) < scaledLinesPerMeter) ||
						(abs(wpmod.y) < scaledLinesPerMeter) || (1 - abs(wpmod.y) < scaledLinesPerMeter))
					{
						fact = CalculateMins(wpmod, scaledLinesPerMeter);
					}
				}
				// draw the border
				if (_DrawBorder)
				{
					scaledLinesPerMeter = _LineScale * (1 / _LinesPerMeter);
					float4 offsets = (
						input.uv.x / abs(input.uv.x),
						input.uv.y / abs(input.uv.y),
						input.uv.z / abs(input.uv.z),
						input.uv.w / abs(input.uv.w)) * (scaledLinesPerMeter / 2);

					// wpmod is documented on the internet, it's basically a
					// floating point mod function.
					wpmod = modf((input.uv + offsets), wpmodip);

					if ((abs(wpmod.x) < scaledLinesPerMeter) || (1 - abs(wpmod.x) < scaledLinesPerMeter) ||
						(abs(wpmod.y) < scaledLinesPerMeter) || (1 - abs(wpmod.y) < scaledLinesPerMeter))
					{
						if (fact == 0)
						{
							fact = CalculateMins(wpmod, scaledLinesPerMeter);
						}
						else
						{
							fact = min(fact, CalculateMins(wpmod, scaledLinesPerMeter));
						}
					}

					// wpmod is documented on the internet, it's basically a
					// floating point mod function.
					wpmod = modf((input.uv - offsets), wpmodip);

					if ((abs(wpmod.x) < scaledLinesPerMeter) || (1 - abs(wpmod.x) < scaledLinesPerMeter) ||
						(abs(wpmod.y) < scaledLinesPerMeter) || (1 - abs(wpmod.y) < scaledLinesPerMeter))
					{
						if (fact == 0)
						{
							fact = CalculateMins(wpmod, scaledLinesPerMeter);
						}
						else
						{
							fact = min(fact, CalculateMins(wpmod, scaledLinesPerMeter));
						}
					}
				}

				// Initialize to draw transparent-black (zero alpha). This way the
				// blackness between the grid-lines won't occlude what is beyond
				// the grid.
				half4 ret = half4(0,0,0,0);

				if (fact != 0)
				{
					ret.r = (_Color.r - fact);
					ret.g = (_Color.g - fact);
					ret.b = (_Color.b - fact);
					// the next line has proven useful for debugging this shader
					// ret = half4(input.uv.x - fact, input.uv.y - fact, input.uv.z - fact, 0);
				}

				return ret * _TransitionAlpha;
			}
			ENDCG
		}
	}
}