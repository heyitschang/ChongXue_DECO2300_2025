using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PaintTool : MonoBehaviour
{
    public Button paintButton;       
    public Slider colorSlider;       
    public Image previewBox;         

    private bool paintMode = false;  
    private Color selectedColor = Color.white;

    void Start()
    {
        if (paintButton != null)
            paintButton.onClick.AddListener(TogglePaintMode);

        UpdateColor(colorSlider.value);
        if (colorSlider != null)
            colorSlider.onValueChanged.AddListener(UpdateColor);
    }

    void UpdateColor(float value)
    {
        selectedColor = Color.HSVToRGB(value, 1f, 1f);
        if (previewBox != null)
            previewBox.color = selectedColor;
    }

    void TogglePaintMode()
    {
        paintMode = !paintMode;
        paintButton.GetComponentInChildren<Text>().text = paintMode ? "Painting..." : "Paint";
    }

    void Update()
    {
    if (!paintMode) return;

 
    if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend != null)
            {

                rend.material = new Material(rend.material);
                rend.material.color = selectedColor;
            }
        }
    }
}
}
