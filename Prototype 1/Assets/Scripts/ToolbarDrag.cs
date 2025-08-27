using UnityEngine;
using UnityEngine.EventSystems;

public class ToolbarDragWorldSpace : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    private Vector3 offset;
    public RectTransform targetPanel; 
    private Camera cam;

    void Awake()
    {
        if (targetPanel == null)
            targetPanel = transform.parent.GetComponent<RectTransform>(); 
        cam = Camera.main;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector3 worldPoint;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(targetPanel, eventData.position, cam, out worldPoint))
        {
            offset = targetPanel.position - worldPoint;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 worldPoint;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(targetPanel, eventData.position, cam, out worldPoint))
        {
            targetPanel.position = worldPoint + offset;
        }
    }
}

