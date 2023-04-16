using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnotherProceduralMesh))]
public class AnotherProceduralMeshEditor : Editor
{
    public override void OnInspectorGUI() 
    {
        DrawDefaultInspector();
        AnotherProceduralMesh mesh = (AnotherProceduralMesh)target;
        if (GUILayout.Button("Generate Mesh"))
        {
            mesh.GenerateMesh();
        } 
    }
}
