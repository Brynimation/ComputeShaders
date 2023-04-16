using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEditor.Compilation;

namespace ProceduralMeshes
{
    public struct PointyHexagonGrid : IMeshGenerator
    {
        public int Resolution { get; set; } //A public property like this is an auto-implemented property, meaning a private associated field is created along with it.
        //if the resolution is greater than one, our grid has a width of (0.5 + 0.25/r) * sqrt(3).
        //if the resolution is equal to one, the width is sqrt(3)/2
        //In both cases, the height is equal to 0.75 + 0.25/r
        Bounds IMeshGenerator.Bounds => new Bounds(Vector3.zero, new Vector3(
            (Resolution > 1) ? (0.5f + 0.25f / Resolution) * sqrt(3) : 0.5f * sqrt(3),
            0f, 
            0.75f + 0.25f / Resolution));
        
        int IMeshGenerator.VertexCount => 7 * Resolution * Resolution; //seven vertices per hexagon; 6 for the corners and 1 for the centre

        int IMeshGenerator.IndexCount => 18 * Resolution * Resolution; //6 triangles per hexagon

        int IMeshGenerator.jobLength => Resolution; //In each job, we generate a single row of hexagons

        void IMeshGenerator.Execute<S>(int zCoord, S streams)
        {
            int vertexIndex = 7 * Resolution * zCoord;
            int triangleIndex = 6 * Resolution * zCoord;

            //We can make more optimisations to this as detailed here: https://catlikecoding.com/unity/tutorials/procedural-meshes/square-grid/
            float h = sqrt(3f) / 4f;
            float2 centreOffset = 0f;
            //Modify the centre depending on whether the row is even or odd to remove of the gaps between rows
            if (Resolution > 1)
            {
                centreOffset.x = (((zCoord & 1) == 0 ? 0.5f : 1.5f) - Resolution) * h;
                centreOffset.y = -0.375f * (Resolution - 1);
            }

            for (int xCoord = 0; xCoord < Resolution; xCoord++, vertexIndex += 7, triangleIndex += 6)
            {

                float2 centre = (float2(2f * h * xCoord, zCoord * 0.75f) + centreOffset)/ Resolution;
                float2 xCoords = centre.x + float2(-h, h) / Resolution;
                float4 zCoords = centre.y + float4(-0.5f, -0.25f, 0.25f, 0.5f) / Resolution;
                //Each hexagon has seven vertices. We start with its centre, then to the bottom, then clockwise from there.
                /*Each hexagon has a width of two triangle heights and a height of 1, both divided by the resolution*/
                streams.SetVertex(vertexIndex, new Vertex(float3(centre.x, 0f, centre.y), up(), float4(1f, 0f, 0f, -1f), float2(0.5f, 0.5f)));

                /*There are 3 different x coords per hexagon, centre (verts 0, 1 and 4), verts 2 and 3 on the left of the centre and 4 and 5 on the right. Their x coordinates are offset by the height of the equilateral triangle in both directions (we'll make this sqrt(3)/4 so
                this grid is roughly the same size as our square grid.
                For the z coordinates there are four extra besides the centre: -0.5, -0.25, 0.25f, 0.5f
                For the texture coords, the hexagon is one unit high and two triangle heights wide. We use this to set the uvs so it is centred on the
                hexagon and doesn't get deformed
                 
                 */
                streams.SetVertex(vertexIndex + 1, new Vertex(float3(centre.x, 0f, zCoords.x), up(), float4(1f, 0f, 0f, -1f), float2(0.5f, 0)));
                streams.SetVertex(vertexIndex + 2, new Vertex(float3(xCoords.x, 0f, zCoords.y), up(), float4(1f, 0f, 0f, -1f), float2(0.5f - h, 0.25f)));
                streams.SetVertex(vertexIndex + 3, new Vertex(float3(xCoords.x, 0f, zCoords.z), up(), float4(1f, 0f, 0f, -1f), float2(0.5f - h, 0.75f)));
                streams.SetVertex(vertexIndex + 4, new Vertex(float3(centre.x, 0f, zCoords.w), up(), float4(1f, 0f, 0f, -1f), float2(0.5f, 1f)));
                streams.SetVertex(vertexIndex + 5, new Vertex(float3(xCoords.y, 0f, zCoords.z), up(), float4(1f, 0f, 0f, -1f), float2(0.5f + h, 0.75f)));
                streams.SetVertex(vertexIndex + 6, new Vertex(float3(xCoords.y, 0f, zCoords.y), up(), float4(1f, 0f, 0f, -1f), float2(0.5f + h, 0.25f)));

                //With the vertices set, we add the six triangles. We start at the bottom and go clockwise from there
                streams.SetTriangle(triangleIndex, vertexIndex + int3(0, 1, 2));
                streams.SetTriangle(triangleIndex + 1, vertexIndex + int3(0, 2, 3));
                streams.SetTriangle(triangleIndex + 2, vertexIndex + int3(0, 3, 4));
                streams.SetTriangle(triangleIndex + 3, vertexIndex + int3(0, 4, 5));
                streams.SetTriangle(triangleIndex + 4, vertexIndex + int3(0, 5, 6));
                streams.SetTriangle(triangleIndex + 5, vertexIndex + int3(0, 6, 1));

            }

        }
    }
}
