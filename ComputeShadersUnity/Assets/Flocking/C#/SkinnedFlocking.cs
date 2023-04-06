using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SkinnedFlocking : MonoBehaviour
{
    public struct Boid
    {
        public Vector3 position;
        public Vector3 direction;
        public float noiseOffset;
        public int frame;

        public Boid(Vector3 position, Vector3 direction, float noiseOffset)
        {
            this.position = position;
            this.direction = direction;
            this.noiseOffset = noiseOffset;
            frame = 0; //which animation frame should be displayed
        }
    }
    public ComputeShader computeShader;
    private SkinnedMeshRenderer boidSMR;
    public GameObject boidObject;
    private Animator animator;
    public AnimationClip animationClip;

    private int numFrames;
    Mesh boidMesh;
    public int boidsCount;
    public float spawnRadius;
    public Transform target;
    public float rotationSpeed = 1f;
    public float boidSpeed = 1f;
    public float neighbourDistance = 1f;
    public float boidSpeedVariation = 1f;
    public float boidFrameSpeed = 10f;
    public bool frameInterpolation = true;

    private int kernelHandle;
    private ComputeBuffer boidsBuffer;
    private ComputeBuffer vertexAnimationBuffer;
    ComputeBuffer indirectArgsBuffer;
    public Material boidMaterial;
    MaterialPropertyBlock props;
    uint[] indirectArgs = new uint[5] { 0, 0, 0, 0, 0 };
    Boid[] boidsArray;
    int groupSizeX;
    int numOfBoids;
    Bounds bounds;

    void Start()
    {
        kernelHandle = computeShader.FindKernel("CSMain");
        uint x;
        computeShader.GetKernelThreadGroupSizes(kernelHandle, out x, out _, out _);
        groupSizeX = Mathf.CeilToInt((float)boidsCount / (float)x);
        numOfBoids = groupSizeX * (int)x;

        bounds = new Bounds(Vector3.zero, Vector3.one * 1000);

        //This material property block is used only for avoiding an instancing bug
        props = new MaterialPropertyBlock();
        props.SetFloat("_UniqueID", Random.value);

        InitBoids();
        GenerateVertexAnimationBuffer();
        InitShader();
    }

    void InitBoids()
    {
        boidsArray = new Boid[numOfBoids];
        for (int i = 0; i < numOfBoids; i++) 
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            Quaternion rot = Quaternion.Slerp(transform.rotation, Random.rotation, 0.3f);
            float offset = Random.value * 1000.0f;
            boidsArray[i] = new Boid(pos, rot.eulerAngles, offset);
        }
    }
    void GenerateVertexAnimationBuffer() 
    {
        boidSMR = boidObject.GetComponentInChildren<SkinnedMeshRenderer>();
        animator = boidObject.GetComponentInChildren<Animator>();
        boidMesh = boidSMR.sharedMesh;
        boidObject.SetActive(false);
    }
    void InitShader() 
    {
        indirectArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

        if (boidMesh) 
        {
            indirectArgs[0] = boidMesh.GetIndexCount(0);
            indirectArgs[1] = (uint)numOfBoids;
            indirectArgsBuffer.SetData(indirectArgs);
        }

        boidsBuffer = new ComputeBuffer(numOfBoids, 8 * sizeof(float));
        boidsBuffer.SetData(boidsArray);

        computeShader.SetFloat("rotationSpeed", rotationSpeed);
        computeShader.SetFloat("boidSpeed", boidSpeed);
        computeShader.SetFloat("boidSpeedVariation", boidSpeedVariation);
        computeShader.SetVector("flockPosition", target.transform.position);
        computeShader.SetFloat("neighbourDistance", neighbourDistance);
        computeShader.SetFloat("boidFrameSpeed", boidFrameSpeed);
        computeShader.SetInt("boidsCount", numOfBoids);
        computeShader.SetInt("numOfFrames", numFrames);
        computeShader.SetBuffer(kernelHandle, "boidsBuffer", boidsBuffer);

        boidMaterial.SetBuffer("boidsBuffer", boidsBuffer);
        boidMaterial.SetInt("numOfFrames", numFrames);

        if (frameInterpolation && !boidMaterial.IsKeywordEnabled("FRAME_INTERPOLATION")) 
        {
            boidMaterial.EnableKeyword("FRAME_INTERPOLATION");        
        }
        if (!frameInterpolation && boidMaterial.IsKeywordEnabled("FRAME_INTERPOLATION")) 
        {
            boidMaterial.DisableKeyword("FRAME_INTERPOLATION");
        }
    }

    // Update is called once per frame
    void Update()
    {
        computeShader.SetFloat("time", Time.time);
        computeShader.SetFloat("deltaTime", Time.deltaTime);

        computeShader.Dispatch(kernelHandle, groupSizeX, 1, 1);

        Graphics.DrawMeshInstancedIndirect(boidMesh, 0, boidMaterial, bounds, indirectArgsBuffer, 0, props);
    }

    private void OnDestroy()
    {
        if (boidsBuffer != null) boidsBuffer.Release();
        if (indirectArgsBuffer != null) indirectArgsBuffer.Release();
        if (vertexAnimationBuffer != null) vertexAnimationBuffer.Release();
    }
}
