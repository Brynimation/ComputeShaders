Shader "Custom/ColourBlit"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _Intensity("Intensity", float ) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off
        Cull Off 

        Pass
        {
            Name "ColourBlitPass"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag 

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionHCS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float _Intensity;
            CBUFFER_END

            Varyings vert(Attributes i)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(i);
                //This pass is set up with a mesh already. We don't need to use TransformToHClipSpace
                o.positionCS = float4(i.positionHCS.xyz, 1.0);
                o.uv = TRANSFORM_TEX(i.uv, _BaseMap); //Transform_Tex Applies the tiling and offset parameters defined in the inspector
                return o;
            }
            float4 frag(Varyings i) : SV_TARGET0
            {
                float4 colour = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                return colour * float4(0, _Intensity, 0, 1);
            }
            ENDHLSL
        }
    }
}
