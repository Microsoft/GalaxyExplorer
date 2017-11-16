// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:1,x:34682,y:32637,varname:node_1,prsc:2|emission-44-OUT;n:type:ShaderForge.SFN_Tex2d,id:2,x:32591,y:32305,ptovrint:False,ptlb:REF,ptin:_REF,varname:node_7363,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-4-OUT;n:type:ShaderForge.SFN_ViewReflectionVector,id:3,x:32224,y:32274,varname:node_3,prsc:2;n:type:ShaderForge.SFN_ComponentMask,id:4,x:32350,y:32274,varname:node_4,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-3-OUT;n:type:ShaderForge.SFN_Multiply,id:7,x:33612,y:32936,varname:node_7,prsc:2|A-28-OUT,B-31-OUT;n:type:ShaderForge.SFN_Power,id:12,x:34526,y:33137,varname:node_12,prsc:2|EXP-13-OUT;n:type:ShaderForge.SFN_Vector1,id:13,x:34385,y:33219,varname:node_13,prsc:2,v1:10;n:type:ShaderForge.SFN_Clamp01,id:15,x:34142,y:32840,varname:node_15,prsc:2|IN-29-OUT;n:type:ShaderForge.SFN_Sin,id:16,x:33764,y:32904,varname:node_16,prsc:2|IN-7-OUT;n:type:ShaderForge.SFN_TexCoord,id:26,x:32558,y:32720,varname:node_26,prsc:2,uv:1;n:type:ShaderForge.SFN_Vector1,id:27,x:33374,y:33047,varname:node_27,prsc:2,v1:6;n:type:ShaderForge.SFN_Add,id:28,x:33416,y:32796,varname:node_28,prsc:2|A-43-OUT,B-30-OUT;n:type:ShaderForge.SFN_Power,id:29,x:34067,y:32976,varname:node_29,prsc:2|VAL-36-OUT,EXP-32-OUT;n:type:ShaderForge.SFN_ValueProperty,id:30,x:32975,y:32993,ptovrint:False,ptlb:SheenOffset,ptin:_SheenOffset,varname:node_9989,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_ValueProperty,id:31,x:33385,y:33125,ptovrint:False,ptlb:SheenFreq,ptin:_SheenFreq,varname:node_3905,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:6;n:type:ShaderForge.SFN_ValueProperty,id:32,x:33882,y:33074,ptovrint:False,ptlb:Sheen_Falloff,ptin:_Sheen_Falloff,varname:node_9829,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:3;n:type:ShaderForge.SFN_Add,id:33,x:34160,y:32628,varname:node_33,prsc:2|A-51-OUT,B-15-OUT;n:type:ShaderForge.SFN_Clamp01,id:36,x:33970,y:32904,varname:node_36,prsc:2|IN-16-OUT;n:type:ShaderForge.SFN_Multiply,id:38,x:32922,y:32563,varname:node_38,prsc:2|A-41-OUT,B-26-U;n:type:ShaderForge.SFN_ComponentMask,id:39,x:33389,y:32502,varname:node_39,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-43-OUT;n:type:ShaderForge.SFN_Multiply,id:41,x:32840,y:32369,varname:node_41,prsc:2|A-2-RGB,B-42-OUT;n:type:ShaderForge.SFN_Vector1,id:42,x:32695,y:32486,varname:node_42,prsc:2,v1:2;n:type:ShaderForge.SFN_Add,id:43,x:33124,y:32685,varname:node_43,prsc:2|A-38-OUT,B-26-U;n:type:ShaderForge.SFN_Add,id:44,x:34498,y:32579,varname:node_44,prsc:2|A-49-OUT,B-33-OUT;n:type:ShaderForge.SFN_Lerp,id:45,x:34170,y:32204,varname:node_45,prsc:2|A-48-RGB,B-46-RGB,T-26-V;n:type:ShaderForge.SFN_Color,id:46,x:33718,y:32047,ptovrint:False,ptlb:COLOR_A,ptin:_COLOR_A,varname:node_7501,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.9419875,c2:0.9447717,c3:0.9632353,c4:1;n:type:ShaderForge.SFN_Color,id:48,x:33649,y:32258,ptovrint:False,ptlb:COLOR_B,ptin:_COLOR_B,varname:node_4401,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.2958477,c2:0.3269538,c3:0.3529412,c4:1;n:type:ShaderForge.SFN_Multiply,id:49,x:34466,y:32269,varname:node_49,prsc:2|A-45-OUT,B-50-OUT;n:type:ShaderForge.SFN_ValueProperty,id:50,x:34066,y:32338,ptovrint:False,ptlb:gradOverlay,ptin:_gradOverlay,varname:node_4312,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.2;n:type:ShaderForge.SFN_Multiply,id:51,x:33934,y:32555,varname:node_51,prsc:2|A-45-OUT,B-1915-OUT;n:type:ShaderForge.SFN_Multiply,id:53,x:32987,y:32798,varname:node_53,prsc:2;n:type:ShaderForge.SFN_Multiply,id:1915,x:33448,y:32362,varname:node_1915,prsc:2|A-2-RGB,B-5551-OUT;n:type:ShaderForge.SFN_ValueProperty,id:5551,x:33182,y:32473,ptovrint:False,ptlb:Ref_mult,ptin:_Ref_mult,varname:node_5551,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;proporder:2-30-31-32-46-48-50-5551;pass:END;sub:END;*/

Shader "Shader Forge/galaxyExplorer_text_shader" {
    Properties {
        _REF ("REF", 2D) = "white" {}
        _SheenOffset ("SheenOffset", Float ) = 1
        _SheenFreq ("SheenFreq", Float ) = 6
        _Sheen_Falloff ("Sheen_Falloff", Float ) = 3
        _COLOR_A ("COLOR_A", Color) = (0.9419875,0.9447717,0.9632353,1)
        _COLOR_B ("COLOR_B", Color) = (0.2958477,0.3269538,0.3529412,1)
        _gradOverlay ("gradOverlay", Float ) = 0.2
        _Ref_mult ("Ref_mult", Float ) = 1
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma exclude_renderers xbox360 ps3 
            #pragma target 3.0
            uniform sampler2D _REF; uniform float4 _REF_ST;
            uniform float _SheenOffset;
            uniform float _SheenFreq;
            uniform float _Sheen_Falloff;
            uniform float4 _COLOR_A;
            uniform float4 _COLOR_B;
            uniform float _gradOverlay;
            uniform float _Ref_mult;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord1 : TEXCOORD1;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv1 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv1 = v.texcoord1;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
////// Lighting:
////// Emissive:
                float3 node_45 = lerp(_COLOR_B.rgb,_COLOR_A.rgb,i.uv1.g);
                float2 node_4 = viewReflectDirection.rg;
                float4 _REF_var = tex2D(_REF,TRANSFORM_TEX(node_4, _REF));
                float3 node_43 = (((_REF_var.rgb*2.0)*i.uv1.r)+i.uv1.r);
                float3 emissive = ((node_45*_gradOverlay)+((node_45*(_REF_var.rgb*_Ref_mult))+saturate(pow(saturate(sin(((node_43+_SheenOffset)*_SheenFreq))),_Sheen_Falloff))));
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
