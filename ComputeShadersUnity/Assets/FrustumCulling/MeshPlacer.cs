using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshPlacer : MonoBehaviour
{
    public int gridSizeX;
    public int gridSizeY;
    public int gridSizeZ;
    public Mesh mesh;
    public int totalInstances;
    public ComputeShader computeShader;
    public Material mat;
    public float starSize;
    Bounds bounds;
    GameObject planeObject;
    ComputeBuffer positions;
    ComputeBuffer indirectArgsBuffer;
    int kernelHandle;
    uint[] indirectArgs = new uint[5] { 0, 0, 0, 0, 0 };
    int groupSizeX;
    uint threadGroupSize;
    public static int POSITION_STRIDE = 3 * sizeof(uint);
    void Start()
    {
        UnityEngine.Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        foreach(UnityEngine.Plane plane in planes) 
        {
            GameObject curPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            curPlane.transform.up = plane.normal;
        }
        positions = new ComputeBuffer(totalInstances, POSITION_STRIDE, ComputeBufferType.Structured);
        indirectArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        Vector3[] positionsArray = new Vector3[totalInstances];
        for (int i = 0; i < totalInstances; i++)
        {
            Vector3 randomPos = new Vector3
                (Random.Range(0, gridSizeX),
                Random.Range(0, gridSizeY),
                Random.Range(0, gridSizeZ)
                );
            positionsArray[i] = randomPos;
        }
        indirectArgs[0] = mesh.GetIndexCount(0);
        indirectArgs[1] = (uint) totalInstances;
        kernelHandle = computeShader.FindKernel("CSMain");
        bounds = new Bounds(transform.position, new Vector3(gridSizeX, gridSizeY, gridSizeZ));
        computeShader.GetKernelThreadGroupSizes(kernelHandle, out threadGroupSize, out _, out _);
        groupSizeX = Mathf.CeilToInt((float)totalInstances / (float)threadGroupSize);
        computeShader.SetBuffer(kernelHandle, "_ids", positions);
        mat.SetBuffer("_ids", positions);
    }

    // Update is called once per frame
    void Update()
    {
        //positions.SetCounterValue(0);
        mat.SetFloat("_MaxStarSize", starSize);
        computeShader.Dispatch(kernelHandle, groupSizeX, 1, 1);
        indirectArgsBuffer.SetData(indirectArgs);
        ComputeBuffer.CopyCount(positions, indirectArgsBuffer, sizeof(uint));

        Graphics.DrawMeshInstancedIndirect(mesh, 0, mat, bounds, indirectArgsBuffer);

    }
}
