using UnityEngine;
using UnityEngine.UI;

public class RoomColorChanger : MonoBehaviour
{
    [Header("References")]
    public GameObject roomParent;   // The parent object (floor with wall children)
    public Button colorButton;      // The UI button to trigger color change

    private Renderer[] renderers;

    void Start()
    {
        if (roomParent != null)
            renderers = roomParent.GetComponentsInChildren<Renderer>();

        if (colorButton != null)
            colorButton.onClick.AddListener(ChangeRoomColor);
    }

    void ChangeRoomColor()
    {
        if (renderers != null && renderers.Length > 0)
        {
            // Pick a random color each time
            Color newColor = new Color(Random.value, Random.value, Random.value);

            // Apply to all child renderers (floor + walls)
            foreach (Renderer rend in renderers)
            {
                rend.material = new Material(rend.material); // make unique instance
                rend.material.color = newColor;
            }
        }
    }
}
