using System.Collections.Generic;
using UnityEngine;
public class DrawProceduralIndirect : MonoBehaviour
{
    public Material MT;
    public ComputeShader computeShader;

    private int kernelIndex;
    private ComputeBuffer mArgBuffer;
    private Bounds mBounds;
    private ComputeBuffer mVertexBuffer;
    private GraphicsBuffer mIndexBuffer;

    void Start()
    {
        //Create vertex and index buffers 
        mVertexBuffer = new ComputeBuffer(4, sizeof(float) * 3, ComputeBufferType.Structured);
        mIndexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, 6, sizeof(uint));
        
        //Bind buffers to material and compute shader
        kernelIndex = computeShader.FindKernel("CSMain");
        MT.SetBuffer("vertArray", mVertexBuffer);
        computeShader.SetBuffer(kernelIndex, "_VertexBuffer", mVertexBuffer);
        //mIndexBuffer.SetData(GetIndexData_List());
        computeShader.SetBuffer(kernelIndex, "_IndexBuffer", mIndexBuffer);

        //Additional arguments to DrawProceduralIndirect: bounds and the arguments buffer
        mBounds = new Bounds(Vector3.zero, new Vector3(100, 100, 100)); 
        mArgBuffer = new ComputeBuffer(1, sizeof(int) * 5, ComputeBufferType.IndirectArguments);
        // index-0 : index count per instance
        // index-1 : instance count
        // index-2 : start vertex location
        // index-3 : start instance location
        mArgBuffer.SetData(new int[] { 6, 1, 0, 0, 0 });
    }


    private List<Vector3> GetVertexData_List()
    {
        var result = new List<Vector3>();

        result.Add(new Vector3(-1.5f, 0, 0));
        result.Add(new Vector3(1.5f, 0, 0));
        result.Add(new Vector3(0, 0, 1.5f));

        return result;
    }

    private List<int> GetIndexData_List()
    {
        var result = new List<int>();
        result.Add(0);
        result.Add(1);
        result.Add(2);
        return result;
    }


    private void Update() 
    {
        computeShader.Dispatch(kernelIndex, 1, 1, 1);
        Graphics.DrawProceduralIndirect(MT, mBounds, MeshTopology.Triangles, mIndexBuffer, mArgBuffer);
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            int[] data = new int[3];
            mIndexBuffer.GetData(data);
            foreach (int x in data) 
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
