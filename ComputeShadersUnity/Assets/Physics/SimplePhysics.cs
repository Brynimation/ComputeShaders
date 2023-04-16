using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class SimplePhysics : MonoBehaviour
{
    public struct Ball 
    {
        public Vector3 position;
        public Vector3 velocity;
        public Color colour;

        /*The constructor sets a ball somewhere within a room with walls posRange units apart, with a height of posRange.*/
        public Ball(float posRange, float maxVel) 
        {
            position.x = Random.value * posRange - posRange / 2;
            position.y = Random.value * posRange;
            position.z = Random.value * posRange - posRange / 2;

            velocity.x = Random.value * maxVel - maxVel / 2;
            velocity.y = Random.value * maxVel - maxVel / 2;
            velocity.z = Random.value * maxVel - maxVel / 2;

            colour.r = Random.value;
            colour.g = Random.value;
            colour.b = Random.value;
            colour.a = 1;
        }
    }
    public ComputeShader computeShader;
    public Mesh ballMesh;
    public float radius;
    public Material ballMaterial;
    public int ballCount = 1000;
    ComputeBuffer ballsBuffer;
    ComputeBuffer argsBuffer;
    uint[] args = new uint[5] { 0, 0, 0, 0, 0};
    int kernelHandle;

    Ball[] ballsArray;
    int groupSizeX;
    Bounds bounds;
    int numOfBalls;

    MaterialPropertyBlock mpb;

    void Start()
    {
        kernelHandle = computeShader.FindKernel("CSMain");
        uint x;
        computeShader.GetKernelThreadGroupSizes(kernelHandle, out x, out _, out _);
        groupSizeX = Mathf.CeilToInt((float)ballCount / (float)x);

        numOfBalls = groupSizeX * (int)x;

        mpb = new MaterialPropertyBlock();
        mpb.SetFloat("_UniqueID", Random.value);

        bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
        InitBalls();
        InitShader();
    }

    void InitBalls() 
    {
        ballsArray = new Ball[numOfBalls];
        for (int i = 0; i < numOfBalls; i++) 
        {
            ballsArray[i] = new Ball(4, 1.0f);
        }
    }
    void InitShader() 
    {
        ballsBuffer = new ComputeBuffer(numOfBalls, 10 * sizeof(float));
        ballsBuffer.SetData(ballsArray);

        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

        //args buffer needed for gpu instancing 
        if (ballMesh != null) 
        {
            args[0] = (uint)ballMesh.GetIndexCount(0);
            args[1] = (uint)numOfBalls;
            args[2] = (uint)ballMesh.GetIndexStart(0);
            args[3] = (uint)ballMesh.GetBaseVertex(0);
        }
        argsBuffer.SetData(args);

        computeShader.SetBuffer(kernelHandle, "ballsBuffer", ballsBuffer);
        computeShader.SetInt("ballsCount", numOfBalls);
        computeShader.SetVector("limitsXZ", new Vector4(-2.5f + radius, 2.5f - radius, -2.5f + radius, 2.5f - radius));
        computeShader.SetFloat("floorY", -2.5f + radius);

        ballMaterial.SetFloat("radius", radius * 2);
        ballMaterial.SetBuffer("ballsBuffer", ballsBuffer);
    }

    // Update is called once per frame
    void Update()
    {
        /*The iterations value means we calculate position 5 times per screen update (ie, 5 times per Update() call)*/
        int iterations = 5; 
        computeShader.SetFloat("deltaTime", Time.deltaTime / iterations);

        for(int i = 0; i < iterations; i++)
        {
            computeShader.Dispatch(kernelHandle, groupSizeX, 1, 1);
        }
        Graphics.DrawMeshInstancedIndirect(ballMesh, 0, ballMaterial, bounds, argsBuffer, 0, mpb);
    }
}
