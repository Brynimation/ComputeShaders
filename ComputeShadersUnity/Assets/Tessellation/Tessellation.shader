Shader "Custom/Tessellation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EdgeFactors("Edge Factors", vector) = (1.0,1.0,1.0)
        _EdgeInsideFactor("Edge inside factor", float) = 1.0
        _FrustumCullTolerance("frustum cull tolerance", float) = 0.01
        _FaceCullTolerance("face cull tolerance", float) = 0.01
        _Scale("Scale", float) = 1.0
        _Bias("Bias", float) = 1.0
        [KeywordEnum(INTEGER, FRAC_EVEN, FRAC_ODD, POW2)] _PARTITIONING("Partition algorithm", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma hull hull
            #pragma domain domain


            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma shader_feature_local _PARTITIONING_INTEGER _PARTITIONING_FRAC_EVEN _PARTITIONING_FRAC_ODD _PARTITIONING_POW2


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            uniform float3 _EdgeFactors;
            uniform float _EdgeInsideFactor;
            uniform float _FrustumCullTolerance;
            uniform float _FaceCullTolerance;
            uniform float _Scale;
            uniform float _Bias;
            //input to the vertex shader 
            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL0;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            //The position semantic is forbidden in this structure, Output by the vertex shader, input to hull shader. Also output by hull shader 
            struct TessellationControlPoint
            {
                float3 positionWS : TEXCOORD0;
                float3 normalWS : NORMAL0;
                float2 uv : TEXCOORD1;
                float4 positionHCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            //Output by the patch constant function 
            struct TessellationFactors
            {
                float edge[3] : SV_TESSFACTOR;
                float inside : SV_INSIDETESSFACTOR;
            };

            //output by the domain shader. Input to the fragment shader.
            struct Interpolators
            {
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            //returns true if a given point is outside of the bounds defined by the upper and lower vectors
            bool IsOutOfBounds(float3 p, float3 lower, float3 higher)
            {
                return (p.x < lower.x || p.x > higher.x || p.y < lower.y || p.y > higher.y || p.z < lower.z || p.z > higher.z);
            }

            //To perform face culling, we need to calculate which side of the triangle is facing the camera. To do this, calculate a normal vector perpendicular to 
            //the plane containing the three points of the triangle. Then, check if this vector roughly points towards the camera
            float3 GetTriangleNormal(float4 posaHCS, float4 posbHCS, float4 poscHCS)
            {
                float3 point0 = posaHCS.xyz / posaHCS.w; //apply perspective division, since we're working with Clip space coordinates. 
                float3 point1 = posbHCS.xyz / posbHCS.w; //By applying perspective division, we get (roughly) screen space positions
                float3 point2 = poscHCS.xyz / poscHCS.w;
                return cross(point1 - point0, point2 - point0);
            }

           bool ShouldBackFaceCull(float4 posaHCS, float4 posbHCS, float4 poscHCS)
           {
                float3 normal = GetTriangleNormal(posaHCS, posbHCS, poscHCS);
                //use the dot product to check if the view direction (0,0,1) is pointing in the same direction as the normal. Note that in clip space, the view direction is along the z axis.
                return dot(normal, float3(0,0,1)) < -_FaceCullTolerance;
           }
            //returns true if the given vertex is outside the camera view frustum 
            bool IsPointOutOfFrustum(float4 positionHCS)
            {
                float3 culling = positionHCS.xyz;
                float w = positionHCS.w;
                float3 lowerBound = float3(-w - _FrustumCullTolerance, -w - _FrustumCullTolerance, -w * UNITY_RAW_FAR_CLIP_VALUE - _FrustumCullTolerance);
                float3 upperBound = float3(w + _FrustumCullTolerance,w + _FrustumCullTolerance,w + _FrustumCullTolerance);
                return IsOutOfBounds(culling, lowerBound, upperBound);
            }

            bool ShouldClipPatch(float4 p0HCS, float4 p1HCS, float4 p2HCS)
            {
                bool allOutside = IsPointOutOfFrustum(p0HCS) && IsPointOutOfFrustum(p1HCS) && IsPointOutOfFrustum(p2HCS);
                return allOutside || ShouldBackFaceCull(p0HCS, p1HCS, p2HCS);
            }

            /*One means of optimising tessellation is to perform edge tessellation proptionally to the length of the edge. Ie, larger edges will be tessellated more,
            since smaller edges are already finely detailed. The below function calculates the edge tessellation factor of an edge bound by two vertices.
            We also tessellate an edge inversely proportional to its distance from the camera - edges closer to the camera are divided more*/
            float EdgeTessellationFactor(float scale, float bias, float3 p0WS, float3 p1WS)
            {
                float edgeLength = distance(p0WS, p1WS);
                float camDist = distance(GetCameraPositionWS(), (p0WS + p1WS) * 0.5); //distance between the centre of the edge and camera
                float factor = edgeLength / (scale * camDist * camDist);
                return max(1, factor + bias);
            }


            //vertex shader converts vertex positions and normals from object space to world space
            TessellationControlPoint vert(Attributes i)
            {
                TessellationControlPoint o;
                VertexPositionInputs posnInputs = GetVertexPositionInputs(i.positionOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(i.normalOS);

                o.positionWS = posnInputs.positionWS;
                o.positionHCS = posnInputs.positionCS;
                o.normalWS = normalInputs.normalWS;
                o.uv = i.uv;
                return o;
            };
            

            //Patch constant function runs once per triangle, in parallel with the hull function
            TessellationFactors PatchConstantFunction(InputPatch<TessellationControlPoint, 3> patch)
            {
                UNITY_SETUP_INSTANCE_ID(patch[0]);
                TessellationFactors tf = (TessellationFactors)0;

                if(ShouldClipPatch(patch[0].positionHCS, patch[1].positionHCS, patch[2].positionHCS))
                {
                    tf.edge[0] = tf.edge[1] = tf.edge[2] = 0; //cull the patch by setting its edge factors to zero
                }else
                {
                    //Defines how many times to subdivide each edge of a triangle. A given edge, x, is the one opposite the vertex, x (just like the sin and cosine rule)
                    //Ie, edge 0 lies between vertices 1 and 2
                    tf.edge[0] = EdgeTessellationFactor(_Scale, _Bias, patch[1].positionWS, patch[2].positionWS); //_EdgeFactors.x;
                    tf.edge[1] = EdgeTessellationFactor(_Scale, _Bias, patch[2].positionWS, patch[0].positionWS);//_EdgeFactors.y;
                    tf.edge[2] = EdgeTessellationFactor(_Scale, _Bias, patch[1].positionWS, patch[0].positionWS);//_EdgeFactors.z;
                    tf.inside = tf.edge[0] + tf.edge[1] + tf.edge[2] / 3.0;//_EdgeInsideFactor; //inside ^ 2 is roughly the number of triangles generated from one input triangle
                }
                return tf;
            }


            //The hull function runs once per vertex per patch. It can be used to modify vertex data based on values in the entire triangle 
            
            [domain("tri")] //input topology is triangles 
            [outputcontrolpoints(3)] //we're also outputting triangles
            [outputtopology("triangle_cw")]
            [patchconstantfunc("PatchConstantFunction")] //register the patch constant function 
            #if defined(_PARTITIONING_INTEGER)
            [partitioning("integer")]
            #elif defined(_PARTITIONING_FRAC_EVEN)
            [partitioning("fractional_even")]
            #elif defined(_PARTITIONING_FRAC_ODD)
            [partitioning("fractional_odd")]
            #elif defined(_PARTITIONING_POW2)
            [partitioning("pow2")]
            #else 
            [partitioning("fractional_odd")]
            #endif
            TessellationControlPoint hull(InputPatch<TessellationControlPoint, 3> patch, uint id: SV_OUTPUTCONTROLPOINTID)
            {
            //The variable "patch" is an input triangle. The variable "id" identifies which vertex on the triangle we're currently processing
                return patch[id];
            }

            #define BARYCENTRIC_INTERPOLATE(fieldName) \
                patch[0].fieldName * barycentricCoordinates.x + \
                patch[1].fieldName * barycentricCoordinates.y + \
                patch[2].fieldName * barycentricCoordinates.z

            [domain("tri")]
            Interpolators domain(
                TessellationFactors factors, //output of patch constant function 
                OutputPatch<TessellationControlPoint, 3> patch, //input triangles 
                float3 barycentricCoordinates : SV_DOMAINLOCATION) //barycentric coordinates of the vertex on the triangle 
                {
                    Interpolators o;
                    UNITY_SETUP_INSTANCE_ID(patch[0]);
                    UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                    float3 positionWS = BARYCENTRIC_INTERPOLATE(positionWS);
                    float3 normalWS = BARYCENTRIC_INTERPOLATE(normalWS);
                    float2 uv = BARYCENTRIC_INTERPOLATE(uv);
                    o.positionHCS = TransformWorldToHClip(positionWS);
                    o.normalWS = normalWS;
                    o.positionWS = positionWS;
                    o.uv = uv;

                    return o;

               }
               float4 frag(Interpolators i) : SV_TARGET0{
                    UNITY_SETUP_INSTANCE_ID(i);
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                    float4 texel = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                    //return texel * _Colour;

                    //Lighting calculations
                    //Cast the lighting and surface input to zero to initialise all its fields to zero
                    InputData lightingInput = (InputData)0; //struct holds information about the position and orientation of the current fragment
                    SurfaceData surfaceInput = (SurfaceData)0;//holds information about the surface material's physical properties.
                
                    lightingInput.normalWS = normalize(i.normalWS);

                    surfaceInput.albedo = texel.rgb;// * i.colour;
                    surfaceInput.alpha = texel.a;// * i.colour;
                    return UniversalFragmentBlinnPhong(lightingInput, surfaceInput);
             }
            
            
            
              
         
            ENDHLSL
        }
    }
}
