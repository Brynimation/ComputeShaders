using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*This script generates a cube by creating and managing the directions of 6 procedurally generated planes*/
public class ProceduralCube : MonoBehaviour
{
    [SerializeField] Material material;
    [SerializeField] int resolution = 10;
    [SerializeField]MeshFilter[] mfs;
    Plane[] planes;
    Vector3[] dirs = new Vector3[6] { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

    private void OnValidate()
    {
        Initialise();
        GenerateMesh();
    }
    void Initialise() 
    {
        if (mfs == null || mfs.Length == 0)
        {
            mfs = new MeshFilter[6];
        }
        planes = new Plane[6];

        for (int i = 0; i < 6; i++) 
        {
            if (mfs[i] == null) 
            {
                GameObject meshObject = new GameObject("mesh");
                meshObject.transform.SetParent(transform);
                meshObject.AddComponent<MeshRenderer>().sharedMaterial = material;
                mfs[i] = meshObject.AddComponent<MeshFilter>();
                mfs[i].sharedMesh = new Mesh();
                
            }
            planes[i] = new Plane(mfs[i].sharedMesh, resolution, dirs[i]);
        }
    }

    void GenerateMesh() 
    {
        foreach (Plane plane in planes) 
        {
            plane.ConstructMesh();
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
