using System.Collections;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [Header("Settings")]
    public Light targetLight; // The light to pulse
    public float minIntensity = 0.5f; // Minimum light intensity
    public float maxIntensity = 2f; // Maximum light intensity
    public float pulseSpeed = 2f; // Speed of the pulsing effect

    private void Update()
    {
        if (targetLight != null)
        {
            // Calculate the pulsing intensity using PingPong
            float intensityRange = maxIntensity - minIntensity;
            targetLight.intensity = minIntensity + Mathf.PingPong(Time.time * pulseSpeed, intensityRange);
        }
        else
        {
            Debug.LogWarning("Target light is not assigned.");
        }
    }
}
