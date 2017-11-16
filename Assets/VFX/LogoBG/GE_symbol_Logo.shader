// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "GE_symbol_Logo"
{
    Properties
    {
        _Cubemap("Cubemap", Cube) = "" {}
        _CubemapContribution("Cubemap Contribution",  Range(0,2)) = 1
        _CubemapRoughness("Cubemap Blur (LOD min, LOD max)", vector) = (0,10,0,0)
        _CubemapBalance("Cubemap Balance (exponent, offset)", vector) = (1,0,0,0)
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 100

            Pass
            {
                CGPROGRAM
                #pragma target 5.0
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float3 normal : NORMAL;
                    float3 pixelFromCamera : TEXCOORD0;
                };

                float _CubemapContribution;
                fixed2 _CubemapRoughness;
                fixed2 _CubemapBalance;
                samplerCUBE _Cubemap;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.normal = mul((float3x3)unity_ObjectToWorld, v.normal);

                    float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
                    float3 posFromCamera = worldPos - _WorldSpaceCameraPos;
                    o.pixelFromCamera = posFromCamera;

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    min16float3 worldReflection = reflect(normalize(i.pixelFromCamera), normalize(i.normal));
                    min16float roughness = _CubemapRoughness.x;
                    min16float3 cubemapColor = texCUBElod(_Cubemap, min16float4(worldReflection, roughness)) *_CubemapBalance.x + _CubemapBalance.y;

                    return min16float4(lerp(1, cubemapColor, _CubemapContribution),1);
                }
                ENDCG
            }
        }
}
