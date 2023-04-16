using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProcCube : MonoBehaviour
{
    public Material mat;
    public float scale = 1f;
    public Vector3Int position;

    float adjustedScale;
    Mesh mesh;
    MeshFilter mf;
    MeshRenderer mr;
    List<Vector3> verts;
    List<int> tris;
    void Awake()
    {
        adjustedScale = scale * 0.5f; //our default cube is 2 units. This makes it one unit
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        mesh = mf.mesh;
        mr.material = mat;

    }

    private void Start()
    {
        MakeProceduralCube(adjustedScale, new Vector3(position.x * scale, position.y * scale, position.z * scale));
        UpdateMesh();
    }

    void MakeFace(int directionIndex, float cubeScale, Vector3 cubePos) 
    {
        verts.AddRange(CubeMeshData.faceVertices(directionIndex, cubeScale, cubePos));
        int vCount = verts.Count;
        tris.Add(vCount - 4);
        tris.Add(vCount - 3);
        tris.Add(vCount - 2);

        tris.Add(vCount - 4);
        tris.Add(vCount - 2);
        tris.Add(vCount - 1);

    }
    void MakeProceduralCube(float cubeScale, Vector3 cubePos) 
    {
        verts = new List<Vector3>();
        tris = new List<int>();

        for (int i = 0; i < 6; i++) 
        {
            MakeFace(i, cubeScale, cubePos);
        }
    }
    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
