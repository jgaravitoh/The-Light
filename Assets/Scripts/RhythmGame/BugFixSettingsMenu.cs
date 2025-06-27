using UnityEngine;

public class BugFixSettingsMenu : MonoBehaviour
{
    public static BugFixSettingsMenu Instance { get; private set; }

    private void Awake()
    {
        // Implement Singleton logic
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: keep it across scene loads
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
