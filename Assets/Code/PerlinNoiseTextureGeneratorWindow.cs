﻿using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public struct PerlinNoiseTextureSettings
{
    public Vector2Int resolution;

    public Vector2 offset;
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
    private Texture2D previewTexture;

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
            offset = new Vector2(0.0f, 0.0f),
            frequency = 5.0f
        };

        headerStyle = new GUIStyle(EditorStyles.boldLabel);
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

        EditorGUILayout.LabelField("Offset", GUILayout.MaxWidth(CustomLabelMaxWidth));

        noiseSettings.offset = EditorGUILayout.Vector2Field("", noiseSettings.offset);

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

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Preview result"))
            CreateTexture(true);

        if (GUILayout.Button("Create texture"))
            CreateTexture(false);

        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        Rect r = EditorGUILayout.GetControlRect();
        r.height = Mathf.Abs(Screen.height - r.y) - 25.0f;
        r.height = Mathf.Clamp(r.height, 0.0f, float.MaxValue);

        if (previewTexture != null)
            EditorGUI.DrawPreviewTexture(r, previewTexture, null, ScaleMode.ScaleToFit);
    }

    private void OnDisable()
    {
        DestroyPreviewTexture();
    }

    #endregion

    #region Methods

    private void DestroyPreviewTexture()
    {
        if (previewTexture != null)
            DestroyImmediate(previewTexture);
    }

    private void CreateTexture(bool isPreview)
    {
        if (isPreview)
            DestroyPreviewTexture();

        // Note: this could probably be a one-channel texture
        Texture2D texture = new Texture2D(noiseSettings.resolution.x, noiseSettings.resolution.y, TextureFormat.RGB24, false);
        texture.wrapMode = TextureWrapMode.Clamp;

        Stopwatch sw = new Stopwatch();
        sw.Start();

        Color32[] pixels = PerlinNoise.Perlin2DColors(noiseSettings);

        UnityEngine.Debug.Log("Noise generation took:" + sw.ElapsedMilliseconds + " milliseconds ");
        sw.Stop();

        texture.SetPixels32(pixels);
        texture.Apply();

        previewTexture = texture;

        if (!isPreview)
            AssetDatabase.CreateAsset(texture, "Assets/PerlinNoise.asset");
    }

    #endregion
}