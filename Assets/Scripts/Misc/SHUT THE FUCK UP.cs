using UnityEngine;

public class SHUTTHEFUCKUP : MonoBehaviour
{
    public AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SHUT_UP()
    {
        audioSource.Stop();
    }
}
