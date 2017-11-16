// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "UI/Cursor"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Alpha ("Alpha", Float) = 1

		_DefaultTex ("Base Texture", 2D) = "white"
		_MainTex ("Main Texture", 2D) = "white"
		_SecondTex ("Second Texture", 2D) = "white"

		_BaseRatio ("Base Texture Ratio", Float) = 1
		_MainSecondRatio ("Main Second Ratio", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Overlay"}
		Blend SrcAlpha OneMinusSrcAlpha
		ZTest Always
		ZWrite Off
		Cull Off

		Pass
		{
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
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _SecondTex;
			float4 _SecondTex_ST;

			sampler2D _DefaultTex;
			float4 _DefaultTex_ST;

			float4 _Color;
			float _Alpha;

			float _MainSecondRatio;
			float _BaseRatio;

			#pragma multi_compile  __ TRANSITION_ON
			
			v2f vert (appdata v)
			{
				v2f o;
				
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			min16float4 frag (v2f i) : SV_Target
			{
				min16float baseOpacity;

#ifndef TRANSITION_ON
				baseOpacity = tex2D(_MainTex, i.uv).a;
#else
				min16float mainSide = tex2D(_MainTex, i.uv).a;
				min16float secondSide = tex2D(_SecondTex, i.uv).a;

				baseOpacity = lerp(mainSide, secondSide, _MainSecondRatio);
#endif

				float defaultOpacity = tex2D(_DefaultTex, i.uv).a;

				min16float4 color = lerp(defaultOpacity, baseOpacity, _BaseRatio) * _Color * _Alpha;

				return color;
			}
			ENDCG
		}
	}
}
