using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/*The aim of this script and accompanying shader is to convert our cube into a sphere. This will be done by normalizing the distance between the centre and 
 current vertex, then multiplying this normalized distance by the radius of our circle.*/
public class MeshDeform : MonoBehaviour
{
    public struct Vertex 
    {
        public Vector3 position;
        public Vector3 normal;

        public Vertex(Vector3 position, Vector3 normal) 
        {
            this.position = position;
            this.normal = normal;
        }
    }
    public ComputeShader computeShader;
    [Range(0.5f, 2.0f)]
    public float radius;
    int kernelHandle;
    Mesh mesh;

    Vertex[] vertArray; //stores the current positions of the vertices on the mesh
    Vertex[] initialArray; //stores the positions of the vertices on the original cube mesh

    ComputeBuffer vertexBuffer;
    ComputeBuffer initialBuffer;
    private void Start()
    {
        if (InitData())
        {
            InitShader();
        }
    }

    private bool InitData()
    {
        kernelHandle = computeShader.FindKernel("CSMain");

        MeshFilter mf = GetComponentInChildren<MeshFilter>();
        if (mf == null)
        {
            return false;
        }
        mesh = mf.mesh;
        InitVertexArrays(mesh);
        InitGPUBuffers();

        return true;
    }
    private void InitShader()
    {
        computeShader.SetFloat("radius", radius);
    }
    private void InitVertexArrays(Mesh mesh)
    {
        //The values in each array are initially identical. However, they are stored at entirely different memory locations so changing values of one
        //will have no impact on the other
        initialArray = new Vertex[mesh.vertices.Length];
        vertArray = new Vertex[mesh.vertices.Length];
        for (int i = 0; i < initialArray.Length; i++) 
        {
            initialArray[i] = new Vertex(mesh.vertices[i], mesh.normals[i]);
        }
        initialArray.CopyTo(vertArray, 0);
    }
    private void InitGPUBuffers()
    {
        vertexBuffer = new ComputeBuffer(vertArray.Length, sizeof(float) * 6);
        vertexBuffer.SetData(vertArray);

        initialBuffer = new ComputeBuffer(vertArray.Length, sizeof(float) * 6);
        initialBuffer.SetData(vertArray);

        computeShader.SetBuffer(kernelHandle, "vertexBuffer", vertexBuffer);
        computeShader.SetBuffer(kernelHandle, "initialBuffer", initialBuffer);
    }
    private void GetVerticesFromGPU()
    {
        vertexBuffer.GetData(vertArray);
        Vector3[] verts = new Vector3[vertArray.Length];
        Vector3[] norms = new Vector3[vertArray.Length];
        for (int i = 0; i < vertArray.Length; i++)
        {
            verts[i] = vertArray[i].position;
            norms[i] = vertArray[i].normal;
        }
        mesh.vertices = verts;
        mesh.normals = norms;
    }

    private void Update()
    {
        if (computeShader != null)
        {
            computeShader.SetFloat("radius", radius);
            float delta = (Mathf.Sin(Time.time) + 1) / 2;
            computeShader.SetFloat("delta", delta);
            computeShader.Dispatch(kernelHandle, vertArray.Length, 1, 1);
            GetVerticesFromGPU();
        }
    }
    private void OnDestroy()
    {
        
    }
}
