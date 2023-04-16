using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Runtime.InteropServices;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class AdvancedSingleStreamProceduralMesh : MonoBehaviour
{

    [StructLayout(LayoutKind.Sequential)] //ensures the order of the data in the struct is maintained as it is passed to the gpu
    struct Vertex 
    {
        public float3 position;
        public float3 normal;
        public float4 tangent;
        public float2 texCoord0;

        public Vertex(float3 position, float3 normal, float4 tangent, float2 texCoord0) 
        {
            this.position = position;
            this.normal = normal;
            this.tangent = tangent;
            this.texCoord0 = texCoord0;
        }
    };

    public Material mat;

    Mesh mesh;
    MeshFilter mf;
    MeshRenderer mr;

    private void Awake()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        mr.material = mat;
    }

    private void OnEnable()
    {
        /*To write into natve mesh data we have to allocate it first. The below method does this.*/
        //MeshDataArray is simply an array of MeshData structs. We have to define the format of the mesh data ourselves using this struct*/
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];

        int vertexAttributeCount = 4; //Our vertices will contain 4 pieces of data: position, normal, tangent, uv.
        int vertexCount = 4; //We start with 4 vertices
        int triIndexCount = 6; //2 triangles to form a quad

        /*We describe these 4 attributes using a temporary NativeArray of VertexAttributeDescriptors
        we optimise our usage of native memory using the NativeArrayOptions.UninitialisedMemory parameter to our NativeArray constructor. This just 
        prevents Unity from filling an anallocated array with zeros, so saves us some precious time.
        */
        NativeArray<VertexAttributeDescriptor> vertexAttributes = new NativeArray<VertexAttributeDescriptor>(vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        vertexAttributes[0] = new VertexAttributeDescriptor(attribute: VertexAttribute.Position, format: VertexAttributeFormat.Float32, dimension: 3);
        vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);
        vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4);
        vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2);
        meshData.SetVertexBufferParams(vertexAttributeCount, vertexAttributes);
        //After calling SetVertexBufferParams, we can retrieve native arrays for the vertex streams using GetVertexData.
        //The native array that this function returns is a pointer to the relevant section of the mesh data.

        NativeArray<Vertex> verts = meshData.GetVertexData<Vertex>();

        /*
         *         NativeArray<float3> positions = meshData.GetVertexData<float3>(0); //takes stream number as input
        positions[0] = 0f; // (0,0,0)
        positions[1] = right(); //(1,0,0)
        positions[2] = up(); //(0,1,0)
        positions[3] = float3(1f, 1f, 0f);

        NativeArray<float3> normals = meshData.GetVertexData<float3>(1);
        normals[0] = normals[1] = normals[2] = normals[3] = back(); //(0, 0, -1)

        NativeArray<float4> tangents = meshData.GetVertexData<float4>(2);
        tangents[0] = tangents[1] = tangents[2] = tangents[3] = float4(1f, 0f, 0f, -1f);

        NativeArray<float2> texCoords = meshData.GetVertexData<float2>(3);
        texCoords[0] = 0f;//(0,0)
        texCoords[1] = float2(1f, 0f);
        texCoords[2] = float2(0f, 1f);
        texCoords[3] = 1f; //(1,1)

         */
        verts[0] = new Vertex(float3(0, 0, 0), back(), float4(1f, 0f, 0f, -1f), float2(0, 0));
        verts[1] = new Vertex(right(), back(), float4(1f, 0f, 0f, -1f), float2(1f, 0f));
        verts[2] = new Vertex(up(), back(), float4(1f, 0f, 0f, -1f), float2(0f, 1f));
        verts[3] = new Vertex(float3(1f, 1f, 0f), back(), float4(1f, 0f, 0f, -1f), float2(1f, 1f));

        /*We must also reserve space for the triangle indices, which is done by invoking SetIndexBufferParams. Must be done AFTER setting the vertex data*/
        meshData.SetIndexBufferParams(triIndexCount, IndexFormat.UInt32);

        //now we retrive a native pointer to the triangle index array,
        NativeArray<uint> triangleIndices = meshData.GetIndexData<uint>();
        triangleIndices[0] = 0;
        triangleIndices[1] = 2;
        triangleIndices[2] = 1;
        triangleIndices[3] = 1;
        triangleIndices[4] = 2;
        triangleIndices[5] = 3;

        //After setting the indices, we set the submesh count
        //When creating a mesh using the advanced api, unity DOES NOT automatically calculate its bounds. We can avoid it from needlessly calculating the bounds
        //of the submeshes by passing in our own bounds.
        Bounds bounds = new Bounds(Vector3.one * 0.5f, Vector3.one);
        vertexAttributes.Dispose();
        meshData.subMeshCount = 1;
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, triIndexCount)
        {
            bounds = bounds,
            vertexCount = vertexCount
        }, MeshUpdateFlags.DontRecalculateBounds);
        mesh = new Mesh();
        mesh.bounds = bounds;
        mesh.name = "Procedural mesh";
        //After applying the mesh data to the mesh with the below method, we can no longer access this mesh data
        //unless retrieved with Mesh.AcquireReadOnlyMeshData
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        mf.mesh = mesh;
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
