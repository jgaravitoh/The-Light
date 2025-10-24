using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CameraFollowTrigger : MonoBehaviour
{
    [Header("Camera to control")]
    public CameraFollowByAxis cameraToControl;

    [Header("Target override (opcional)")]
    public Transform targetOverride; // Si lo dejas vacío, usará el objeto que entra al trigger

    [Header("Axes to follow")]
    public bool followX = true;
    public bool followY = true;
    public bool followZ = true;

    [Header("Behavior")]
    public bool snapImmediately = true;   // Coloca la cámara al instante
    public bool? lookAtOverride = null;   // null = no cambiar; true/false = forzar valor
    public string requiredTag = "Player"; // Deja vacío para aceptar cualquier collider

    void Reset()
    {
        // Asegura que el Collider sea trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!cameraToControl) return;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        Transform newTarget = targetOverride ? targetOverride : other.transform;
        cameraToControl.ConfigureFollow(newTarget, followX, followY, followZ, snapImmediately, lookAtOverride);
    }
}