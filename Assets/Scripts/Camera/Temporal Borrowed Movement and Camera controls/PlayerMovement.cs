using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        // Get input from WASD or arrow keys
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float moveZ = Input.GetAxis("Vertical");   // W/S or Up/Down

        // Create movement vector
        Vector3 movement = new Vector3(moveX, 0f, moveZ);

        // Move the object
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);
    }
}
