using UnityEngine;

public class PlayerTriggerDetector : MonoBehaviour
{
    public HumiditySensor humiditySensor; // Referencia al script del sensor

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            humiditySensor.SetPlayerInTrigger(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            humiditySensor.SetPlayerInTrigger(false);
        }
    }
}
