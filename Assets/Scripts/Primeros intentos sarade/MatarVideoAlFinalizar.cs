using UnityEngine;
using UnityEngine.Video;

public class MatarVideoAlFinalizar : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject gameCamera;
    public bool killAppBool;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        videoPlayer = gameObject.GetComponent<VideoPlayer>();
        if (killAppBool) { videoPlayer.loopPointReached += OnVideoEndKillApp; }
        else
        {
            gameCamera.SetActive(false);
            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        gameCamera.SetActive(true);
        Destroy(gameObject);
        // You can trigger your logic here, like loading a new scene or showing UI
    }

    void OnVideoEndKillApp(VideoPlayer vp)
    {
        Application.Quit();
    }
}
