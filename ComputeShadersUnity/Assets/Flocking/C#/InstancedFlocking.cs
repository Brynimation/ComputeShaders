using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstancedFlocking : MonoBehaviour
{
    // Start is called before the first frame update
    public struct Boid
    {
        public Vector3 position;
        public Vector3 direction;
        public float noiseOffset; //used in the compute shader to add a small variation to each boid's speed
        public Boid(Vector3 pos, Vector3 dir, float noiseOffset)
        {
            this.position = pos;
            this.direction = dir;
            this.noiseOffset = noiseOffset;
        }
    }
    public const int BOID_STRIDE = 7 * sizeof(float);
    public ComputeShader computeShader;

    public float rotationSpeed = 1f;
    public float boidSpeed = 1f;
    public float neighbourDistance = 1f;
    public float boidSpeedVariation = 1f;
    public Mesh boidMesh;
    public Material boidMaterial;
    public int boidsCount;
    public float spawnRadius = 2f;
    public Transform target;

    int kernelHandle;
    Bounds bounds;
    ComputeBuffer boidsBuffer;
    ComputeBuffer argsBuffer;
    uint[] args = new uint[5] { 0, 0, 0, 0, 0};
    Boid[] boidsArray;
    int groupSizeX;
    int numOfBoids;
    void Start()
    {
        target = this.transform;
        kernelHandle = computeShader.FindKernel("CSMain");
        uint x; //Will hold the number of threads in a single thread group in the x dimension
        computeShader.GetKernelThreadGroupSizes(kernelHandle, out x, out _, out _);
        groupSizeX = Mathf.CeilToInt((float)boidsCount / (float)x); //calculate the minimum number of thread groups we need to spawn.
        numOfBoids = groupSizeX * (int)x;

        bounds = new Bounds(transform.position, Vector3.one * 1000); //the meshes will be drawn somewhere within a cube with 1000 unit side length
        InitBoids();
        InitShader();
    }

    private void InitBoids()
    {
        boidsArray = new Boid[numOfBoids];

        for (int i = 0; i < numOfBoids; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius; //Random.insideUnitSphere returns a Vector3 where x,y,z are in range [0,1]
         
            /*Quaternions were invented by an Irish Mathematician in 1843 called William Rowan Hamilton, and are a means to describe orientations.
             Slerp = Spherical Linear Interpolation, which rotates a vector around a sphere
             Quaternion.eulerAngles describes a rotation about the z, x then y axes (in that order)
             */
            Quaternion rot = Quaternion.Slerp(transform.rotation, Random.rotation, 0.3f); //apply a 30% rotation from the current rotation in a random direction
            float noiseOffset = Random.value * 1000.0f;
            boidsArray[i] = new Boid(pos, rot.eulerAngles, noiseOffset);
        }
    }
    private void InitShader()
    {
        //setup boids buffer
        boidsBuffer = new ComputeBuffer(numOfBoids, BOID_STRIDE);
        boidsBuffer.SetData(boidsArray);

        //setup indirect args buffer
        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

        if (boidMesh != null) 
        {
            args[0] =  boidMesh.GetIndexCount(0); //vertex count per instance 
            args[1] = (uint)numOfBoids; //number of meshes
        }
        argsBuffer.SetData(args);
        //bind buffer to compute shader
        computeShader.SetBuffer(kernelHandle, "_BoidsBuffer", boidsBuffer);
        boidMaterial.SetBuffer("_BoidsBuffer", boidsBuffer);
        computeShader.SetFloat("rotationSpeed", rotationSpeed);
        computeShader.SetFloat("boidSpeed", boidSpeed);
        computeShader.SetFloat("boidSpeedVariation", boidSpeedVariation);
        computeShader.SetVector("flockPosition", target.transform.position);
        computeShader.SetFloat("neighbourDistance", neighbourDistance);
        computeShader.SetInt("boidsCount", numOfBoids);
    }
    // Update is called once per frame
    void Update()
    {
        Boid[] tempBoidArray = new Boid[numOfBoids];
        
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            boidsBuffer.GetData(tempBoidArray);
            Debug.Log(tempBoidArray[0].position);
        }
        computeShader.SetFloat("time", Time.time);
        computeShader.SetFloat("deltaTime", Time.deltaTime);

        computeShader.Dispatch(kernelHandle, groupSizeX, 1, 1);

        //https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstancedIndirect.html
        //Draws the same mesh with the same material multiple times. Bypasses the use of unity gameobjects and just draws directly to the screen.
        Graphics.DrawMeshInstancedIndirect(boidMesh, 0, boidMaterial, bounds, argsBuffer);
    }

    private void OnDestroy()
    {
        if (boidsBuffer != null)
        {
            boidsBuffer.Dispose();
        }
        if (argsBuffer != null) 
        {
            boidsBuffer.Dispose();
        }
    }
}
