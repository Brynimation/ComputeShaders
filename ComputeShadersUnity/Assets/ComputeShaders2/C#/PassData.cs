using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassData : MonoBehaviour
{
    public ComputeShader shader;
    public int texResolution = 1024;

    Renderer rend;
    RenderTexture outputTexture;

    int kernelIndex;

    public Color clearColour = new Color();
    public Color circleColour = new Color();
    void Start()
    {
        outputTexture = new RenderTexture(texResolution, texResolution, 0);
        outputTexture.enableRandomWrite= true;
        outputTexture.Create();

        rend = GetComponent<Renderer>();
        rend.enabled = true;
        InitShader();
    }

    private void InitShader() 
    {
        kernelIndex = shader.FindKernel("Circles");
        shader.SetInt("texResolution", texResolution);
        shader.SetTexture(kernelIndex, "Result", outputTexture);
        rend.material.SetTexture("_BaseMap", outputTexture);
    }

    void DispatchKernel(int count) 
    {
        shader.Dispatch(kernelIndex, count, 1, 1);
    }
    // Update is called once per frame
    void Update()
    {
        DispatchKernel(1);
    }
}
