using ProceduralMeshes;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static AnotherProceduralMesh;

public class MyCubeSphereDispatcher : MonoBehaviour
{
    ComputeBuffer vertexBuffer;
    ComputeBuffer indexBuffer;
    Mesh mesh;

    public const int VERTEX_ATTRIBUTE_COUNT = 4;
    void Setup(Mesh.MeshData data, Bounds bounds, int vertexCount, int indexCount)
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
        //stream0 = data.GetVertexData<Stream0>();
        //we can copy the triangle data to the index buffer by reinterpreting the index data to int3 triangle data
        //triangles = data.GetIndexData<int>().Reinterpret<int3>(4);
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
    }
}
