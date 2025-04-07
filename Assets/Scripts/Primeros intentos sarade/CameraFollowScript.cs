using UnityEngine;

public class CameraFollowNew : MonoBehaviour
{
    public Transform target; // Personaje a seguir
    public Vector3 offset = new Vector3(0f, 2f, -4f); // Posici�n detr�s del personaje
    public float smoothTime = 0.15f; // Tiempo de suavizado (m�s bajo = m�s r�pido)
    private Vector3 velocity = Vector3.zero; // Velocidad de interpolaci�n

    void Start()
    {
        // Asegurar que la c�mara comience en la posici�n correcta sin rotar
        transform.position = target.position + offset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Solo seguimos la posici�n sin afectar la rotaci�n
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        // Desactivamos la rotaci�n autom�tica
        transform.rotation = Quaternion.Euler(10f, 0f, 0f); // Ajusta el �ngulo seg�n necesites
    }
}
