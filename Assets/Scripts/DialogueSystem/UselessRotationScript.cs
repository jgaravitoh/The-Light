using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UselessRotationScript : MonoBehaviour
{
    public float rotationSpeed = 5f;
    public float orbitSpeed = 10f;
    public float orbitRadius = 3f;

    void Update()
    {
        // Rotate the cube around its own axis
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Orbit the cube around the center of the screen
        OrbitAroundCenter();
    }

    void OrbitAroundCenter()
    {
        // Calculate the new position based on time and orbit speed
        float angle = Time.time * orbitSpeed;
        float x = Mathf.Cos(angle) * orbitRadius;
        float z = Mathf.Sin(angle) * orbitRadius;

        // Set the new position
        transform.position = new Vector3(x, 0f, z);
    }
}
