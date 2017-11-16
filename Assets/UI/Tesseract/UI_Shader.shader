// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "UI/Button"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Tint("Tint", Color) = (1,1,1,1)

		_TransitionAlpha("Transition Alpha", Float) = 1
		_SRCBLEND("Source Blend", float) = 1
		_DSTBLEND("Destination Blend", float) = 0
		_ZWRITE ("Z Write", float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Geometry+100" }
		Pass
		{
			Blend[_SRCBLEND][_DSTBLEND]
			ZWrite [_ZWRITE]

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _Tint;
			float _TransitionAlpha;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return tex2D(_MainTex, i.uv) * _TransitionAlpha;
			}
			ENDCG
		}
	}
}
