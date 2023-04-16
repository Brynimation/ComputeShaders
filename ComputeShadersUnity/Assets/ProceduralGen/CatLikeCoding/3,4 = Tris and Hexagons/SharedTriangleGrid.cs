using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;
using Unity.VisualScripting;

namespace ProceduralMeshes 
{
    public struct SharedTriangleGrid : IMeshGenerator
    {
        public int Resolution { get; set; }

        public Bounds Bounds => new Bounds(Vector3.zero, (new Vector3(1f + 0.5f / Resolution, 0f, sqrt(3f)/2f)));

        //A grid with R quads using shared vertices will contain R + 1 vertices.
        public int VertexCount => (Resolution + 1) * (Resolution + 1);

        public int IndexCount => 6 * (Resolution) * (Resolution);

        public int jobLength => Resolution + 1;

        /*To form equilateral triangles, we must shift alternating rows half a triangle relative to eachother. To keep
         the grid centred, we'll shift even vertex rows by -0.25 and odd rows by +0.25. We'll then divide by the resolution, and
        subtract 0.5 from that to keep the grid centred about the origin.
        However, the steps described above will just make a grid of rhombuses. To make them equilateral triangles, we must reduce the 
        height / z value of the triangle. The height of an equilateral triangle is sqrt(3)/2 * (side length).
        We also need to make adjustments to the even triangle rows; that is, triangles beneath even vertex rows. We must use different 
        triangle layouts (ie, the indices of the triangle must be given in a different order) for triangles below even rows.
         */
        public void Execute<S>(int zCoord, S streams) where S : struct, IMeshStreams
        {
            int vertexIndex = (Resolution + 1) * zCoord; //Index of the first vertex of the row current row
            int triangleIndex = 2 * Resolution * (zCoord - 1);
            bool isEven = (zCoord % 2 == 0) ? true : false;
            //Here we make adjustments to the order in which our triangles are constructed from their indices based on whether the row is even or odd
            int iA = -Resolution - 2;
            int iB = -Resolution - 1;
            int iC = -1;
            int iD = 0;
            int3 triA = isEven ? int3(iA, iC, iB) : int3(iA, iC, iD);
            int3 triB = isEven ? int3(iB, iC, iD) : int3(iA, iD, iB);


            //Here we make adjustments to the texture coordinates to accommodate the equilateral triangle grid.
            float xOffset = isEven ? -0.25f : 0.25f;//The offset now differs per row. shift left if even, right if odd
            xOffset = xOffset / Resolution - 0.5f;
            float uOffset = isEven ? 0 : 0.5f / (Resolution + 0.5f); //We need to unskew the texture coordinates with an offset
            float3 normal = up();
            float4 tangent = float4(1f, 0f, 0f, -1f);

            //Z coordinates and uvs are constant per row. We only need to change the x component of our positions and the u component of our uvs.
            float zPos = ((float)zCoord / Resolution - 0.5f) * sqrt(3f)/2f;
            float zUV = (float)zCoord / (1f + 0.5f / Resolution) + 0.5f; //scale down the texture vertically 

            float3 pos0 = float3(xOffset, 0, zPos);
            float2 uv0 = float2(uOffset, zUV);
            streams.SetVertex(vertexIndex, new Vertex(pos0, normal, tangent, uv0));
            vertexIndex += 1;
            for (int x = 1; x <= Resolution; x++, vertexIndex++, triangleIndex += 2)
            {
                //Set vertices
                float3 pos = float3((float)x / Resolution + xOffset, 0f, zPos);
                float2 uv = float2((float)x / (Resolution + 0.5f) + uOffset, zUV);
                streams.SetVertex(vertexIndex, new Vertex(pos, normal, tangent, uv));

                //Set triangles
                //Triangle indices are shared and they are made relative to the top-right vertex
                //This means: vertex index offset for the top right index is 0, top left is -1, bottom right is res-1 and bottom left is res-2
                if (zCoord > 0) //This check ensures that we don't try to generate quads for the bottom row of vertices 
                {
                    streams.SetTriangle(triangleIndex, vertexIndex + triA);
                    streams.SetTriangle(triangleIndex + 1, vertexIndex + triB);
                }


            }

        }
    }
}

