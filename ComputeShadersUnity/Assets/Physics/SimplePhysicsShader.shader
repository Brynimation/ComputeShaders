Shader "Custom/SimplePhysicsShader"
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
        cull Off 
        Pass
        {
            HLSLPROGRAM
            #pragma target 5.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Ball
            {
                float3 position;
                float3 velocity;
                float4 colour;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            StructuredBuffer<Ball> ballsBuffer;
            float4 _Colour;
            float3 _BallPosition;
            float radius;

            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                uint instanceId : SV_INSTANCEID;
                uint id : SV_VERTEXID;
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
            };

            

            Interpolators vert (Attributes i)
            {
                _Colour = ballsBuffer[i.instanceId].colour;
                _BallPosition = ballsBuffer[i.instanceId].position;
                Interpolators o;
                o.positionHCS = TransformObjectToHClip(_BallPosition + (i.positionOS.xyz * radius));
                o.uv = i.uv;
                return o;
            }

            float4 frag (Interpolators i) : SV_Target
            {
                // sample the texture
                float4 baseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _Colour;
                return baseTex;
            }
            ENDHLSL
        }
    }
}
