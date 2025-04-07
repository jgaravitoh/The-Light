using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotatopotato : MonoBehaviour
{
    public float rotationSpeedX = 0, rotationSpeedY = 0, rotationSpeedZ = 0;
    float timeVar;

    void FixedUpdate()
    {
        timeVar = Time.deltaTime;
        transform.Rotate(new Vector3(rotationSpeedX * timeVar, rotationSpeedY * timeVar, rotationSpeedZ * timeVar));
    }
}
