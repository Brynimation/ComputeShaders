using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

public class BezierMeshRenderer : MonoBehaviour
{
    private static readonly float4x4 BEZIER_MATRIX = new float4x4
    (
        -1, 3,-3, 1,
         3,-6, 3, 0,
        -3, 3, 0, 0,
         1, 0, 0 ,0
    );
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
