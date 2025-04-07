using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform player; // Asigna el jugador en el Inspector
    public Vector3 offset = new Vector3(0f, 2f, -10f); // Ajusta la distancia de la cámara
    public float smoothSpeed = 5f; // Suavidad del seguimiento

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }
    }
}
