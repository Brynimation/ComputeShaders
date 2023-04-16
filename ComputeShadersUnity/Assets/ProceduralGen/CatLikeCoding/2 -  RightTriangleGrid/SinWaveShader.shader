Shader "Custom/SinWaveShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaxVertexDisplacement("Maximum vertex displacement", float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        HLSLINCLUDE

        #pragma target 5.0
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        uniform float _MaxVertexDisplacement;

        ENDHLSL
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                uint id : SV_VERTEXID;
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
            };

            

            Interpolators vert (Attributes i)
            {
                float3 curObjectPos = _MaxVertexDisplacement * float3(i.positionOS.x, sin(i.id * _Time.y), i.positionOS.z);
                Interpolators o;
                o.positionHCS = TransformObjectToHClip(curObjectPos);
                o.uv = i.uv;
                return o;
            }

            float4 frag (Interpolators i) : SV_Target
            {
                // sample the texture
                float4 baseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);;
                return baseTex;
            }
            ENDHLSL
        }
    }
}
