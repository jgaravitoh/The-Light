using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneLoader
{
    public static void SceneLoadById(int id)
    {
        // Carga la escena por índice en Build Settings
        SceneManager.LoadScene(id);
    }

    public static void SceneLoadByName(string name)
    {
        // Carga la escena por nombre
        SceneManager.LoadScene(name);
    }
}