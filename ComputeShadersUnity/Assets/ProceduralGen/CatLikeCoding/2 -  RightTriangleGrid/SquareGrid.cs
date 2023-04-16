using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEditor.Compilation;

namespace ProceduralMeshes
{
    /*Square grid where each quad is independent. Each quad consists of two triangles that 
     * share two vertices, but adjacent quads do not share vertices
     * This is good for tilemapping, since each quad may have its own uvs.*/
    public struct SquareGrid : IMeshGenerator
    {
        public int Resolution { get; set;} //A public property like this is an auto-implemented property, meaning a private associated field is create along with it.
        Bounds IMeshGenerator.Bounds => new Bounds(Vector3.zero, (new Vector3(1f, 0f, 1f)));
        int IMeshGenerator.VertexCount => 4 * Resolution * Resolution;

        int IMeshGenerator.IndexCount => 6 * Resolution * Resolution;

        int IMeshGenerator.jobLength => Resolution; //In each job, we generate a single row of quads (each row contains resolution quads)

        /*The job index passed to the Execute method represents the quad index. There are 4 verts per quad, and 2 triangles per quad*/

        void IMeshGenerator.Execute<S>(int zCoord, S streams)
        {
            int vertexIndex = 4 * Resolution * zCoord;
            int triangleIndex = 2 * Resolution * zCoord;


            //The four corners of our quad are as follows: BottomLeft(x, 0 ,z), BottomRight:(x + 1, 0, z), topLeft: (x, 0, z+1), topright(x+1, 0f, z+1). We divide by (Resolution) and subtract 0.5f so that the centre of the plane is its origin
            //This is a plane in the x-z plane. It's normals will therefore point up, in the y direction
            //We can make more optimisations to this as detailed here: https://catlikecoding.com/unity/tutorials/procedural-meshes/square-grid/
            for (int xCoord = 0; xCoord < Resolution; xCoord++, vertexIndex+=4, triangleIndex+=2) 
            {
                streams.SetVertex(vertexIndex, new Vertex(float3(xCoord, 0, zCoord) / Resolution - 0.5f, up(), float4(1f, 0f, 0f, -1f), float2(0, 0)));
                streams.SetVertex(vertexIndex + 1, new Vertex(float3(xCoord + 1f, 0f, zCoord) / Resolution - 0.5f, up(), float4(1f, 0f, 0f, -1f), float2(1f, 0f)));
                streams.SetVertex(vertexIndex + 2, new Vertex(float3(xCoord, 0f, zCoord + 1f) / Resolution - 0.5f, up(), float4(1f, 0f, 0f, -1f), float2(0f, 1f)));
                streams.SetVertex(vertexIndex + 3, new Vertex(float3(xCoord + 1f, 0f, zCoord + 1f) / Resolution - 0.5f, up(), float4(1f, 0f, 0f, -1f), float2(1f, 1f)));

                streams.SetTriangle(triangleIndex, vertexIndex + int3(0, 2, 1));
                streams.SetTriangle(triangleIndex + 1, vertexIndex + int3(1, 2, 3));
            }

        }
    }
}
