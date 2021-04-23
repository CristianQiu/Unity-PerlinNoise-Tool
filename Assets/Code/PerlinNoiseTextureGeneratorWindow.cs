using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public struct PerlinNoiseTextureSettings
{
    public Vector2Int resolution;

    public Vector2 offset;
    public float frequency;

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
            frequency = 5.0f,
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

        string prefix = !isPreview ? "Noise texture created in Assets folder." : "Noise texture preview was created.";
        Debug.LogFormat("{0} Texture generation took: {1} milliseconds", prefix, sw.ElapsedMilliseconds);

        // FIXME: there's a glitch where the project view asset is not refreshed until changing
        // folder and going back, not sure if there's something I should call in the asset database
        // or is just a bug
        if (isPreview)
            previewTexture = texture;
        else
            AssetDatabase.CreateAsset(texture, "Assets/PerlinNoise.asset");
    }

    #endregion
}