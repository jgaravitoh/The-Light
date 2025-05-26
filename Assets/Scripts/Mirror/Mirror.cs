using UnityEngine;

public class MirrorCamera : MonoBehaviour
{
    // --- ROTACIÓN ---
    public bool followRotationX = true;
    public bool followRotationY = true;
    public bool followRotationZ = true;

    // --- POSICIÓN ---
    public bool followPositionX = true;
    public bool followPositionY = true;
    public bool followPositionZ = true;

    // Offset opcional para la posición
    public Vector3 positionOffset = Vector3.zero;

    void LateUpdate()
    {
        if (Camera.main == null) return;

        HandleRotation();
        HandlePosition();
    }

    private void HandleRotation()
    {
        Vector3 targetEuler = Camera.main.transform.rotation.eulerAngles;
        Vector3 currentEuler = transform.rotation.eulerAngles;

        float x = followRotationX ? targetEuler.x : currentEuler.x;
        float y = followRotationY ? -targetEuler.y + 180 : currentEuler.y;
        float z = followRotationZ ? targetEuler.z : currentEuler.z;

        transform.rotation = Quaternion.Euler(x, y, z);
    }

    private void HandlePosition()
    {
        Vector3 targetPos = Camera.main.transform.position + positionOffset;
        Vector3 currentPos = transform.position;

        float x = followPositionX ? -targetPos.x : currentPos.x;
        float y = followPositionY ? -targetPos.y : currentPos.y;
        float z = followPositionZ ? -targetPos.z : currentPos.z;

        transform.position = new Vector3(x, y, z);
    }
}
