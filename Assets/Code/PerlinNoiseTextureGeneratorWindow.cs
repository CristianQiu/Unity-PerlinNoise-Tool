using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public struct PerlinNoiseTextureSettings
{
    public Vector2Int resolution;

    public Vector2 offset;
    public float frequency;
    public int octaves;
    public float persistence;
    public float lacunarity;

    public int GetNumTexels()
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
            frequency = 1.0f,
            octaves = 6,
            persistence = 0.5f,
            lacunarity = 2.0f,
        };

        CreateTexture(true);
    }

    private void OnGUI()
    {
        if (headerStyle == null)
            CreateStyle();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Noise settings", headerStyle);

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("Box", GUILayout.MaxWidth(Screen.width));

        noiseSettings.frequency = EditorGUILayout.FloatField("Frequency", noiseSettings.frequency);
        noiseSettings.frequency = Mathf.Clamp(noiseSettings.frequency, 0.0f, float.MaxValue);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Offset", GUILayout.MaxWidth(CustomLabelMaxWidth));

        noiseSettings.offset = EditorGUILayout.Vector2Field("", noiseSettings.offset);

        GUILayout.EndHorizontal();

        noiseSettings.octaves = EditorGUILayout.IntField("Octaves", noiseSettings.octaves);
        noiseSettings.octaves = Mathf.Clamp(noiseSettings.octaves, 1, 16);
        noiseSettings.persistence = EditorGUILayout.FloatField("Persistence", noiseSettings.persistence);
        noiseSettings.persistence = Mathf.Clamp(noiseSettings.persistence, 0.0f, noiseSettings.persistence);
        noiseSettings.lacunarity = EditorGUILayout.FloatField("Lacunarity", noiseSettings.lacunarity);

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

    private void CreateStyle()
    {
        headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 13;
        headerStyle.alignment = TextAnchor.MiddleCenter;
    }

    private void DestroyPreviewTexture()
    {
        if (previewTexture != null)
            DestroyImmediate(previewTexture);
    }

    private void CreateTexture(bool isPreview)
    {
        if (isPreview)
            DestroyPreviewTexture();

        Texture2D texture = new Texture2D(noiseSettings.resolution.x, noiseSettings.resolution.y, TextureFormat.Alpha8, false);
        texture.wrapMode = TextureWrapMode.Clamp;

        Stopwatch sw = new Stopwatch();
        sw.Start();

        PerlinNoise.FillTextureWithPerlin2D(texture, noiseSettings);

        sw.Stop();

        if (!isPreview)
        {
            string prefix = "Noise texture created in Assets folder.";
            Debug.LogFormat("{0} Texture filling took: {1} milliseconds", prefix, sw.ElapsedMilliseconds);
        }

        if (isPreview)
            previewTexture = texture;
        else
        {
            bool removed = AssetDatabase.DeleteAsset("Assets/PerlinNoise.asset");
            if (removed)
                Debug.Log("Asset deleted at: Assets/PerlinNoise.asset. A new asset has been created. Rename or move it if you wish to keep several perlin noise texture assets.");

            AssetDatabase.CreateAsset(texture, "Assets/PerlinNoise.asset");
        }
    }

    #endregion
}