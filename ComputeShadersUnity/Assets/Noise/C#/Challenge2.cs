using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Challenge2 : MonoBehaviour
{
    public ComputeShader computeShader;
    public int texResolution;

    public Color fillColour;
    public Color clearColour;
    public float radius;
    public float edgeThickness;
    public int numSides;

    RenderTexture renderTexture;
    Renderer rend;

    int kernelIndex;
    void Start()
    {
        kernelIndex = computeShader.FindKernel("CSMain");

        renderTexture = new RenderTexture(texResolution, texResolution, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        rend = GetComponent<Renderer>();
        rend.enabled = true;

        InitShader();
    }
    void InitShader() 
    {
        kernelIndex = computeShader.FindKernel("CSMain");

        computeShader.SetInt("texResolution", texResolution);
        computeShader.SetVector("fillColour", fillColour);
        computeShader.SetVector("clearColour", clearColour);

        computeShader.SetTexture(kernelIndex, "Result", renderTexture);
        rend.material.SetTexture("_BaseMap", renderTexture);
    }

    private void DispatchShader(int x, int y) 
    {
        computeShader.SetFloat("time", Time.time);
        computeShader.SetFloat("radius", radius);
        computeShader.SetFloat("edgeThickness", edgeThickness);
        computeShader.SetInt("numSides", numSides);
        computeShader.Dispatch(kernelIndex, x, y, 1);
    }

    // Update is called once per frame
    void Update()
    {
        DispatchShader(texResolution / 8, texResolution / 8);
    }
}
