// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "Asteroids/Ring"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TransitionAlpha("TransitionAlpha", Float) = 1
		_LightPosition("Light Position", Vector) = (0,0,0,0)

		[HDR] _LightColor("Light Color", Color) = (1,1,1,1)
		[HDR] _AmbientColor("Light Ambient", Color) = (0,0,0,0)

		_SRCBLEND("Source Blend", float) = 1
		_DSTBLEND("Destination Blend", float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		ZWrite On
		Blend [_SRCBLEND] [_DSTBLEND]

		Pass
		{
			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "/./../../Shaders/cginc/NearClip.cginc"

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
				float2 uv : TEXCOORD0;
				float3 lightToCenter : TEXCOORD1;
				float clipAmount: TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float3 _LightPosition;
			float4 _LightColor;
			float4 _AmbientColor;
			float _TransitionAlpha;

			v2f vert(appdata v)
			{
				float3 wPos = mul(unity_ObjectToWorld, v.vertex);
				
				float3 lightToCenter = normalize(wPos - (float3)_LightPosition);

				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.normal = mul((float3x3)unity_ObjectToWorld, v.normal);
				o.lightToCenter = lightToCenter;
				o.clipAmount = CalcVertClipAmount(wPos);

				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				min16float4 col = tex2D(_MainTex, i.uv);
				min16float4 finalColor = min16float4(_LightColor.xyz * saturate(dot(i.lightToCenter, -normalize(i.normal)).xxx) * col.xyz + _AmbientColor, _TransitionAlpha);

				return ApplyVertClipAmount(finalColor, i.clipAmount);
			}
			ENDCG
		}
	}
}
