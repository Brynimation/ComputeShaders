using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEditor.Compilation;

namespace ProceduralMeshes
{
    /*We need to create a six - squared grid and then fold that net into a cube.
     Since each side will have its own position and orientation, we introduce the 
     CubeFace struct to encapsulate this information
     */
    public struct CubeSphere : IMeshGenerator
    {
        private struct CubeFace {
            public int id;
            public float3 uvOrigin; //origin position in the cube's space
            public float3 uAxis;
            public float3 vAxis;
            public float3 normal;
            public float4 tangent;

            public CubeFace(int id) 
            {
                this.id = id;
                switch (id) 
                {
                    case 0:
                        uvOrigin = float3(-1, -1, -1);
                        uAxis = 2f * right();
                        vAxis = 2f * up();
                        normal = back();
                        tangent = float4(1f, 0f, 0f, -1f);
                        break;
                    case 1:
                        uvOrigin = float3(1f, -1f, -1f);
                        uAxis = 2f * forward();
                        vAxis = 2f * up();
                        normal = right();
                        tangent = float4(0f, 0f, 1f, -1f);
                        break;
                    case 2:
                        uvOrigin = float3(-1f, -1f, -1f);
                        uAxis = 2f * forward();
                        vAxis = 2f * right();
                        normal = down();
                        tangent = float4(0f, 0f, 1f, -1f);
                        break;
                    case 3:
                        uvOrigin = float3(-1f, -1f, 1f);
                        uAxis = 2f * up();
                        vAxis = 2f * right();
                        normal = forward();
                        tangent = float4(0f, 1f, 0f, -1f);
                        break;
                    case 4:
                        uvOrigin = float3(-1f, -1f, -1f);
                        uAxis = 2f * up();
                        vAxis = 2f * forward();
                        normal = left();
                        tangent = float4(0f, 1f, 0f, -1f);
                        break;
                    case 5:
                        uvOrigin = float3(-1f, 1f, -1f);
                        uAxis = 2f * right();
                        vAxis = 2f * forward();
                        normal = up();
                        tangent = float4(1f, 0f, 0f, -1f);
                        break;
                    default:
                        uvOrigin = float3(-1, -1, -1); ;
                        uAxis = 2f * right();
                        vAxis = 2f * up();
                        normal = back();
                        tangent = float4(1f, 0f, 0f, -1f);
                        break;
                }
            }
        };
        static float3 CubeToSphere(float3 p) => normalize(p);
        public int Resolution { get; set; } //A public property like this is an auto-implemented property, meaning a private associated field is create along with it.
        Bounds IMeshGenerator.Bounds => new Bounds(Vector3.zero, (new Vector3(2f, 2f, 2f)));
        int IMeshGenerator.VertexCount => 6 * 4 * Resolution * Resolution;

        int IMeshGenerator.IndexCount => 6 * 6 * Resolution * Resolution;

        int IMeshGenerator.jobLength => 6 * Resolution; //In each job, we generate a single row of quads (each row contains resolution quads)

        /*The job index passed to the Execute method represents the quad index. There are 4 verts per quad, and 2 triangles per quad*/


        /*This method creates a column of quads for each cube face. A column has two consistent u values, uA (the left side of the quad) and uB (the right side)*/
        void IMeshGenerator.Execute<S>(int i, S streams)
        {
            int uCoord = i / 6;

            //The origin, uAxis, vAxis, normal and tangent are determined by the index. 
            CubeFace side = new CubeFace(i - 6 * uCoord);
            
            int vertexIndex = 4 * Resolution * (Resolution * side.id + uCoord);
            int triangleIndex = 2 * Resolution * (Resolution * side.id + uCoord);

            float3 uA = side.uvOrigin + side.uAxis * (float)uCoord / Resolution;
            float3 uB = side.uvOrigin + side.uAxis * (float)(uCoord + 1) / Resolution;

            float2 uv0 = float2(0f, 0f);
            float2 uv1 = float2(1f, 0f);
            float2 uv2 = float2(0f, 1f);
            float2 uv3 = float2(1f, 1f);

            /*Since quads share vertices, we can cache the computation of the rightmost vertices and use them as the leftmost
             vertices of the next quad. This saves some compute.*/
            float3 p0 = normalize(uA); 
            float3 p1 = normalize(uB);
            float4 tangent = float4(normalize(p1 - p0), 1);
            for (int vCoord = 1; vCoord <= Resolution; vCoord++, vertexIndex += 4, triangleIndex += 2)
            {
                float3 p2 = normalize(uA + side.vAxis * (float)vCoord / Resolution);
                float3 p3 = normalize(uB + side.vAxis * (float)vCoord / Resolution);

                streams.SetVertex(vertexIndex, new Vertex(p0, p0, tangent, uv0));
                streams.SetVertex(vertexIndex + 1, new Vertex(p1, p1, tangent, uv1));

                tangent = float4(normalize(p3 - p2), 1);

                streams.SetVertex(vertexIndex + 2, new Vertex(p2, p2, tangent, uv2));
                streams.SetVertex(vertexIndex + 3, new Vertex(p3, p3, tangent, uv3));

                streams.SetTriangle(triangleIndex + 0, vertexIndex + int3(0, 2, 1));
                streams.SetTriangle(triangleIndex + 1, vertexIndex + int3(1, 2, 3));

                p0 = p2;
                p1 = p3;
                
            }

        }
    }
}
