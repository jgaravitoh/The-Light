using UnityEngine;

public class CameraFollowNew : MonoBehaviour
{
    public Transform target; // Personaje a seguir
    public Vector3 offset = new Vector3(0f, 2f, -4f); // Posición detrás del personaje
    public float smoothTime = 0.15f; // Tiempo de suavizado (más bajo = más rápido)
    private Vector3 velocity = Vector3.zero; // Velocidad de interpolación

    void Start()
    {
        // Asegurar que la cámara comience en la posición correcta sin rotar
        transform.position = target.position + offset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Solo seguimos la posición sin afectar la rotación
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        // Desactivamos la rotación automática
        transform.rotation = Quaternion.Euler(10f, 0f, 0f); // Ajusta el ángulo según necesites
    }
}
