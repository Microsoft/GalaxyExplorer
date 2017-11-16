// Simple transparent shader.

Shader "Custom/UITransparent" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "black" {}
	_Tint("Color Tint", Color) = (1.0,1.0,1.0,1.0)
}

SubShader {
	Tags{ "Queue" = "Transparent" }
	Pass{
		Cull Off
		LOD 100
		ZWrite Off
		Lighting Off
		Blend One One
		SetTexture[_MainTex]{ combine texture }
	}
}
}