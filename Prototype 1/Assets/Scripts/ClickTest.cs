using UnityEngine;

public class ClickTest : MonoBehaviour
{
    void OnMouseDown()
    {
        Debug.Log("Sphere clicked!");
        GetComponent<Renderer>().material.color = Color.red;
    }
}
