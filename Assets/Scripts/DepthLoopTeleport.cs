using UnityEngine;

public class DepthLoopTeleport : MonoBehaviour
{
    public Transform frontBoundary;
    public Transform backBoundary;

    void Update()
    {
        Vector3 pos = transform.position;

        if (pos.z > frontBoundary.position.z)
        {
            pos.z = backBoundary.position.z;
        }
        else if (pos.z < backBoundary.position.z)
        {
            pos.z = frontBoundary.position.z;
        }

        transform.position = pos;
    }
}
