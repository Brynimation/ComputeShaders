Shader "Custom/MyParticle"
{
    Properties
    {
        _PointSize("Point Size", Float) = 2.0
        _MainTex("Main Texture", 2D) = "white"{}
		_CameraPosition("Camera Position", vector) = (0.0, 0.0, 0.0)
    }
    SubShader
    {
        pass
        {
            Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
            LOD 200
            Blend SrcAlpha one

            HLSLPROGRAM
            //Pragmas
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag 

            //Uniforms 
			uniform float _PointSize;

			float3 _CameraPosition;

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			#pragma target 5.0

			struct Particle{
				float3 position;
				float3 velocity;
				float3 lifetime;
			};

			StructuredBuffer<Particle> _ParticleBuffer; //shader does not write to this buffer (hence, not a RWBuffer)
		
			struct VertexOut{

				float4 positionWS : POSITION; 
				float4 colour : COLOR;
				float life : TEXCOORD0;
				float size: TEXCOORD1;
			};

			struct GeomOut
			{
				float4 positionHCS : SV_POSITION;
				float4 colour : COLOR;
				float2 uv : TEXCOORD0;
			};
		

			VertexOut vert(uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
			{
				//vertex_id will always be zero
				//instance_id will range from (0, instanceCount - 1)
				VertexOut o = (VertexOut)0;

				// Colour
				float life = _ParticleBuffer[instance_id].lifetime;
				float lerpVal = life * 0.25f;
				o.colour = float4(1 - lerpVal + 0.1, lerpVal + 0.1, 1, lerpVal);
				//position
				o.positionWS = mul(unity_ObjectToWorld, float4(_ParticleBuffer[instance_id].position, 1.0f));
				o.size = _PointSize;

				return o;
			}

			[maxvertexcount(4)]
			void geom(point VertexOut input[1], inout TriangleStream<GeomOut> outputStream)
			{
				VertexOut centre = input[0];

				float3 forward = -(_WorldSpaceCameraPos - centre.positionWS);
				forward.y = 0.0;
				forward = normalize(forward);

				float3 up = float3(0.0, 1.0, 0.0);
				float3 right = normalize(cross(forward, up));

				up.y *= (float) centre.size/2;
				right *= (float) centre.size/2;//centre.size/2;

				float3 WSPositions[4];
				float2 uvs[4];


				//Generate points on a quad:
				WSPositions[0] = centre.positionWS - right - up;
				WSPositions[1] = centre.positionWS + right - up;
				WSPositions[2] = centre.positionWS - right + up;
				WSPositions[3] = centre.positionWS + right + up;
			
				uvs[0] = float2(0, 0);
				uvs[1] = float2(1, 0);
				uvs[2] = float2(0, 1);
				uvs[3] = float2(1, 1);
				
				for(int i = 0; i < 4; i++)
				{
					GeomOut o;
				    o.positionHCS = mul(UNITY_MATRIX_VP, float4(WSPositions[i], 1.0));
					o.uv = uvs[i];
					o.colour = centre.colour;
					outputStream.Append(o);
				}
			}


			float4 frag(GeomOut i) : SV_TARGET0 
			{
				float4 baseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				//if(baseTex.a == 0.0) discard;
				float4 colour = i.colour;


				return i.colour;
			}
            ENDHLSL
        }
    }
	FallBack Off
}
