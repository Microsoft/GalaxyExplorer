// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "POI_Porthole"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Distance("Distance", float) = 3.0
		_Scale("Scale", float) = 0.1
		_GlowColor("GlowColor", color) = (1,1,1,1)
		_EdgeColor("EdgeColor", color) = (1,1,1,1)
    }

	SubShader
	{
		Tags
		{
			"RenderType"="Opaque"
			"Queue"="Transparent+100"
		}
			
		Cull Back
		Lighting Off

		Pass
		{
		    CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "cginc/NearClip.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
			    float2 texcoord : TEXCOORD0;
			    float4 color : COLOR;
			};

			struct v2f
			{
				min16float4 pos : SV_POSITION;
			    min16float2 uv  : TEXCOORD0;
			    min16float4 color : COLOR;
			    min16float  clipAmount : TEXCOORD2;
			};

			sampler2D _MainTex;
			float _Distance;
			float _Scale;
			float4 _GlowColor;
			float4 _EdgeColor;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);

				float3 localCamPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos,1));
				float3 camToPixel = (v.vertex * float3(1,1,.75) - localCamPos);

		        // flip horizontal (tangent space vs world space)
				camToPixel.x = -camToPixel.x;
		    
		        // intersect with plane
				o.uv = ((v.texcoord - 0.5) + _Distance * (camToPixel.xy / abs(camToPixel.z))) * _Scale + 0.5;
			
				o.color.xyz = _EdgeColor.xyz * v.color.b + _GlowColor.xyz * v.color.g;
				o.color.w = (1 - v.color.g) * v.color.r;

                // calculate clip fade out
				float3 wPos = mul(unity_ObjectToWorld, v.vertex);
				o.clipAmount = CalcVertClipAmount(wPos);

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
			    
			    //return i.color;
			    min16float2 offsetUV = (min16float2)i.uv * 2 - 1;
				min16float fadeoff = saturate(1 - dot(offsetUV, offsetUV));

				// sample texture
				min16float4 color = tex2D(_MainTex, i.uv) * fadeoff;
				color.xyz = i.color.rgb + color.xyz * i.color.w;

				return ApplyVertClipAmount(color, i.clipAmount);
			}
		    ENDCG
		}
	}
}
