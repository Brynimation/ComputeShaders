Shader "Custom/MyParticle2" {
	Properties     
    {         
        _PointSize("Point size", Float) = 5.0
		_MainTex("Main Texture", 2D) = "white"{}
    } 

	SubShader {
		Pass {
		Tags{"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		HLSLPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma vertex vert
		#pragma geometry geom
		#pragma fragment frag

		uniform float _PointSize;

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 5.0

		TEXTURE2D(_MainTex);
		SAMPLER(sampler_MainTex);

		struct Particle{
			float3 position;
			float3 velocity;
			float lifetime;
		};
		StructuredBuffer<Particle> _ParticleBuffer;
		struct VertexOut{
			float4 positionWS : POSITION;
			float4 colour : COLOR;
			float life : TEXCOORD0;
			float size: TEXCOORD1;
		};
		struct GeomOut{
			float4 positionHCS : SV_POSITION;
			float4 colour : COLOR;
			float2 uv : TEXCOORD0;

		};
		// particles' data

		

		VertexOut vert(uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
		{
			VertexOut o = (VertexOut)0;

			// Color
			float life = _ParticleBuffer[instance_id].lifetime;
			float lerpVal = life * 0.25f;
			o.colour = float4(1.0f - lerpVal+0.1, lerpVal+0.1, 1.0f, lerpVal);

			// Position
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
			float4 colour = baseTex * i.colour;


			return baseTex * i.colour;
		}


		ENDHLSL
		}
	}
	FallBack Off
}