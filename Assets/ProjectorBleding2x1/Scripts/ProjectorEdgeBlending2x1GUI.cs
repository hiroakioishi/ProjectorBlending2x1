using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class ProjectorEdgeBlending2x1GUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private PostProcessVolume postProcessVolume;

    [Header("GUI")]
    [SerializeField] private bool showGUI = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;

    [Header("Save")]
    [SerializeField] private string fileName = "projector_edge_blending_2x1_settings.json";

    [Tooltip("true: StreamingAssets Ç…ï€ë∂ / false: persistentDataPath Ç…ï€ë∂")]
    [SerializeField] private bool useStreamingAssets = true;

    [Header("GUI Position")]
    [SerializeField] private Rect windowRect = new Rect(20, 20, 420, 420);

    private ProjectorEdgeBlending2x1 settings;

    [Serializable]
    private class SaveData
    {
        public int outputWidthPixels = 3840;
        public int projectorWidthPixels = 1920;
        public int blendWidthPixels = 256;
        public float blendGamma = 1.0f;
        public int debugBlend = 0;
        public int debugUV = 0;
    }

    private void Awake()
    {
        FindSettings();

        if (settings != null)
        {
            LoadSettings();
        }
    }

    private void Update()
    {
    }

    private void FindSettings()
    {
        if (postProcessVolume == null)
        {
            postProcessVolume = FindAnyObjectByType<PostProcessVolume>();
        }

        if (postProcessVolume == null)
        {
            Debug.LogError("PostProcessVolume Ç™å©Ç¬Ç©ÇËÇ‹ÇπÇÒÅB");
            return;
        }

        if (postProcessVolume.profile == null)
        {
            Debug.LogError("PostProcessVolume Ç… Profile Ç™ê›íËÇ≥ÇÍÇƒÇ¢Ç‹ÇπÇÒÅB");
            return;
        }

        if (!postProcessVolume.profile.TryGetSettings(out settings))
        {
            Debug.LogError("PostProcessProfile Ç… ProjectorEdgeBlending2x1 Ç™í«â¡Ç≥ÇÍÇƒÇ¢Ç‹ÇπÇÒÅB");
            return;
        }
    }

    private void OnGUI()
    {
        HandleToggleKeyByOnGUI();

        if (!showGUI || settings == null)
        {
            return;
        }

        windowRect = GUI.Window(
            123456,
            windowRect,
            DrawWindow,
            "Projector Edge Blending 2x1"
        );
    }

    private void HandleToggleKeyByOnGUI()
    {
        Event e = Event.current;

        if (e == null)
        {
            return;
        }

        if (e.type == EventType.KeyDown && e.keyCode == toggleKey)
        {
            showGUI = !showGUI;
            e.Use();
        }
    }

    private void DrawWindow(int windowID)
    {
        GUILayout.Label("Press F1 to show / hide");

        GUILayout.Space(8);

        DrawIntSlider(
            "Output Width",
            settings.outputWidthPixels,
            1,
            7680
        );

        DrawIntSlider(
            "Projector Width",
            settings.projectorWidthPixels,
            1,
            3840
        );

        DrawIntSlider(
            "Blend Width",
            settings.blendWidthPixels,
            1,
            Mathf.Max(1, settings.projectorWidthPixels.value - 1)
        );

        DrawFloatSlider(
            "Blend Gamma",
            settings.blendGamma,
            0.1f,
            5.0f
        );

        GUILayout.Space(8);

        settings.debugBlend.value = GUILayout.Toggle(
            settings.debugBlend.value == 1,
            "Debug Blend"
        ) ? 1 : 0;

        settings.debugUV.value = GUILayout.Toggle(
            settings.debugUV.value == 1,
            "Debug UV"
        ) ? 1 : 0;

        GUILayout.Space(12);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Save"))
        {
            SaveSettings();
        }

        if (GUILayout.Button("Load"))
        {
            LoadSettings();
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        GUILayout.Label("Save Path:");
        GUILayout.TextArea(GetSavePath());

        GUI.DragWindow();
    }

    private void DrawIntSlider(
        string label,
        IntParameter parameter,
        int min,
        int max
    )
    {
        GUILayout.Label($"{label}: {parameter.value}");

        float value = GUILayout.HorizontalSlider(
            parameter.value,
            min,
            max
        );

        parameter.value = Mathf.RoundToInt(value);
    }

    private void DrawFloatSlider(
        string label,
        FloatParameter parameter,
        float min,
        float max
    )
    {
        GUILayout.Label($"{label}: {parameter.value:F2}");

        parameter.value = GUILayout.HorizontalSlider(
            parameter.value,
            min,
            max
        );
    }

    private string GetSavePath()
    {
        string directory;

        if (useStreamingAssets)
        {
            directory = Application.streamingAssetsPath;
        }
        else
        {
            directory = Application.persistentDataPath;
        }

        return Path.Combine(directory, fileName);
    }

    public void SaveSettings()
    {
        if (settings == null)
        {
            return;
        }

        SaveData data = new SaveData
        {
            outputWidthPixels = settings.outputWidthPixels.value,
            projectorWidthPixels = settings.projectorWidthPixels.value,
            blendWidthPixels = settings.blendWidthPixels.value,
            blendGamma = settings.blendGamma.value,
            debugBlend = settings.debugBlend.value,
            debugUV = settings.debugUV.value
        };

        string json = JsonUtility.ToJson(data, true);
        string path = GetSavePath();
        string directory = Path.GetDirectoryName(path);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, json);

        Debug.Log($"ProjectorEdgeBlending2x1 settings saved: {path}");
    }

    public void LoadSettings()
    {
        if (settings == null)
        {
            return;
        }

        string path = GetSavePath();

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Settings file not found: {path}");
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (data == null)
        {
            Debug.LogWarning("Settings file could not be loaded.");
            return;
        }

        settings.outputWidthPixels.value = data.outputWidthPixels;
        settings.projectorWidthPixels.value = data.projectorWidthPixels;
        settings.blendWidthPixels.value = data.blendWidthPixels;
        settings.blendGamma.value = data.blendGamma;
        settings.debugBlend.value = data.debugBlend;
        settings.debugUV.value = data.debugUV;

        ClampValues();

        Debug.Log($"ProjectorEdgeBlending2x1 settings loaded: {path}");
    }

    private void ClampValues()
    {
        settings.outputWidthPixels.value = Mathf.Clamp(
            settings.outputWidthPixels.value,
            1,
            7680
        );

        settings.projectorWidthPixels.value = Mathf.Clamp(
            settings.projectorWidthPixels.value,
            1,
            3840
        );

        settings.blendWidthPixels.value = Mathf.Clamp(
            settings.blendWidthPixels.value,
            1,
            Mathf.Max(1, settings.projectorWidthPixels.value - 1)
        );

        settings.blendGamma.value = Mathf.Clamp(
            settings.blendGamma.value,
            0.1f,
            5.0f
        );

        settings.debugBlend.value = Mathf.Clamp(
            settings.debugBlend.value,
            0,
            1
        );

        settings.debugUV.value = Mathf.Clamp(
            settings.debugUV.value,
            0,
            1
        );
    }
}