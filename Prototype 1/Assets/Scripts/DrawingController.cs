using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DrawingController : MonoBehaviour
{
    [Header("UI References")]
    public Button activateDrawingButton;

    [Header("Color Picker")]
    public HueSliderController colorPicker;

    [Header("Drawing")]
    public GameObject brushPrefab;

    private bool isDrawingActive = false;

    void Start()
    {
        if (activateDrawingButton != null)
            activateDrawingButton.onClick.AddListener(ToggleDrawing);
    }

    void ToggleDrawing()
    {
        isDrawingActive = !isDrawingActive;
        activateDrawingButton.GetComponentInChildren<Text>().text =
            isDrawingActive ? "Drawing Active" : "Activate Drawing";
    }

    void Update()
    {
        if (isDrawingActive && Input.GetMouseButton(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 3f;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            GameObject brush = Instantiate(brushPrefab, worldPos, Quaternion.identity);

            brush.tag = "Brush";

            if (colorPicker != null && colorPicker.previewBox != null)
                SetBrushColor(brush, colorPicker.previewBox.color);
        }
    }


    void SetBrushColor(GameObject brush, Color color)
    {
        var rend = brush.GetComponent<Renderer>();
        if (rend != null) { rend.material = new Material(rend.material); rend.material.color = color; return; }

        var sr = brush.GetComponent<SpriteRenderer>();
        if (sr != null) { sr.color = color; return; }

        var img = brush.GetComponent<Image>();
        if (img != null) img.color = color;
    }
}
