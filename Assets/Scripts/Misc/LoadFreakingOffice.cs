using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadFreakingOffice : MonoBehaviour
{
    [Header("Objeto que contiene el VideoPlayer")]
    [SerializeField] private GameObject videoObject;

    [Header("Referencia al VideoPlayer")]
    [SerializeField] private VideoPlayer videoPlayer;

    [SerializeField] private float waitToPlay;

    private bool hasPlayed = false;

    private void Awake()
    {
        // Si no asignas el VideoPlayer manualmente, intenta buscarlo en el objeto del video
        if (videoPlayer == null && videoObject != null)
        {
            videoPlayer = videoObject.GetComponent<VideoPlayer>();
        }
    }

    IEnumerator WaitToStart(float wait)
    {
        yield return new WaitForSeconds(wait);
        hasPlayed = true;

        // 1️⃣ Activar objeto del video
        videoObject.SetActive(true);

        // 2️⃣ Suscribirse al evento de fin de video
        videoPlayer.loopPointReached += OnVideoFinished;

        // 3️⃣ Reproducir video
        videoPlayer.Play();

        Debug.Log("[LoadFreakingOffice] Video activado y reproduciéndose...");
    }
    public void StartVideo()
    {
        PlayerMovement.sharedInstancePlayerMovement.allowMovement = false;
        StartCoroutine(WaitToStart(waitToPlay));
    }
    /// <summary>
    /// Este método se llama automáticamente cuando el video termina.
    /// </summary>
    private void OnVideoFinished(VideoPlayer vp)
    {
        // Nos desuscribimos para evitar que se llame múltiples veces
        videoPlayer.loopPointReached -= OnVideoFinished;

        Debug.Log("[LoadFreakingOffice] El video terminó. Ejecutando acción posterior...");

        // Llamar la función que quieres que ocurra después del video
        DoAfterVideo();
    }

    /// <summary>
    /// Aquí pones TODO lo que quieres hacer cuando el video termine.
    /// </summary>
    private void DoAfterVideo()
    {
        SceneManager.LoadScene("Office");
        PlayerMovement.sharedInstancePlayerMovement.allowMovement = true;
    }
}
