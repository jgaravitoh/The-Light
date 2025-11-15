using System.Collections;
using UnityEngine;

public class LoadSpecificScenes : MonoBehaviour
{
    [SerializeField] private bool waitBeforeLoading = false;
    [SerializeField] private float waitTime = 0f;
    public void LoadStreetScene()
    {
        StartCoroutine(LoadNewScene("Calle"));
    }
    public void LoadOfficeScene()
    {
        StartCoroutine(LoadNewScene("Office"));
    }
    public void LoadGuitarScene()
    {
        StartCoroutine(LoadNewScene("Guitar Thing"));
    }
    public void LoadRoomScene()
    {
        StartCoroutine(LoadNewScene("Evan's Room"));
    }
    public void LoadEvanRoomAfterGuitar()
    {
        StartCoroutine(LoadNewScene("Evan's Room No dialogo inicio"));
    }
    public void LoadEvanRoomAfterOffice()
    {
        StartCoroutine(LoadNewScene("Evan's Room PostWork"));
    }
    public void LoadEvanRoomMediano()
    {
        StartCoroutine(LoadNewScene("Evan's Room Mediano"));
    }

    public IEnumerator LoadNewScene(string sceneName)
    {
        if (waitBeforeLoading) yield return new WaitForSeconds(waitTime);
        SimpleSceneLoader.SceneLoadByName(sceneName);
    }

    

}
