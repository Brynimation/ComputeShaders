using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class AssignTexture : MonoBehaviour
{
    public ComputeShader computeShader;
    public int textureResolution = 256;
    public Color colour = Color.white;
    public float radius = 2f;
    public string kernelName = "SolidColour";

    Renderer rend;
    RenderTexture renderTexture; //A render texture is a texture that can be rendered to. Its data representation is stored on the GPU.
    int circleKernelIndex;
    void Start()
    {
        //constructor: RenderTexture(Width, height, depth). Our texture has no depth buffer
        renderTexture = new RenderTexture(textureResolution, textureResolution, 0);
        /*enabling RandomAccessWrite allows level 5.0 compute and pixel shaders to write into arbitrary locations of unorderered access views (RWBuffers and RWTextures)
         When a texture has this flag set, it can be set as the random access write target for pixel shaders using Graphics.SetRandomWriteTarget. It can also be
        written into as a RWTexture * in HLSL
         */
        renderTexture.enableRandomWrite = true;
        /*The render texture is not actually created on the hardware until we call Create() or set the texture to active. If created already, this function does nothing*/
        renderTexture.Create();

        rend = GetComponent<Renderer>();
        rend.enabled = true;
        InitialiseShader();

        
    }

    private void InitialiseShader() 
    {
        circleKernelIndex = computeShader.FindKernel(kernelName);
        computeShader.SetTexture(circleKernelIndex, "Result", renderTexture);
        computeShader.SetInt("texResolution", textureResolution);
        computeShader.SetVector("colour", colour);
        computeShader.SetFloat("radius", radius);
        rend.material.SetTexture("_BaseMap", renderTexture);

        int threadGroupSizeX = textureResolution / 8;
        int threadGroupSizeY = textureResolution / 8;
        int threadGroupSizeZ = 1;

        
        DispatchShader(circleKernelIndex, threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ);
    }

    private void DispatchShader(int circleKernelIndex, int x, int y, int z) 
    {
        //The Dispatch function defines how many thread groups to create in each dimension to run the kernel.
        //Each block of threads is identified by the semantic SV_GroupId. At
        computeShader.Dispatch(circleKernelIndex, x, y, z);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U)) 
        {
            DispatchShader(circleKernelIndex, textureResolution / 8, textureResolution / 8, 1);
        }
    }
}
