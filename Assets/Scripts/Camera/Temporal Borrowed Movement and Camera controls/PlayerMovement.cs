using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement sharedInstancePlayerMovement { get; private set; }

    private void Awake()
    {
        // Singleton setup
        if (sharedInstancePlayerMovement != null && sharedInstancePlayerMovement != this)
        {
            Destroy(gameObject);
        }
        else
        {
            sharedInstancePlayerMovement = this;
        }
    }

    public float moveSpeed = 5f;
    public bool allowMovement = true;



    void Update()
    {
        if (allowMovement) { 
            // Get input from WASD or arrow keys
            float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right
            float moveZ = Input.GetAxis("Vertical");   // W/S or Up/Down

            // Create movement vector
            Vector3 movement = new Vector3(-moveX, 0f, -moveZ);

            // Move the object
            transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);
        }
    }
}
