using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class SceneChanger : MonoBehaviour
{
    public string sceneToLoad; // Nombre de la escena a cargar
    public VideoPlayer videoPlayer; // Referencia al VideoPlayer
    public GameObject videoCanvas; // Panel con el RawImage para el video

    private bool playerInRange = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.Z))
        {
            StartCoroutine(PlayCinematic());
        }
    }

    IEnumerator PlayCinematic()
    {
        videoCanvas.SetActive(true); // Activa el video
        videoPlayer.Play();

        // Espera hasta que el video termine
        while (videoPlayer.isPlaying || videoPlayer.frame < 1)
        {
            yield return null;
        }

        SceneManager.LoadScene(sceneToLoad); // Cambia la escena inmediatamente
    }

}