using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Mesh;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace ProceduralMeshes 
{
    public struct SingleStream : IMeshStreams
    {
        public const int VERTEX_ATTRIBUTE_COUNT = 4;
        [StructLayout(LayoutKind.Sequential)]

        /*Why do we need this stream0 struct - Can't we just use Vertex for the stream type?
        Yes, if we gave Vertex an explicit sequential layout. However, this approach allows us more flexibility to add data to Vertex—for example a vertex color—without having to immediately adjust SingleStream. Keep in mind that Burst will optimize away intermediate steps like copying from Vertex to Stream0.
        */
        struct Stream0 
        {
            public float3 position;
            public float3 normal;
            public float4 tangent;
            public float2 texCoord0;

            public Stream0(float3 position, float3 normal, float4 tangent, float2 texCoord0) 
            {
                this.position = position;
                this.normal = normal;
                this.tangent = tangent;
                this.texCoord0 = texCoord0;
            }
        }
        /*Without the NativeDisableContainerSafetyRestriction attribute, Unity complains that the two native arrays are aliasing. This is when
         native arrays represent overlapping data. In general this is a good warning, but we are sure the vertex and index data don't overlap, so
        we can nullify this error being thrown.*/
        [NativeDisableContainerSafetyRestriction]
        NativeArray<Stream0> stream0;
        [NativeDisableContainerSafetyRestriction]
        NativeArray<int3> triangles;
        void IMeshStreams.Setup(Mesh.MeshData data, Bounds bounds, int vertexCount, int indexCount)
        {
            NativeArray<VertexAttributeDescriptor> vertexAttributes = new NativeArray<VertexAttributeDescriptor>(VERTEX_ATTRIBUTE_COUNT, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            vertexAttributes[0] = new VertexAttributeDescriptor(attribute: VertexAttribute.Position, format: VertexAttributeFormat.Float32, dimension: 3);
            vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);
            vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4);
            vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2);
            data.SetVertexBufferParams(vertexCount, vertexAttributes);
            //After calling SetVertexBufferParams, we can retrieve native arrays for the vertex streams using GetVertexData.
            //The native array that this function returns is a pointer to the relevant section of the mesh data.

            vertexAttributes.Dispose();

            data.SetIndexBufferParams(indexCount, IndexFormat.UInt32);


            data.subMeshCount = 1;
            data.SetSubMesh(0, new SubMeshDescriptor(0, indexCount)
            {
                bounds = bounds,
                vertexCount = vertexCount
            }, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
            stream0 = data.GetVertexData<Stream0>();
            //we can copy the triangle data to the index buffer by reinterpreting the index data to int3 triangle data
            triangles = data.GetIndexData<int>().Reinterpret<int3>(4);
        }

        /*SetVertex consists of copying the vertex data to a stream0 value, then storing it at the appropriate index in the stream
         * We instruct SetVertex to be called Inline by Burst. This is just a compiler optimisation where the code of the function
         is rolled into the caller. Small methods are usually inlined automatically*/
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IMeshStreams.SetVertex(int index, Vertex data) => stream0[index] = new Stream0
        {
            position = data.position,
            normal = data.normal,
            tangent = data.tangent,
            texCoord0 = data.texCoord0
        };
        void IMeshStreams.SetTriangle(int index, int3 triangle) => triangles[index] = triangle;

    }
}

