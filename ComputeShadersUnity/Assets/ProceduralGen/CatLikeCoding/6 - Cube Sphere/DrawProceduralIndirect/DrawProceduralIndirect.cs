using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DrawProceduralIndirect : MonoBehaviour
{
    public Material MT;
    public ComputeShader computeShader;
    [Range(1, 10)]
    public int Resolution = 10;
    public int numInstances = 1000;
    private int kernelIndex;
    private ComputeBuffer mArgBuffer;
    private Bounds mBounds;
    Vector3[] positions;
    private ComputeBuffer positionsBuffer;
    private ComputeBuffer mVertexBuffer;
    private GraphicsBuffer mIndexBuffer;

    void Start()
    {
        //Create vertex and index buffers 
        int numVertsPerInstance = Resolution * Resolution * 4 * 6; //Plane of verts made up of groups of quads. 1 plane for each of the 6 faces of a cube
        int numIndicesPerInstance = 6 * 6 * Resolution * Resolution; //indicesPerTriangle * trianglesPerQuad * 6 faces of cube * resolution^2

        positionsBuffer = new ComputeBuffer(numInstances, sizeof(float) * 3, ComputeBufferType.Structured);
        mVertexBuffer = new ComputeBuffer(numVertsPerInstance * numInstances, sizeof(float) * 3, ComputeBufferType.Structured);
        mIndexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, numIndicesPerInstance * numInstances, sizeof(uint));
        
        //Bind buffers to material and compute shader
        kernelIndex = computeShader.FindKernel("CSMain");
        MT.SetBuffer("_VertexBuffer", mVertexBuffer);
        MT.SetBuffer("_Positions", positionsBuffer);
        computeShader.SetBuffer(kernelIndex, "_VertexBuffer", mVertexBuffer);
        computeShader.SetBuffer(kernelIndex, "_Positions", positionsBuffer);
        //mIndexBuffer.SetData(GetIndexData_List());
        computeShader.SetBuffer(kernelIndex, "_IndexBuffer", mIndexBuffer);
        computeShader.SetInt("_Resolution", Resolution);
        computeShader.SetInt("_NumVertsPerInstance", numVertsPerInstance);

        positions = new Vector3[numInstances];
        for (int i = 0; i < numInstances; i++) 
        {
            positions[i] = new Vector3(Random.Range(1, 100), Random.Range(1, 100), Random.Range(1, 100));
        }
        //Additional arguments to DrawProceduralIndirect: bounds and the arguments buffer
        mBounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000));
        positionsBuffer.SetData(positions);
        mArgBuffer = new ComputeBuffer(1, sizeof(int) * 5, ComputeBufferType.IndirectArguments);
        // index-0 : index count per instance
        // index-1 : instance count
        // index-2 : start vertex location
        // index-3 : start instance location
        mArgBuffer.SetData(new int[] { numIndicesPerInstance, numInstances, 0, 0, 0});
    }


    private void Update() 
    {
        int numRows = Resolution;
        computeShader.Dispatch(kernelIndex, Resolution, Resolution, 1);
        computeShader.SetInt("_Resolution", Resolution);
        Graphics.DrawProceduralIndirect(MT, mBounds, MeshTopology.Triangles, mIndexBuffer, mArgBuffer);
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            Vector3[] data = new Vector3[numInstances];
            positionsBuffer.GetData(data);
            foreach (Vector3 x in data) 
            {
                Debug.Log(x);
            }
        }
    }

    private void OnDestroy()
    {
        if (mVertexBuffer != null)
        {
            mVertexBuffer.Release();
            mVertexBuffer = null;
        }

        if (mArgBuffer != null)
        {
            mArgBuffer.Release();
            mArgBuffer = null;
        }

        if (mIndexBuffer != null)
        {
            mIndexBuffer.Release();
            mIndexBuffer = null;
        }
    }
}
