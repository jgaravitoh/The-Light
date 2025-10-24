using UnityEngine;

public class CameraFollowByAxis : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Axes to follow")]
    public bool followX = true;
    public bool followY = true;
    public bool followZ = true;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0f, 5f, -10f);
    public Space offsetSpace = Space.Self; // Self = relativo al target; World = absoluto

    [Header("Smoothing")]
    public bool smooth = true;
    [Range(0f, 20f)] public float smoothSpeed = 10f; // más alto = más rápido

    [Header("Optional")]
    public bool lookAtTarget = false;

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desired = ComputeDesired();
        Vector3 goal = ApplyAxisMask(desired, transform.position);

        // Desplazamiento (suavizado opcional)
        transform.position = smooth
            ? Vector3.Lerp(transform.position, goal, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime))
            : goal;

        if (lookAtTarget) transform.LookAt(target);
    }

    // --------------------------
    // MÉTODOS PÚBLICOS QUE PEDISTE
    // --------------------------

    /// <summary>
    /// Cambia qué ejes seguir. Si snap==true, posiciona la cámara de inmediato en el objetivo.
    /// </summary>
    public void SetAxes(bool x, bool y, bool z, bool snap = false)
    {
        followX = x; followY = y; followZ = z;

        if (snap && target != null)
        {
            Vector3 desired = ComputeDesired();
            Vector3 goal = ApplyAxisMask(desired, transform.position);
            transform.position = goal; // sin interpolación
        }
    }

    /// <summary>
    /// Configura target y ejes en una sola llamada.
    /// Si snap==true, mueve de inmediato; si lookAtOverride tiene valor, sobreescribe lookAtTarget.
    /// </summary>
    public void ConfigureFollow(Transform newTarget, bool x, bool y, bool z, bool snap = false, bool? lookAtOverride = null)
    {
        target = newTarget;
        if (lookAtOverride.HasValue) lookAtTarget = lookAtOverride.Value;
        SetAxes(x, y, z, snap);
    }

    // --------------------------
    // HELPERS PRIVADOS
    // --------------------------

    Vector3 ComputeDesired()
    {
        return target.position +
               (offsetSpace == Space.Self ? target.TransformVector(offset) : offset);
    }

    Vector3 ApplyAxisMask(Vector3 desired, Vector3 current)
    {
        return new Vector3(
            followX ? desired.x : current.x,
            followY ? desired.y : current.y,
            followZ ? desired.z : current.z
        );
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!target) return;
        Gizmos.color = Color.cyan;
        Vector3 desired = ComputeDesired();
        Gizmos.DrawLine(transform.position, desired);
        Gizmos.DrawWireSphere(desired, 0.2f);
    }
#endif
}
