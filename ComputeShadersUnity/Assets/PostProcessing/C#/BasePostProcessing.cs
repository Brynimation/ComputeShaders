using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering.Universal;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BasePostProcessing : MonoBehaviour
{
    public ComputeShader computeShader;
    protected string kernelName = "CSMain";

    protected Vector2Int texSize = new Vector2Int(0, 0);
    protected Vector2Int groupSize = new Vector2Int();//Stores the dispatch x and y parameters
    public Camera thisCamera;


    protected RenderTexture output = null; //created by the compute shader
    protected RenderTexture renderedSource = null; //stores the rendered image

    protected int kernelIndex = -1;
    protected bool initialised = false;

    protected virtual void Init() 
    {

        if (!computeShader) 
        {
            Debug.LogError("No shader");
            return;
        }
        kernelIndex = computeShader.FindKernel("CSMain");
        thisCamera = GetComponent<Camera>();
        if (!thisCamera)
        {
            Debug.LogError("No camera");
            return;
        }

        CreateTextures();
        initialised = true; 
    }

    protected void ClearTexture(ref RenderTexture textureToClear) 
    {
        if (textureToClear != null) 
        {
            textureToClear.Release();
            textureToClear = null;
        }
    }
    protected virtual void ClearTextures() 
    {
        ClearTexture(ref output);
        ClearTexture(ref renderedSource);
    }

    protected void CreateTexture(ref RenderTexture textureToMake, int divide = 1) 
    {
        textureToMake = new RenderTexture(texSize.x / divide, texSize.y / divide, 0);
        textureToMake.enableRandomWrite = true;
        textureToMake.Create();
    }
    protected virtual void CreateTextures() 
    {
        texSize.x = thisCamera.pixelWidth;
        texSize.y = thisCamera.pixelHeight;

        if (computeShader) 
        {
            uint x, y;
            computeShader.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out _); //get size of the numthreads x and y values
            groupSize.x = Mathf.CeilToInt((float)texSize.x / (float)x); //CeilToInt(float x) returns the smallest integer greater or equal to x
            groupSize.y = Mathf.CeilToInt((float)texSize.y / (float)y);
        }
        CreateTexture(ref output);
        CreateTexture(ref renderedSource);

        computeShader.SetTexture(kernelIndex, "source", renderedSource);
        computeShader.SetTexture(kernelIndex, "output", output);
    }
    protected virtual void OnEnable() 
    {
        Init();
    }
    protected virtual void OnDisable() 
    {
        ClearTextures();
        initialised = false;
    }
    protected virtual void OnDestroy() 
    {
        ClearTextures();
        initialised = false;
    }
    protected virtual void DispatchWithSource(ref RenderTexture source, ref RenderTexture destination)
    {
        Graphics.Blit(source, renderedSource); //copy source image to the compute shader's texture
        computeShader.Dispatch(kernelIndex, groupSize.x, groupSize.y, 1);
        Graphics.Blit(output, destination);//copy the compute shader's output to the destination
    }
    protected void checkResolution(out bool resChange) 
    {
        resChange = false;
        if (texSize.x != thisCamera.pixelWidth || texSize.y != thisCamera.pixelHeight) 
        {
            //check if the camera's pixel dimensions match the values of the texture's size.
            resChange = true;
            CreateTextures();
        }
    }

    //OnRenderImage is called after an image is fully rendered
    protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination) 
    {
        if (!initialised || computeShader == null)
        {
            Graphics.Blit(source, destination); //copy the source texture to the destination texture
        }
        else {
            checkResolution(out _);
            DispatchWithSource(ref source, ref destination);
        }
    }
}
