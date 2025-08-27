using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;       // forward/backward speed
    public float rotationSpeed = 100f; // rotation speed (degrees per second)

    void Update()
    {
        // Forward/backward movement (W/S or Up/Down arrows)
        float moveZ = Input.GetAxis("Vertical"); 
        Vector3 move = transform.forward * moveZ * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);

        // Rotation (A/D or Left/Right arrows)
        float rotationY = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up * rotationY * rotationSpeed * Time.deltaTime);
    }
}
