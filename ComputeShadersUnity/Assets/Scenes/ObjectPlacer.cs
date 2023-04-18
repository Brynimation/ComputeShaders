using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class ObjectPlacer : MonoBehaviour
{
    public ComputeShader distanceChecker;
    public Material material;
    public float minDist;
    int numObjects = 1000;
    int kernelHandle;
    int numGroupsX;
    Vector3[] positionsArray;

    ComputeBuffer positions;
    ComputeBuffer closeEnough;
    
    
    void Start()
    {
        positions = new ComputeBuffer(numObjects, sizeof(float), ComputeBufferType.Structured);
        positionsArray = new Vector3[numObjects];
        for (int i = 0; i < numObjects; i++) 
        {
            positionsArray[i] = new Vector3(Random.Range(0, 10), Random.Range(0, 10), Random.Range(0, 10));
        }
        closeEnough = new ComputeBuffer(numObjects, sizeof(uint), ComputeBufferType.Structured);
        kernelHandle = distanceChecker.FindKernel("CSMain");
        distanceChecker.SetBuffer(kernelHandle, "_Positions", positions);
        distanceChecker.SetBuffer(kernelHandle, "_CloseEnough", closeEnough);
        material.SetBuffer("_Positions", positions);
        uint x;
        distanceChecker.GetKernelThreadGroupSizes(kernelHandle, out x, out _, out _);
        numGroupsX = Mathf.CeilToInt((float)numObjects / (float)x);
    }

    private void OnDrawGizmos()
    {
        if (positionsArray == null) return;
        foreach (Vector3 pos in positionsArray) 
        {
            Gizmos.DrawSphere(pos, 0.2f);
        }
    }


    // Update is called once per frame
    void Update()
    {
        distanceChecker.Dispatch(kernelHandle, numGroupsX, 1, 1);
        distanceChecker.SetFloat("_MinDist", minDist);
        distanceChecker.SetVector("_CameraPosition", Camera.main.transform.position);
        AsyncGPUReadback.Request(closeEnough, checkIfChangeScene);
    }
    void checkIfChangeScene(AsyncGPUReadbackRequest request) 
    {
        if (request.done && !request.hasError) 
        {
            Debug.Log("done!"); 
            NativeArray<uint> data = request.GetData<uint>();
            foreach (int val in data)
            {
                Debug.Log(val);
                if (val == 1)
                {
                    SceneManager.LoadScene(1);
                    break;
                }
            }
        }

    }
}
