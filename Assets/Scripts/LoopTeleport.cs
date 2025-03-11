using UnityEngine;

public class LoopTeleport : MonoBehaviour
{
    public float leftBoundary = -10f;  // L�mite izquierdo
    public float rightBoundary = 10f;  // L�mite derecho
    public Transform cameraTransform;  // Referencia a la c�mara

    private void Update()
    {
        Vector3 position = transform.position;
        Vector3 cameraPosition = cameraTransform.position;

        if (position.x < leftBoundary)
        {
            float offset = rightBoundary - leftBoundary;
            position.x = rightBoundary;
            cameraPosition.x += offset; // Mueve la c�mara con el jugador
        }
        else if (position.x > rightBoundary)
        {
            float offset = rightBoundary - leftBoundary;
            position.x = leftBoundary;
            cameraPosition.x -= offset;
        }

        transform.position = position;
        cameraTransform.position = cameraPosition;
    }
}
