using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralWood : MonoBehaviour
{
    public ComputeShader computeShader;
    public int texResolution = 256;

    Renderer rend;
    RenderTexture renderTexture;

    int kernelHandle;

    public Color paleColour;// = new Color();
    public Color darkColour;
    public float frequency = 2.0f;
    public float noiseScale = 6.0f;
    public float ringScale = 0.6f;
    public float contrast = 4.0f;
    void Start()
    {
        renderTexture = new RenderTexture(texResolution, texResolution, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        rend = GetComponent<Renderer>();
        rend.enabled = true;
        InitShader();

    }
    private void InitShader() 
    {
        kernelHandle = computeShader.FindKernel("CSMain");

        computeShader.SetInt("texResolution", texResolution);
        computeShader.SetVector("paleColour", paleColour);
        computeShader.SetVector("darkColour", darkColour);
        computeShader.SetFloat("frequency", frequency);
        computeShader.SetFloat("noiseScale", noiseScale);
        computeShader.SetFloat("ringScale", ringScale);
        computeShader.SetFloat("contrast", contrast);

        computeShader.SetTexture(kernelHandle, "Result", renderTexture);
        rend.material.SetTexture("_BaseMap", renderTexture);

        DispatchShader(texResolution / 8, texResolution / 8);
    }

    private void DispatchShader(int x, int y)
    {
        computeShader.Dispatch(kernelHandle, x, y, 1);
    }
    // Update is called once per frame
    void Update()
    {
        computeShader.SetFloat("time", Time.deltaTime);
        if (Input.GetKeyUp(KeyCode.U)) 
        {
            DispatchShader(texResolution / 8, texResolution / 8);
        }
    }
}
