// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "Galaxy/ScreenClear"
{
	SubShader 
	{ 
		// Normal pass
		Pass 
		{
 			ZTest Always Cull Off
			ZWrite On
			Blend One Zero

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.vertex.z = 1;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return 0;
			}
			ENDCG 

		}
	}
	Fallback Off 
}