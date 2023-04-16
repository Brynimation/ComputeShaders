using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
public class ProceduralMesh : MonoBehaviour
{
    public Material mat;

    Mesh mesh;
    MeshFilter mf;
    MeshRenderer mr;
    Vector3[] verts;
    int[] triangles;
    void Awake()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        mesh = mf.mesh;
    }

    private void Start()
    {
        MakeMeshData();
        mr.material = mat;
        CreateMesh();
    }
    void MakeMeshData() 
    {
        verts = new Vector3[]
        {
            Vector3.zero,
            new Vector3(0.0f,0.0f,1.0f), //one unit in the positive z direction (forward)
            new Vector3(1.0f, 0.0f, 1.0f),
            new Vector3(1.0f, 0.0f, 0.0f) //one unit in the position x direction (right)
            
        };
        triangles = new int[]
        {
            0, 1, 3, //triangle 1
            1, 2, 3
        };

    }

    void CreateMesh() 
    {
        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
