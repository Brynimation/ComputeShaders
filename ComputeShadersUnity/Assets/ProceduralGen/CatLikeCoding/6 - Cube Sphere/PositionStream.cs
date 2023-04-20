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
    public struct PositionStream : IMeshStreams
    {
        public const int VERTEX_ATTRIBUTE_COUNT = 1;

        /*Why do we need this stream0 struct - Can't we just use Vertex for the stream type?
        Yes, if we gave Vertex an explicit sequential layout. However, this approach allows us more flexibility to add data to Vertex—for example a vertex color—without having to immediately adjust SingleStream. Keep in mind that Burst will optimize away intermediate steps like copying from Vertex to Stream0.
        */

        /*Without the NativeDisableContainerSafetyRestriction attribute, Unity complains that the two native arrays are aliasing. This is when
         native arrays represent overlapping data. In general this is a good warning, but we are sure the vertex and index data don't overlap, so
        we can nullify this error being thrown.*/
        [NativeDisableContainerSafetyRestriction]
        NativeArray<float3> stream0;
        [NativeDisableContainerSafetyRestriction]
        NativeArray<int3> triangles;
        void IMeshStreams.Setup(Mesh.MeshData data, Bounds bounds, int vertexCount, int indexCount)
        {
            NativeArray<VertexAttributeDescriptor> vertexAttributes = new NativeArray<VertexAttributeDescriptor>(VERTEX_ATTRIBUTE_COUNT, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            vertexAttributes[0] = new VertexAttributeDescriptor(attribute: VertexAttribute.Position, format: VertexAttributeFormat.Float32, dimension: 3);
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
            stream0 = data.GetVertexData<float3>();
            //we can copy the triangle data to the index buffer by reinterpreting the index data to int3 triangle data
            triangles = data.GetIndexData<int>().Reinterpret<int3>(4);
        }

        /*SetVertex consists of copying the vertex data to a stream0 value, then storing it at the appropriate index in the stream
         * We instruct SetVertex to be called Inline by Burst. This is just a compiler optimisation where the code of the function
         is rolled into the caller. Small methods are usually inlined automatically*/
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IMeshStreams.SetVertex(int index, Vertex data) => stream0[index] = data.position;
        void IMeshStreams.SetTriangle(int index, int3 triangle) => triangles[index] = triangle;

    }
}

