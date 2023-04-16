using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace ProceduralMeshes 
{
    public struct Vertex
    {
        public float3 position;
        public float3 normal;
        public float4 tangent;
        public float2 texCoord0;

        public Vertex(float3 position, float3 normal, float4 tangent, float2 texCoord0) 
        {
            this.position = position;
            this.normal = normal;
            this.tangent = tangent;
            this.texCoord0 = texCoord0;
        }
    }
}


