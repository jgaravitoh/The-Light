using UnityEngine;

public class RotateAround : MonoBehaviour
{
    public Transform target; // The object to rotate around
    public float speed = 10f; // Speed of rotation

    void Update()
    {
        if (target != null)
        {
            // Rotate around the target's position, using the Y-axis as the rotation axis
            transform.RotateAround(target.position, Vector3.up, speed * Time.deltaTime);
        }
    }
}
