using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public class ProceduralGrid : MonoBehaviour
{
    Mesh mesh;
    MeshFilter mf;
    MeshRenderer mr;
    Vector3[] verts;
    int[] tris;

    //Grid settings
    public float cellSize;
    public Vector3 gridOffset;
    public int gridSize; //our grid will be a square, so width and height are both equal to size
    public Material mat;
    void Awake()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        mesh = mf.mesh;
        mr.material = mat;

    }

    private void Start()
    {
        MakeProceduralGrid();
        UpdateMesh();
    }
    // Update is called once per frame
    void MakeProceduralGrid() 
    {
        verts = new Vector3[(gridSize + 1) * (gridSize + 1)];
        tris = new int[gridSize * gridSize * 6];

        int vertexIndex = 0;
        int triangleIndex = 0;

        float vertexOffset = cellSize * 0.5f; //have the origin of a cell exist at its centre

        //Set up all verts first

        for (int x = 0; x <= gridSize; x++) 
        {
            for (int y = 0; y <= gridSize; y++) 
            {
                verts[vertexIndex] = new Vector3(((x-gridSize/2) * cellSize) - vertexOffset, 0, ((y-gridSize/2) * cellSize) - vertexOffset);
                vertexIndex++;
            }
        }
        vertexIndex = 0;
        for (int x = 0; x < gridSize; x++) 
        {
            for (int y = 0; y < gridSize; y++) 
            {
                //triangle 1
                tris[triangleIndex] = vertexIndex;
                tris[triangleIndex + 1] = vertexIndex + 1;
                tris[triangleIndex + 2] = vertexIndex + (gridSize + 1);
                //triangle 2
                tris[triangleIndex + 3] = vertexIndex + (gridSize + 1);
                tris[triangleIndex + 4] = vertexIndex + 1;
                tris[triangleIndex + 5] = vertexIndex + (gridSize + 1) + 1;

                vertexIndex++;
                triangleIndex += 6;
            }
            vertexIndex++;
        }
    }
    void UpdateMesh() 
    {
        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
    }
    
    void Update()
    {
        
    }
}
