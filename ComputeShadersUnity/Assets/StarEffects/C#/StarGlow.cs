using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarGlow : MonoBehaviour
{
    [Range(0, 1)]
    public float threshold = 1f;
    [Range(0, 10)]
    public float intensity = 1f;
    [Range(1, 20)]
    public int divide = 3;
    [Range(1, 5)]
    public int iteration = 5;
    [Range(0, 1)]
    public float attenuation = 1f;
    [Range(0, 360)]
    public float angleOfStreak = 0f;
    [Range(1, 16)]
    public int numStreaks = 4;

    public Material material;
    public Color colour = Color.white;
    private int compositeTexId = 0;
    private int compositeColourId = 0;
    private int brightnessSettingsId = 0;
    private int iterationID = 0;
    private int offsetID = 0;

    private void Start()
    {
        compositeTexId = Shader.PropertyToID("_CompositeTex");
        compositeColourId = Shader.PropertyToID("_CompositeColor");
        brightnessSettingsId = Shader.PropertyToID("_Iteration");
        offsetID = Shader.PropertyToID("_Offset");
    }
}
