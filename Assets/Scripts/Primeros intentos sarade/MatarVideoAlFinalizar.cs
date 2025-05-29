using UnityEngine;
using UnityEngine.Video;

public class MatarVideoAlFinalizar : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject gameCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameCamera.SetActive(false);
        videoPlayer = gameObject.GetComponent<VideoPlayer>();
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        gameCamera.SetActive(true);
        Destroy(gameObject);
        // You can trigger your logic here, like loading a new scene or showing UI
    }
}
