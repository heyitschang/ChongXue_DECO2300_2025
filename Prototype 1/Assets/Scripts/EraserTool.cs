using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EraserTool : MonoBehaviour
{
    [Header("UI References")]
    public Button eraserButton;     

    [Header("Settings")]
    public Camera playerCamera;     
    public string erasableTag = "Brush"; 

    private bool eraserActive = false;

    void Start()
    {
        if (eraserButton != null)
            eraserButton.onClick.AddListener(ToggleEraser);

        SetButtonText("Eraser");
    }

    void ToggleEraser()
    {
        eraserActive = !eraserActive;
        SetButtonText(eraserActive ? "Erasing Active" : "Eraser");
    }

    void Update()
    {
        if (!eraserActive) return;


        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButton(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {
                    if (string.IsNullOrEmpty(erasableTag) ||
                        hit.collider.gameObject.CompareTag(erasableTag))
                    {
                        Destroy(hit.collider.gameObject);
                    }
                }
            }
        }
    }

    private void SetButtonText(string txt)
    {
        Text legacyText = eraserButton.GetComponentInChildren<Text>();
        if (legacyText != null)
            legacyText.text = txt;
    }
}
