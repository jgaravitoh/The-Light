using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class DeleteEventSystemsOnSceneLoad : MonoBehaviour
{
    public static DeleteEventSystemsOnSceneLoad sharedInstanceEventSystem;
    private void Awake()
    {
        if (sharedInstanceEventSystem == null)
        {
            sharedInstanceEventSystem = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

    }
    void OnEnable()
    {
        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Unsubscribe from the sceneLoaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Method called when a scene is loaded
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Se llama la función al cargar la escena");
        DeleteAllEventSystemsExceptTagged();
    }

    // Function to find and delete all EventSystem objects except the one with the tag "EventSystem"
    void DeleteAllEventSystemsExceptTagged()
    {
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
        foreach (EventSystem eventSystem in eventSystems)
        {
            Debug.Log("encontró un objeto EventSystem");
            if (!eventSystem.CompareTag("EventSystem"))
            {
                Debug.Log("encontró un objeto EventSystem sin el tag");
                Destroy(eventSystem.gameObject);
            }
        }
    }
}
