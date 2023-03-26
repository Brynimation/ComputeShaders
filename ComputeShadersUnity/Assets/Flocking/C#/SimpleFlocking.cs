using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;

public class SimpleFlocking : MonoBehaviour
{
    // Start is called before the first frame update
    public struct Boid 
    {
        public Vector3 position;
        public Vector3 direction;

        public Boid(Vector3 pos) 
        {
            this.position.x = pos.x;
            this.position.y = pos.y;
            this.position.z = pos.z;
            direction.x = 0;
            direction.y = 0;
            direction.z = 0;
        }
    }
    public const int BOID_STRIDE = 6 * sizeof(float);
    public ComputeShader computeShader;

    public float rotationSpeed = 1f;
    public float boidSpeed = 1f;
    public float neighbourDistance = 1f;
    public float boidSpeedVariation = 1f;
    public GameObject boidPrefab;
    public int boidsCount;
    public float spawnRadius = 2f;
    public Transform target;

    int kernelHandle;
    ComputeBuffer boidsBuffer;
    Boid[] boidsArray;
    GameObject[] boids;
    int groupSizeX;
    int numOfBoids;
    void Start()
    {
        target = this.transform;
        kernelHandle = computeShader.FindKernel("CSMain");
        uint x; //Will hold the number of threads in a single thread group in the x dimension
        computeShader.GetKernelThreadGroupSizes(kernelHandle, out x, out _, out _);
        groupSizeX = Mathf.CeilToInt((float)boidsCount / (float)x); //calculate the minimum number of thread groups we need to spawn.
        numOfBoids = groupSizeX * (int) x; 

        InitBoids();
        InitShader();
    }

    private void InitBoids() 
    {
        boids = new GameObject[numOfBoids];
        boidsArray = new Boid[numOfBoids];

        for (int i = 0; i < numOfBoids; i++) 
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius; //Random.insideUnitSphere returns a Vector3 where x,y,z are in range [0,1]

            boidsArray[i] = new Boid(pos);
            boids[i] = Instantiate(boidPrefab, pos, Quaternion.identity) as GameObject;
            boidsArray[i].direction = boids[i].transform.forward;
        }
    }
    private void InitShader() 
    {
        boidsBuffer = new ComputeBuffer(numOfBoids, BOID_STRIDE);
        boidsBuffer.SetData(boidsArray);
        computeShader.SetBuffer(kernelHandle, "_BoidsBuffer", boidsBuffer);

        computeShader.SetFloat("rotationSpeed", rotationSpeed);
        computeShader.SetFloat("boidSpeed", boidSpeed);
        computeShader.SetFloat("boidSpeedVariation", boidSpeedVariation);
        computeShader.SetVector("flockPosition", target.transform.position);
        computeShader.SetFloat("neighbourDistance", neighbourDistance);
        computeShader.SetInt("boidsCount", boidsCount);
    }
    // Update is called once per frame
    void Update()
    {
        computeShader.SetFloat("time", Time.time);
        computeShader.SetFloat("deltaTime", Time.deltaTime);

        computeShader.Dispatch(kernelHandle, groupSizeX, 1, 1);

        boidsBuffer.GetData(boidsArray);

        for (int i = 0; i < boidsArray.Length; i++) 
        {
            Debug.Log(boidsArray[i].position);
            boids[i].transform.localPosition = boidsArray[i].position;
            if (!boidsArray[i].direction.Equals(Vector3.zero)) 
            {
                boids[i].transform.rotation = Quaternion.LookRotation(boidsArray[i].direction);
            }
        }
    }

    private void OnDestroy()
    {
        if (boidsBuffer != null) 
        {
            boidsBuffer.Release();
        }
    }
}
