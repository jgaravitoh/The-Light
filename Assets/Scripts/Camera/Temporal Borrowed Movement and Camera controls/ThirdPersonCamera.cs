using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;            // The player to follow
    public Vector3 offset = new Vector3(0, 3, -6);  // Camera offset
    public float sensitivity = 3f;      // Mouse sensitivity
    public float distance = 6f;         // Distance from target
    public float minY = -40f;           // Min vertical angle
    public float maxY = 80f;            // Max vertical angle

    private float currentX = 0f;
    private float currentY = 20f;

    void LateUpdate()
    {
        if (!target) return;

        currentX += Input.GetAxis("Mouse X") * sensitivity;
        currentY -= Input.GetAxis("Mouse Y") * sensitivity;
        currentY = Mathf.Clamp(currentY, minY, maxY);

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 direction = rotation * Vector3.back * distance;
        transform.position = target.position + direction + Vector3.up * offset.y;

        transform.LookAt(target.position + Vector3.up * offset.y);
    }
}
