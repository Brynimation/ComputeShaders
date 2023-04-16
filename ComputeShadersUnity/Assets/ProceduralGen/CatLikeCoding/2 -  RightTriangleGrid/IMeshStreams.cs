using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

/*To store the mesh data we need to define the vertex and index buffers and then copy the relevant data in the 
 appropriate format. So we don't have to define this explicitly for each job, we use this interface. This will take care
of setting up vertex and index buffers, hiding the details of how many streams there are and what format they're in.*/
namespace ProceduralMeshes{ 
    public interface IMeshStreams 
    {
        //an object who implements the interface will have to initialise the mesh data. This will be done in the Setup method.
        void Setup(Mesh.MeshData data, Bounds bounds, int vertexCount, int indexCount);
        //It will also copy a vertex to the mesh's vertex buffer, regardless of the number of streams and the data format
        void SetVertex(int index, Vertex data);
        //It will also need to copy the appropriate indices of a triangle to the mesh's index buffer
        void SetTriangle(int index, int3 triangle);
    }
}
