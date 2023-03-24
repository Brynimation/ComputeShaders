using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbittingStars : MonoBehaviour
{
    public int starCount;
    public ComputeShader computeShader;

    public GameObject starPrefab;

    int kernelHandle;
    uint threadGroupSizeX;
    int groupSizeX;

    Transform[] stars;
    ComputeBuffer resultBuffer;
    Vector3[] output;
    /**/

    void Start()
    {
        kernelHandle = computeShader.FindKernel("OrbittingStars");
        computeShader.GetKernelThreadGroupSizes(kernelHandle, out threadGroupSizeX, out _, out _);
        groupSizeX = (int)((starCount + threadGroupSizeX - 1) / threadGroupSizeX);

        int stride = sizeof(float) * 3;
        resultBuffer = new ComputeBuffer(starCount, stride);
        computeShader.SetBuffer(kernelHandle, "Result", resultBuffer);
        output = new Vector3[starCount];

        stars = new Transform[starCount];
        for (int i = 0; i < starCount; i++) 
        {
            stars[i] = Instantiate(starPrefab, transform).transform;
        }

    }

    // Update is called once per frame
    void Update()
    {
        computeShader.SetFloat("time", Time.time);
        resultBuffer.GetData(output);
        for (int i = 0; i < starCount; i++) 
        {
            stars[i].localPosition = output[i];
        }
        computeShader.Dispatch(kernelHandle, groupSizeX, 1, 1);
    }
    private void OnDestroy()
    {
        resultBuffer.Dispose();
    }
}
