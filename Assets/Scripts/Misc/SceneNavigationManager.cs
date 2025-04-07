using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneNavigationManager : MonoBehaviour
{
    public GameObject uiCanvas; // Asigna tu UI Canvas aquí
    public List<string> scenes; // Lista de nombres de las escenas

    private void Awake()
    {
        // Asegúrate de que el UI persista entre escenas
        if (uiCanvas != null)
        {
            DontDestroyOnLoad(uiCanvas);
        }
    }

    public void NextScene()
    {

        MenuManager.sharedInstanceMenuManager.ResetButton();
        int currentSceneIndex = scenes.FindIndex(scene => scene == SceneManager.GetActiveScene().name);
        if(SceneManager.GetActiveScene().name == "URP testing options base") { currentSceneIndex = 0; }
        int nextSceneIndex = (currentSceneIndex + 1) % scenes.Count;
        SceneManager.LoadScene(scenes[nextSceneIndex]);
    }

    public void PreviousScene()
    {

        MenuManager.sharedInstanceMenuManager.ResetButton();
        int currentSceneIndex = scenes.FindIndex(scene => scene == SceneManager.GetActiveScene().name);
        if (SceneManager.GetActiveScene().name == "URP testing options base") { currentSceneIndex = 0; }
        int previousSceneIndex = (currentSceneIndex - 1 + scenes.Count) % scenes.Count;
        SceneManager.LoadScene(scenes[previousSceneIndex]);
    }
}
