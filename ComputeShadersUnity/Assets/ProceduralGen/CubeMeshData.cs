using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public static class CubeMeshData
{
    public static Vector3[] verts = new Vector3[]
    {
        new Vector3(1,1,1),
        new Vector3(-1,1,1),
        new Vector3(-1,-1,1),
        new Vector3(1,-1,1),
        new Vector3(-1,1,-1),
        new Vector3(1,1,-1),
        new Vector3(1,-1,-1),
        new Vector3(-1,-1,-1),
    }; //the 8 vertices of our cube

    public static int[][] faceTriangles = {
        new int[]{0, 1, 2, 3}, //North face of our cube
        new int[]{5, 0, 3, 6 }, //east/right face
        new int[]{4, 5, 6, 7 }, //South face
        new int[]{1, 4, 7, 2 }, //west/left
        new int[]{5, 4, 1, 0 },
        new int[]{3, 2, 7, 6 }
    };

    public static Vector3[] faceVertices(int dir, float scale, Vector3 pos) 
    {
        Vector3[] faceVerts = new Vector3[4];
        for (int i = 0; i < faceVerts.Length; i++) 
        {
            faceVerts[i] = (verts[faceTriangles[dir][i]] * scale) + pos; //faceTriangles[dir][i];
        }
        return faceVerts;
    }
}
