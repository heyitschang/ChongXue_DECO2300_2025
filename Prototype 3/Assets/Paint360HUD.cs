using UnityEngine;
using UnityEngine.UI;

public class Paint360HUD : MonoBehaviour
{
    [Header("Attach the center-eye transform (OVRCameraRig.centerEyeAnchor)")]
    public Transform eye;

    [Header("Layout")]
    public float distance = 0.6f;
    public float heightOffset = 0.15f;
    public Vector2 panelSize = new Vector2(420, 90); // in reference pixels
    public float showSeconds = 2f;

    [Header("World Space Canvas")]
    [Tooltip("Meters per reference pixel. ~0.0015 gives you a ~0.63m wide 420px panel.")]
    public float worldScale = 0.0015f;
    [Tooltip("Layer used by the main camera's culling mask. Use Default if unsure.")]
    public string canvasLayerName = "Default";

    Canvas canvas;
    RectTransform canvasRT;
    RectTransform panel;
    Text toolText;
    Image colorSwatch;

    float hideAt = -1f;

    public void Init(Transform centerEye)
    {
        eye = centerEye;
        BuildUI();
    }

    void Awake()
    {
        if (canvas == null) BuildUI();
    }

    void BuildUI()
    {
        if (canvas != null) return;

        // Canvas (World Space)
        var canvasGO = new GameObject("Paint360HUD_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // Layer: Default is safest for Quest main camera
        int layer = LayerMask.NameToLayer(canvasLayerName);
        if (layer < 0) layer = 0; // Default
        canvasGO.layer = layer;

        // RectTransform + scale in meters
        canvasRT = canvasGO.GetComponent<RectTransform>();
        canvasRT.sizeDelta = panelSize; // reference pixels — scale controls meters
        canvasRT.localScale = Vector3.one * worldScale;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        DontDestroyOnLoad(canvasGO);

        // Panel (background)
        var panelGO = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panelGO.transform.SetParent(canvas.transform, false);
        panel = panelGO.GetComponent<RectTransform>();
        panel.sizeDelta = panelSize;
        var bg = panelGO.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.55f);

        // Tool text
        var textGO = new GameObject("Tool", typeof(Text));
        textGO.transform.SetParent(panel, false);
        toolText = textGO.GetComponent<Text>();
        toolText.alignment = TextAnchor.MiddleLeft;
        toolText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        toolText.fontSize = 36;
        toolText.color = Color.white;
        var textRT = toolText.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0, 0);
        textRT.anchorMax = new Vector2(1, 1);
        textRT.offsetMin = new Vector2(20, 10);
        textRT.offsetMax = new Vector2(-120, -10);

        // Color swatch
        var swatchGO = new GameObject("Color", typeof(Image));
        swatchGO.transform.SetParent(panel, false);
        colorSwatch = swatchGO.GetComponent<Image>();
        var swRT = colorSwatch.GetComponent<RectTransform>();
        swRT.anchorMin = new Vector2(1, 0.5f);
        swRT.anchorMax = new Vector2(1, 0.5f);
        swRT.sizeDelta = new Vector2(60, 60);
        swRT.anchoredPosition = new Vector2(-30, 0);
        colorSwatch.color = Color.clear;

        canvas.enabled = false; // start hidden; Ping() will show it
    }

    void LateUpdate()
    {
        if (!eye || !canvas) return;

        // Position in front of the eye and face it
        var targetPos = eye.position + eye.forward * distance + eye.up * heightOffset;
        canvas.transform.position = Vector3.Lerp(canvas.transform.position, targetPos, 0.5f);
        canvas.transform.rotation = Quaternion.Slerp(canvas.transform.rotation, Quaternion.LookRotation(eye.forward, eye.up), 0.5f);

        // Auto-hide
        if (hideAt > 0f && Time.time >= hideAt)
        {
            canvas.enabled = false;
            hideAt = -1f;
        }
    }

    /// <summary>Show HUD for showSeconds. Pass Color.clear to hide the swatch.</summary>
    public void Ping(string toolLabel, Color color)
    {
        if (!canvas) BuildUI();
        canvas.enabled = true;
        hideAt = Time.time + showSeconds;

        toolText.text = string.IsNullOrEmpty(toolLabel) ? " " : toolLabel;

        if (color.a <= 0.001f)
            colorSwatch.color = Color.clear;
        else
        {
            color.a = 1f;
            colorSwatch.color = color;
        }
    }
}
