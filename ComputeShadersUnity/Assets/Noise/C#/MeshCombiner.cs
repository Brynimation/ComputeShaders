using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshCombiner : MonoBehaviour
{
    [SerializeField] List<MeshFilter> sourceMeshFilters;
    [SerializeField] MeshFilter combinedMeshFilter;
    [SerializeField] MeshRenderer combinedMeshRenderer;
    [ContextMenu("Combine meshes")]
    private void CombineMeshes() 
    {
        combinedMeshRenderer = GetComponent<MeshRenderer>();
        combinedMeshRenderer.sharedMaterial = sourceMeshFilters[0].GetComponent<MeshRenderer>().sharedMaterial;
        combinedMeshFilter = GetComponent<MeshFilter>();
                
        CombineInstance[] combine = new CombineInstance[sourceMeshFilters.Count];

        for (int i = 0; i < sourceMeshFilters.Count; i++) 
        {
            combine[i].mesh = sourceMeshFilters[i].sharedMesh;
            combine[i].transform = sourceMeshFilters[i].transform.localToWorldMatrix;
        }

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);
        combinedMeshFilter.mesh = mesh;
    }
}
