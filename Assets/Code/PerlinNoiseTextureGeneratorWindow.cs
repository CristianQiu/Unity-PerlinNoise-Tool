﻿using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public struct PerlinNoiseTextureSettings
{
    public Vector2Int resolution;

    public Vector2 origin;
    public float frequency;

    public int octaves;
    public float lacunarity; // < the rate at which frequency changes each octave
    public float persistence; // the rate at which amplitude changes each octave

    public int GetNumPixels()
    {
        return resolution.x * resolution.y;
    }
}

public class PerlinNoiseTextureGeneratorWindow : EditorWindow
{
    #region Private Attributes

    private const int CustomLabelMaxWidth = 150;

    private const int MaxResolutionX = 8192;
    private const int MaxResolutionY = 8192;

    private PerlinNoiseTextureSettings noiseSettings;

    private GUIStyle headerStyle;

    #endregion

    #region EditorWindow Methods

    [MenuItem("Window/Tools/Perlin noise texture generator")]
    private static void Init()
    {
        PerlinNoiseTextureGeneratorWindow window = (PerlinNoiseTextureGeneratorWindow)GetWindow(typeof(PerlinNoiseTextureGeneratorWindow));
        window.Show();
    }

    private void OnEnable()
    {
        noiseSettings = new PerlinNoiseTextureSettings()
        {
            resolution = new Vector2Int(256, 256),
            origin = new Vector2(0.0f, 0.0f),
            frequency = 5.0f
        };

        headerStyle = EditorStyles.boldLabel;
        headerStyle.fontSize = 13;
        headerStyle.alignment = TextAnchor.MiddleCenter;
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Noise settings", headerStyle);

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("Box", GUILayout.MaxWidth(Screen.width));

        noiseSettings.frequency = EditorGUILayout.FloatField("Frequency", noiseSettings.frequency);
        noiseSettings.frequency = Mathf.Clamp(noiseSettings.frequency, 0.01f, float.MaxValue);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Origin", GUILayout.MaxWidth(CustomLabelMaxWidth));

        noiseSettings.origin = EditorGUILayout.Vector2Field("", noiseSettings.origin);

        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Texture settings", headerStyle);

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("Box", GUILayout.MaxWidth(Screen.width));

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Resolution", GUILayout.MaxWidth(CustomLabelMaxWidth));

        noiseSettings.resolution = EditorGUILayout.Vector2IntField("", noiseSettings.resolution);
        noiseSettings.resolution.x = Mathf.Clamp(noiseSettings.resolution.x, 1, MaxResolutionX);
        noiseSettings.resolution.y = Mathf.Clamp(noiseSettings.resolution.y, 1, MaxResolutionY);

        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        //EditorGUILayout.Space();

        if (GUILayout.Button("Preview result"))
            CreateTexture();

        if (GUILayout.Button("Create texture"))
            CreateTexture();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates the perlin noise texture, with the given configured values from the window, and is
    /// saved in assets.
    /// </summary>
    private void CreateTexture()
    {
        // Note: this could probably be a one-channel texture
        Texture2D tex = new Texture2D(noiseSettings.resolution.x, noiseSettings.resolution.y, TextureFormat.RGB24, false);

        Stopwatch sw = new Stopwatch();
        sw.Start();

        Color32[] pixels = PerlinNoise.Perlin2DColors(noiseSettings);

        UnityEngine.Debug.Log("Noise generation took:" + sw.ElapsedMilliseconds + " milliseconds ");
        sw.Stop();

        tex.SetPixels32(pixels);
        tex.Apply();

        AssetDatabase.CreateAsset(tex, "Assets/PerlinNoise.asset");
    }

    #endregion
}