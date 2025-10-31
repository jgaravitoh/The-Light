using UnityEngine;

public class LoadSpecificScenes : MonoBehaviour
{
    public void LoadStreetScene()
    {
        SimpleSceneLoader.SceneLoadByName("Calle");
    }
    public void LoadOfficeScene()
    {
        SimpleSceneLoader.SceneLoadByName("Office");
    }
    public void LoadGuitarScene()
    {
        SimpleSceneLoader.SceneLoadByName("Guitar Thing");
    }
    public void LoadRoomScene()
    {
        SimpleSceneLoader.SceneLoadByName("Evan's Room");
    }
}
