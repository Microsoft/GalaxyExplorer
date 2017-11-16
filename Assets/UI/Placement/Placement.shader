// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "Placement"
{
	Properties
	{
	    _RadiusStart("RadiusStart", float) = 0.001
	    _RadiusEnd("RadiusEnd", float) = 0.005
	    _LineStart("LineStart", float) = 0.9
	    _LineEnd("LineEnd", float) = 1.0
	    _LocalSpaceSunDir("_LocalSpaceSunDir", vector) = (1,0,0,0)
	    [HDR] _TintStart("TintStart", Color) = (1,1,1,1)
	    [HDR] _TintEnd("TintEnd", Color) = (1,1,1,1)
	    [HDR] _TintEndDark("TintEndDark", Color) = (1,1,1,1)
        _BackClip("_BackClip", Range(-2,1)) = -1
	    _BackScale("_BackClip", float) = 1
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}

		Pass
		{
			Fog { Mode Off }
			Lighting Off
            Blend One One
            ZTest Off
            ZWrite Off

			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			    float3 normal : NORMAL0;
			};

			struct v2f
			{
			    float4 vertex : SV_POSITION;
			    float3 normal : NORMAL0;
			    float3 pos : TEXCOORD1;
			    float4 tintEnd : TEXCOORD2;
			    float diffuse : TEXCOORD3;
			    float fade : TEXCOORD4;
			};
            
			struct Segment
			{
			    float3 P0;
			    float3 P1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float3 _LocalSpaceCameraPos;
			float3 _LocalSpaceSunDir;
			float _RadiusStart;
			float _RadiusEnd;
			float _LineStart;
			float _LineEnd;
			float4 _TintStart;
			float4 _TintEnd;
			float4 _TintEndDark;
			float _BackClip;
			float _BackScale;
            
			#define Vector float3
            #define SMALL_NUM 0.000001

			//===================================================================
			float dist3D_Segment_to_Segment(Segment S1, Segment S2, out float t)
			{
				Vector   u = S1.P1 - S1.P0;
				Vector   v = S2.P1 - S2.P0;
				Vector   w = S1.P0 - S2.P0;
				float    a = dot(u, u);         // always >= 0
				float    b = dot(u, v);
				float    c = dot(v, v);         // always >= 0
				float    d = dot(u, w);
				float    e = dot(v, w);
				float    D = a*c - b*b;        // always >= 0
				float    sc, sN, sD = D;       // sc = sN / sD, default sD = D >= 0
				float    tc, tN, tD = D;       // tc = tN / tD, default tD = D >= 0

												// compute the line parameters of the two closest points
				//if (D < SMALL_NUM) { // the lines are almost parallel
				//	sN = 0.0;         // force using point P0 on segment S1
				//	sD = 1.0;         // to prevent possible division by 0.0 later
				//	tN = e;
				//	tD = c;
				//}
				//else
				{                 // get the closest points on the infinite lines
					sN = (b*e - c*d);
					tN = (a*e - b*d);
					if (sN < 0.0) {        // sc < 0 => the s=0 edge is visible
						sN = 0.0;
						tN = e;
						tD = c;
					}
					else if (sN > sD) {  // sc > 1  => the s=1 edge is visible
						sN = sD;
						tN = e + b;
						tD = c;
					}
				}

				if (tN < 0.0) {            // tc < 0 => the t=0 edge is visible
					tN = 0.0;
					// recompute sc for this edge
					if (-d < 0.0)
						sN = 0.0;
					else if (-d > a)
						sN = sD;
					else {
						sN = -d;
						sD = a;
					}
				}
				else if (tN > tD) {      // tc > 1  => the t=1 edge is visible
					tN = tD;
					// recompute sc for this edge
					if ((-d + b) < 0.0)
						sN = 0;
					else if ((-d + b) > a)
						sN = sD;
					else {
						sN = (-d + b);
						sD = a;
					}
				}

				// finally do the division to get sc and tc
				sc = sN / sD;//(abs(sN) < SMALL_NUM ? 0.0 : sN / sD);
				tc = tN / tD;//(abs(tN) < SMALL_NUM ? 0.0 : tN / tD);

				// get the difference of the two closest points
				Vector dP = w + (sc * u) - (tc * v);  // =  S1(sc) - S2(tc)

				t = tc;

				return dot(dP,dP);   // return the sqrd closest distance
			}

			v2f vert(appdata v)
			{
				v2f o;

			    min16float3 camToPixel = (v.vertex - _LocalSpaceCameraPos);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.pos = camToPixel * 2 + _LocalSpaceCameraPos;
				o.normal = v.normal;
				
				o.diffuse = -dot(o.normal, _LocalSpaceSunDir);
				o.tintEnd = lerp(_TintEnd, _TintEndDark, o.diffuse * .5 + .5);
			    
				float ndotv = -dot(normalize(camToPixel), v.normal);
				o.fade = abs(ndotv) * saturate(_BackScale * ndotv - _BackClip);
				
				return o;
			}

			min16float4 frag(v2f i) : SV_Target
			{
			    Segment segView;
			    Segment segLine;
			    segView.P0 = _LocalSpaceCameraPos;
			    segView.P1 = i.pos;
                
			    segLine.P0 = i.normal * _LineStart;
			    segLine.P1 = i.normal * _LineEnd;

			    min16float t;
			    min16float distanceToCenter = dist3D_Segment_to_Segment(segView, segLine, t);
			    
			    min16float radius = lerp(_RadiusStart, _RadiusEnd, t);
			    min16float4 color = lerp(_TintStart, i.tintEnd, t);
			    
			    min16float luminance = 1 - (distanceToCenter / (radius*radius));
			    return luminance * color * i.fade;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
