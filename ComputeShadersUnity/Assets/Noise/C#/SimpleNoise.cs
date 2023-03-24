using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleNoise : MonoBehaviour
{
    public ComputeShader computeShader;
    public int texResolution = 256;

    RenderTexture renderTexture;
    Renderer rend;

    int kernelHandle;
    void Start()
    {
        renderTexture = new RenderTexture(texResolution, texResolution, 0);
        renderTexture.enableRandomWrite = true; //allows pixel and compute shaders to write to the texture
        renderTexture.Create(); //created if not already created (renderTextures are only created when they're enabled or create is explicitly called)

        rend = GetComponent<Renderer>();
        rend.enabled = true;
        InitShader();
    }

    private void InitShader() 
    {
        kernelHandle = computeShader.FindKernel("CSMain");

        computeShader.SetInt("texResolution", texResolution);
        computeShader.SetTexture(kernelHandle, "Result", renderTexture); //Textures are set PER KERNEL

        rend.material.SetTexture("_BaseMap", renderTexture);
    }

    private void DispatchShader(int x, int y) 
    {
        computeShader.SetFloat("time", Time.time);
        computeShader.Dispatch(kernelHandle, x, y, 1);
    }
    // Update is called once per frame
    void Update()
    {
        DispatchShader(texResolution / 8, texResolution / 8);
    }
}
