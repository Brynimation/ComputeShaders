using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane
{
    Mesh mesh;
    int resolution;
    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;

    public Plane(Mesh mesh, int resolution, Vector3 localUp) 
    {
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;
        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }
    public void ConstructMesh() 
    {
        Vector3[] verts = new Vector3[resolution * resolution];
        Vector2[] uvs = new Vector2[resolution * resolution];
        //(res-1)^2 = number of subdivided faces, 2 = no trianges per face, 3 = no points per triangle
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 2 * 3];
        int triIndex = 0;
        int index = 0;
        for (int y = 0; y < resolution; y++) 
        {
            for (int x = 0; x < resolution; x++) 
            {
                index = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB;
                verts[index] = pointOnUnitCube;

                if (x != resolution - 1 && y != resolution - 1) 
                {
                    //triangle one
                    triangles[triIndex] = index;
                    triangles[triIndex + 1] = index + resolution + 1;
                    triangles[triIndex + 2] = index + resolution;
                    //triangle two
                    triangles[triIndex + 3] = index;
                    triangles[triIndex + 4] = index + 1;
                    triangles[triIndex + 5] = index + resolution + 1;

                    triIndex += 6;
                }
            }
        }
        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
