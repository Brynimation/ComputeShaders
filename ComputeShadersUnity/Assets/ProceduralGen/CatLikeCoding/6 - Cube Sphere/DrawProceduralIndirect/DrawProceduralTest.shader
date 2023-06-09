Shader "Custom/DrawProceduralTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        pass
        {
            cull Off
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma target 5.0
            #pragma vertex vert 
            #pragma fragment frag
            #pragma multi_compile_instancing

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            StructuredBuffer<float3> _Positions;
            StructuredBuffer<float3> _VertexBuffer;
            float4x4 _ModelMatrix;

            float4x4 CreateMatrix(float3 pos, float3 dir, float3 up, uint id) {
                float3 zaxis = normalize(dir);
                float3 xaxis = normalize(cross(up, zaxis));
                float3 yaxis = cross(zaxis, xaxis);
                //float scale = GenerateRandom(id) * _MaxStarSize;
                //Transform the vertex into the object space of the currently drawn mesh using a Transform Rotation Scale matrix.
                return float4x4(
                    xaxis.x * 10, yaxis.x, zaxis.x, 10,
                    xaxis.y, yaxis.y * 10, zaxis.y, 10,
                    xaxis.z, yaxis.z, zaxis.z * 10, 10,
                    0, 0, 0, 1
                );
            }
            struct Attributes
            {
                uint vertexId : SV_VERTEXID;
                //float4 positionOS : POSITION;
                uint instanceId : SV_INSTANCEID;
            };

            struct Interpolators
            {
                float4 positionHCS : SV_POSITION;
            };

            Interpolators vert(Attributes i)
            {
                Interpolators o;
                //_ModelMatrix = CreateMatrix(float3(200.0, 200.0, 200.0), float3(1.0, 1.0, 1.0), float3(0.0, 1.0, 0.0), i.instanceId);
                float4 vertexPosOS = float4(_Positions[i.instanceId] + _VertexBuffer[i.vertexId], 1.0);
                float4 posWS = mul(unity_ObjectToWorld, vertexPosOS);
                VertexPositionInputs positionData = GetVertexPositionInputs(vertexPosOS);
                //o.positionHCS = mul(UNITY_MATRIX_VP, posWS);
                o.positionHCS = positionData.positionCS;
                return o;
            }

            float4 frag(Interpolators i) : SV_TARGET0
            {
                return float4(0.0, 0.3, 0.4, 1);
            }


            ENDHLSL
        }
    }
}
