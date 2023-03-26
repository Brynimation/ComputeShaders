using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class PassData : MonoBehaviour
{
    public ComputeShader shader;
    public int texResolution = 1024;
    ComputeBuffer circleDataBuffer;

    Renderer rend;
    RenderTexture outputTexture;

    int circleKernelIndex;
    int clearKernelIndex;

    public int numThreadGroupsX;

    public Color clearColour = new Color();
    public Color circleColour = new Color();

    struct Circle 
    {
        public Vector2 origin;
        public Vector2 velocity;
        public float radius;

        public Circle(Vector2 origin, Vector2 velocity, float radius) 
        {
            this.origin = origin;
            this.velocity = velocity;
            this.radius = radius;
        }
    }

    Circle[] circleData;
    ComputeBuffer buffer;
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
        circleKernelIndex = shader.FindKernel("Circles");
        clearKernelIndex = shader.FindKernel("Clear");
        shader.SetInt("texResolution", texResolution);

        //The assignment of textures is done PER KERNEL HANDLE, which is why we need to assign the texture to both active kernels
        shader.SetTexture(circleKernelIndex, "Result", outputTexture);
        InitData();
        shader.SetTexture(clearKernelIndex, "Result", outputTexture);

        shader.SetVector("circleColour", circleColour);
        shader.SetVector("clearColour", clearColour);
        shader.SetFloat("time", Time.time);
        rend.material.SetTexture("_BaseMap", outputTexture);
    }
    private void InitData() 
    {
        //Get the number of threads in each thread group so we know how many circles to populate our array with
        uint threadGroupSizeX;
        shader.GetKernelThreadGroupSizes(circleKernelIndex, out threadGroupSizeX, out _, out _);
        int totalCircles = numThreadGroupsX * (int)threadGroupSizeX;

        float speed = 100f;
        float minRadius = 10.0f;
        float maxRadius = 30.0f;

        circleData = new Circle[totalCircles];
        for (int i = 0; i < totalCircles; i++) 
        {
            Vector2 origin = new Vector2(Random.Range(0f, 1f) * texResolution, Random.Range(0f, 1f) * texResolution);
            Vector2 velocity = new Vector2(Random.Range(0f, 1f) * speed - speed/2, Random.Range(0f, 1f) * speed - speed/2);
            float radius = Random.Range(minRadius, maxRadius);
            circleData[i] = new Circle(origin, velocity, radius);
        }
        int stride = (2 + 2 + 1) * sizeof(float);
        circleDataBuffer = new ComputeBuffer(totalCircles, stride);
        circleDataBuffer.SetData(circleData);
        shader.SetBuffer(circleKernelIndex, "circleDataBuffer", circleDataBuffer);
        
    }

    void DispatchKernels() 
    {
        shader.Dispatch(clearKernelIndex, texResolution / 8, texResolution / 8, 1);
        shader.Dispatch(circleKernelIndex, numThreadGroupsX, 1, 1);
    }
    // Update is called once per frame
    void Update()
    {
        /*The value we pass to DispatchKernels determines determines how many thread groups we dispatch*/
        DispatchKernels();
        shader.SetVector("circleColour", circleColour);
        shader.SetVector("clearColour", clearColour);
        shader.SetFloat("time", Time.time);
    }
}
