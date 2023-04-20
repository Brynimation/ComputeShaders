using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralMeshes;
using UnityEngine.Rendering;
using Unity.VisualScripting;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AnotherProceduralMesh : MonoBehaviour
{
    Vector3[] verts;
    Vector3[] normals;
    Vector4[] tangents;
    //With this array and corresponding MeshType enum, we can choose exactly which job, and hence which meshgenerator, to generate a mesh in the inspector
    static MeshJobScheduleDelegate[] jobs = new MeshJobScheduleDelegate[]
    {
        MeshJob<SquareGrid, SingleStream>.ScheduleParallel,
        MeshJob<SharedSquareGrid, SingleStream>.ScheduleParallel,
        MeshJob<SharedTriangleGrid, SingleStream>.ScheduleParallel,
        MeshJob<PointyHexagonGrid, SingleStream>.ScheduleParallel,
        MeshJob<FlatHexagonGrid, SingleStream>.ScheduleParallel,
        MeshJob<UVSphere, SingleStream>.ScheduleParallel,
        MeshJob<CubeSphere, SingleStream>.ScheduleParallel,
        MeshJob<SharedCubeSphere, PositionStream>.ScheduleParallel

    };
    [System.Flags]
    public enum GizmoMode {Nothing = 0, Vertices = 1, Normals = 0b10, Tangents =0b100 }
    public enum MeshType
    {
        SquareGrid, SharedSquareGrid, SharedTriangleGrid, PointyHexagonGrid, FlatHexagonGrid, UVSphere, CubeSphere, SharedCubeSphere
    }

    public GizmoMode gizmoMode;
    public MeshType meshType;
    public Material mat;
    [SerializeField, Range(1, 10)]
    int resolution = 1;
    Mesh mesh;
    MeshFilter mf;
    MeshRenderer mr;
    bool initialised;
    private void Awake()
    {
    }

    /*Generating the mesh consists of allocating writable meseh data, followed by scheduling and immediately completing a meshjob for it. We
     will do this with our SquareGrid and SingleStream types. We will apply the output of the job to the mesh.*/
    public void GenerateMesh() 
    {
        if (!initialised) 
        {
            mesh = new Mesh();
            mesh.name = "Procedural mesh";
            //GenerateMesh();
            mf = GetComponent<MeshFilter>();
            mf.mesh = mesh;
            mr = GetComponent<MeshRenderer>();
            mr.material = mat;
            initialised = true;
        }
        mr.material = mat;
        verts = mesh.vertices;
        normals = mesh.normals;
        tangents = mesh.tangents;
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];

        jobs[(int)meshType](mesh, resolution, meshData, default).Complete();
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
    }

    private void OnDrawGizmos()
    {
        if (gizmoMode == GizmoMode.Nothing || mesh == null) 
        {
            return;
        }
        bool drawVerts = (gizmoMode & GizmoMode.Vertices) != 0;
        bool drawNorms = (gizmoMode & GizmoMode.Normals) != 0;
        bool drawTangents = (gizmoMode & GizmoMode.Tangents) != 0;
        //We must convert the vertices from object space to world space. We do this with transform.TransformPoint
        //We must also convert the tangents and normals with transform.TransformDirection
        if (verts != null && drawVerts) 
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < verts.Length; i++) 
            {
                
                Gizmos.DrawSphere(transform.TransformPoint(verts[i]), 0.02f);
            }
        }
        if (normals != null && drawNorms) 
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < normals.Length; i++) 
            {
                Gizmos.DrawRay(transform.TransformPoint(verts[i]), transform.TransformDirection(normals[i] * 0.2f));
            }
        }
        if (tangents != null && drawTangents) 
        {
            Gizmos.color = Color.green;
            
            for (int i = 0; i < tangents.Length; i++) 
            {
                Gizmos.DrawRay(transform.TransformPoint(verts[i]), transform.TransformDirection(tangents[i] * 0.2f));
            }
        }
    }
}
