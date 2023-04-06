using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

//https://github.com/ellioman/Indirect-Rendering-With-Compute-Shaders
/*https://forum.unity.com/threads/graphics-drawmeshinstancedindirect-and-frustum-culling.479577/*/

struct InstanceData 
{
    public Vector4 position;
    public uint instanceId;
}
public class MeshPlacer2 : MonoBehaviour
{
    // Our shader that does frustum cull and LOD selection
    [SerializeField] ComputeShader computeShader;
    Vector3 prevCamPos;
    Quaternion prevCamRot;

    int kernel;

    // All the unique meshes registered to be rendered
    List<Mesh> meshesToRender = new List<Mesh>();

    public Mesh meshToRender;
    public Material instancedMaterial;

    public ComputeBuffer _Positions;
    public ComputeBuffer _IndirectArgsBuffer;
    public ComputeBuffer _FrustumBuffer;
    public ComputeBuffer _Results;

    uint[] indirectArgs = new uint[5] { 0, 0, 0, 0, 0 };
    public int instanceCount;
    public const int POSITION_BUFFER_STRIDE = sizeof(float) * 4 + sizeof(uint);

    int indexCount;
    // Temp storage until we can setup our ComputeBuffers
    Dictionary<string, List<Vector3>> tmpPosData = new Dictionary<string, List<Vector3>>();
    Dictionary<string, List<Quaternion>> tmpRotData = new Dictionary<string, List<Quaternion>>();


    // Camera frustum planes
    private Plane[] frustumPlanes;

    void Start()
    {
        kernel = computeShader.FindKernel("CSMain");

        indirectArgs[0] = meshToRender.GetIndexCount(0);
        indirectArgs[1] = (uint)instanceCount;
        _Positions = new ComputeBuffer(instanceCount, POSITION_BUFFER_STRIDE);
        //_VisiblePositions = new ComputeBuffer(instanceCount, POSITION_BUFFER_STRIDE * 2, ComputeBufferType.Append);
        _IndirectArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        _FrustumBuffer = new ComputeBuffer(6, sizeof(float) * 4);
        _Results = new ComputeBuffer(instanceCount, sizeof(uint), ComputeBufferType.Append);

        computeShader.SetBuffer(kernel, "_Positions", _Positions);
        computeShader.SetBuffer(kernel, "_Results", _Results);
        //computeShader.SetBuffer(kernel, "visibleList", _VisiblePositions);
        InstanceData[] positions = new InstanceData[instanceCount];
        for (int i = 0; i < instanceCount; i++)
        {
            InstanceData data = new InstanceData();
            data.position = new Vector4(Random.Range(0, 100), Random.Range(0, 100), Random.Range(0, 100), 1f);
            data.instanceId = (uint)i;
            positions[i] = data;
        }
        _Positions.SetData(positions);
        _IndirectArgsBuffer.SetData((indirectArgs));
    }

    void Update()
    {
        // Calculate camera frustum planes		
        //frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);	
        //_FrustumBuffer.SetData(frustumPlanes);
        //computeShader.SetBuffer(kernel, "cameraFrustumPlanes", _FrustumBuffer);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            InstanceData[] posns = new InstanceData[instanceCount];
            _Positions.GetData(posns);
            foreach (InstanceData data in posns)
            {
                Debug.Log(data.position + ", " + data.instanceId);
            }
            Debug.Log(posns.Length);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            uint[] visib = new uint[instanceCount];
            _Results.GetData(visib);
            foreach (uint id in visib)
            {
                Debug.Log(id);
            }

        }
        float[] planeNormals;
        ConstructFrustumPlanes(Camera.main, out planeNormals);
        computeShader.SetFloats("_CameraFrustumPlanes", planeNormals);

        if (meshToRender != null)
        {
            // Do frustum culling
            Dispatch();

            instancedMaterial.SetBuffer("_Positions", _Positions);
            instancedMaterial.SetBuffer("_Results", _Results);
            Graphics.DrawMeshInstancedIndirect(meshToRender, 0, instancedMaterial, new Bounds(Vector3.zero, new Vector3(1000.0f, 100.0f, 1000.0f)), _IndirectArgsBuffer);
        }
        prevCamPos = Camera.main.transform.position;
        prevCamRot = Camera.main.transform.rotation;
    }

    /* void OnValidate()
     {

         if (renderInfo != null)
         {
             int instanceCount = positions.Count;
             renderInfo.positionBuffer = new ComputeBuffer(instanceCount, 16);

             //positionBuffer = new ComputeBuffer(instanceCount, 16);
             Vector4[] pos = new Vector4[instanceCount];
             for (int j = 0; j < instanceCount; j++)
             {

                 //float angle = Random.RandomRange(0.0f, Mathf.PI * 2.0f);
                 //float distance = Random.RandomRange(20.0f, 100.0f);
                 //float height = Random.RandomRange(-2.0f, 2.0f);
                 float size = 0.1f;

                 Vector3 instancePos = positions[j];
                 pos[j] = new Vector4(instancePos.x, instancePos.y, instancePos.z, size);
             }
             renderInfo.positionBuffer.SetData(pos);

             // Create result buffer
             renderInfo.resultBuffer = new ComputeBuffer(instanceCount, 16, ComputeBufferType.Append);

             // indirect args
             uint numIndices = (renderInfo.instanceMesh != null) ? (uint)renderInfo.instanceMesh.GetIndexCount(0) : 0;
             uint[] args = new uint[5] { numIndices, (uint)instanceCount, 0, 0, 0 };
             renderInfo.argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
             renderInfo.argsBuffer.SetData(args);
         }
     }
    */
    /*public void RegisterInstance(Mesh mesh, Material material, Vector3 pos, float scale)
    {
        // Add unique meshes
        if (!meshesToRender.Contains(mesh.name))
        {
            // Add the mesh to our list
            meshesToRender.Add(mesh.name);

            // Create new position list
            List<Vector3> positions = new List<Vector3>();
            tmpPosData.Add(mesh.name, positions);

            // Create new gpu info for this insance of the mesh
            GPURenderInfo newGpuInfo = new GPURenderInfo();
            newGpuInfo.instanceMesh = mesh;
            newGpuInfo.instanceMaterial = material;

            gpuData.Add(mesh.name, newGpuInfo);
        }

        // Add this position to our list
        List<Vector3> posList;
        tmpPosData.TryGetValue(mesh.name, out posList);

        posList.Add(pos);
    }*/

    void Dispatch()
    {
        //renderInfo.resultBuffer.SetCounterValue(0);
        //_VisiblePositions.SetCounterValue(0);
        if (Camera.main.transform.rotation != prevCamRot || Camera.main.transform.position != prevCamPos)
        {
            _Positions.SetCounterValue(0);
        }
        //instancedMaterial.SetBuffer("_Positions", _VisiblePositions);

        int numThreadGroupsX = Mathf.CeilToInt((float)instanceCount / 256.0f);
        computeShader.Dispatch(kernel, numThreadGroupsX, 1, 1);

        //ComputeBuffer.CopyCount(_VisiblePositions, _IndirectArgsBuffer, sizeof(uint));

    }

    private void ConstructFrustumPlanes(Camera camera, out float[] planeNormals)
    {
        const int floatPerNormal = 4;

        // https://docs.unity3d.com/ScriptReference/GeometryUtility.CalculateFrustumPlanes.html
        // Ordering: [0] = Left, [1] = Right, [2] = Down, [3] = Up, [4] = Near, [5] = Far
        UnityEngine.Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

        planeNormals = new float[planes.Length * floatPerNormal];
        for (int i = 0; i < planes.Length; ++i)
        {
            planeNormals[i * floatPerNormal + 0] = planes[i].normal.x;
            planeNormals[i * floatPerNormal + 1] = planes[i].normal.y;
            planeNormals[i * floatPerNormal + 2] = planes[i].normal.z;
            planeNormals[i * floatPerNormal + 3] = planes[i].distance;
        }
    }

    void OnDestroy()
    {
        for (int i = 0; i < meshesToRender.Count; i++)
        {
            if (meshToRender != null)
            {
                _Positions.Release();
                _IndirectArgsBuffer.Release();
                _Results.Release();
            }
        }
    }

}
