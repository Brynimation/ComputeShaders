using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace ProceduralMeshes 
{
    public struct SharedSquareGrid : IMeshGenerator
    {
        public int Resolution { get; set; }

        public Bounds Bounds => new Bounds(Vector3.zero, (new Vector3(1f, 0f, 1f)));

        //A grid with R quads using shared vertices will contain R + 1 vertices.
        public int VertexCount => (Resolution + 1) * (Resolution + 1);

        public int IndexCount => 6 * (Resolution) * (Resolution);

        public int jobLength => Resolution + 1;

        public void Execute<S>(int zCoord, S streams) where S : struct, IMeshStreams
        {
            int vertexIndex = (Resolution + 1) * zCoord; //Index of the first vertex of the row current row
            int triangleIndex = 2 * Resolution * (zCoord - 1);
            float3 normal = up();
            float4 tangent = float4(1f, 0f, 0f, -1f);
            //Z coordinates and uvs are constant per row. We only need to change the x component of our positions and the u component of our uvs.
            float zPos = (float)zCoord / Resolution - 0.5f;
            float zUV = (float)zCoord / Resolution;

            float3 pos0 = float3(-0.5f, 0, zPos);
            float2 uv0 = float2(0.0f, zUV);
            streams.SetVertex(vertexIndex, new Vertex(pos0, normal, tangent, uv0));
            vertexIndex += 1;
            for (int x = 1; x <= Resolution; x++, vertexIndex++, triangleIndex+=2) 
            {
                //Set vertices
                float3 pos = float3((float)x / Resolution - 0.5f, 0f, zPos);
                float2 uv = float2((float)x / Resolution, zUV);
                streams.SetVertex(vertexIndex, new Vertex(pos, normal, tangent, uv));

                //Set triangles
                //Triangle indices are shared and they are made relative to the top-right vertex
                //This means: vertex index offset for the top right index is 0, top left is -1, bottom right is res-1 and bottom left is res-2
                if (zCoord > 0) //This check ensures that we don't try to generate quads for the bottom row of vertices 
                {
                    streams.SetTriangle(triangleIndex, vertexIndex + int3(-Resolution - 2, -1, -Resolution - 1));
                    streams.SetTriangle(triangleIndex + 1, vertexIndex + int3(-Resolution - 1, -1, 0));
                }

            
            }

        }
    }
}

