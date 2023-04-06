Shader "Custom/InstancedMat"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaxStarSize("Max Star Size", float) = 10.0
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


            StructuredBuffer<float4> _Positions;
            float _MaxStarSize;
            float4x4 _Matrix;

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL0;
                float2 uv : TEXCOORD0;
                uint index: SV_InstanceID;
            };
            struct Interpolators
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : NORMAL0;
                float2 uv : TEXCOORD0;
            };

            float4x4 CreateMatrix(float3 pos, float3 dir, float3 up, uint id){
                float3 zaxis = normalize(dir);
                float3 xaxis = normalize(cross(up, zaxis));
                float3 yaxis = cross(zaxis, xaxis);
                //float scale = GenerateRandom(id) * _MaxStarSize;
                //Transform the vertex into the object space of the currently drawn mesh using a Transform Rotation Scale matrix.
                return float4x4(
                    xaxis.x * _MaxStarSize, yaxis.x, zaxis.x, pos.x,
                    xaxis.y, yaxis.y * _MaxStarSize, zaxis.y, pos.y,
                    xaxis.z, yaxis.z, zaxis.z * _MaxStarSize, pos.z,
                    0, 0, 0, 1
                );
            }
            float3 GeneratePosition(uint3 id)
            {
                return (float3)id;
            }
            Interpolators vert(Attributes i)
            {
                Interpolators o;
                float4 position = _Positions[i.index];
                _Matrix = CreateMatrix(position, float3(1.0, 1.0, 1.0), float3(0.0, 1.0, 0.0), i.index);
                float4 posOS = mul(_Matrix, i.positionOS);
                float4 posWS = mul(unity_ObjectToWorld, posOS);
                float3 normalOS = mul(_Matrix, i.normalOS);
                o.normalWS = mul(unity_ObjectToWorld, normalOS);
                o.positionHCS = mul(UNITY_MATRIX_VP, posWS);
                o.uv = i.uv;
                return o;
            }

            float4 frag(Interpolators i) : SV_TARGET0
            {
                //return float4(1.0,1.0,1.0,1.0);
                float4 texel = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                //Lighting calculations
                //Cast the lighting and surface input to zero to initialise all its fields to zero
                InputData lightingInput = (InputData)0; //struct holds information about the position and orientation of the current fragment
                SurfaceData surfaceInput = (SurfaceData)0;//holds information about the surface material's physical properties.

                lightingInput.normalWS = normalize(i.normalWS);

                surfaceInput.albedo = texel.rgb;
                surfaceInput.alpha = texel.a;
                return UniversalFragmentBlinnPhong(lightingInput, surfaceInput);
            }
           
            ENDHLSL
        }
    }
}
