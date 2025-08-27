using UnityEngine;
using UnityEngine.EventSystems;

public class PanelResizer : MonoBehaviour, IDragHandler
{
    public RectTransform panelToResize;
    public Vector2 minSize = new Vector2(0.2f, 0.1f);  
    public Vector2 maxSize = new Vector2(2f, 1f);     

    [Range(0.001f, 0.05f)]
    public float sensitivity = 0.01f;

    public void OnDrag(PointerEventData eventData)
    {
        if (panelToResize == null) return;


        Vector3 scaleChange = new Vector3(eventData.delta.x, -eventData.delta.y, 0) * sensitivity;

        Vector3 newScale = panelToResize.localScale + scaleChange;


        newScale.x = Mathf.Clamp(newScale.x, minSize.x, maxSize.x);
        newScale.y = Mathf.Clamp(newScale.y, minSize.y, maxSize.y);

        panelToResize.localScale = newScale;
    }
}

