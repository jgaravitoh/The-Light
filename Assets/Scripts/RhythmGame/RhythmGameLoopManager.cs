using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RhythmGameLoopManager : MonoBehaviour
{
    public static RhythmGameLoopManager Instance { get; private set; }
    [SerializeField]private TMP_Text missCounterText;

    [SerializeField] public int max_misses = 10;
    public int current_misses = 0;

    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject welcomePanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Ensures only one instance exists
            return;
        }

        Instance = this;
        Time.timeScale = 0f;
    }

    void Start()
    {
        current_misses = 0;

        UpdateMissCounter(current_misses);
    }

    void Update()
    {
        if (current_misses >= max_misses)
        {
            Time.timeScale = 0;
            ShowLose();
            // Show losing panel
        }
        if(Input.GetKeyDown("escape") && !winPanel.activeSelf && !losePanel.activeSelf && !settingsPanel.activeSelf) { ShowPause(); }
    }
    
    
    public void AddMissCounter()
    {
        current_misses++;
        UpdateMissCounter(current_misses);
    }
    private void UpdateMissCounter(int misses)
    {
        missCounterText.text =  "Misses " +misses+"/"+max_misses;
    }



    
    public void ShowSettings() { settingsPanel.SetActive(true); }
    public void HideWelcome() { welcomePanel.SetActive(false); }

    public void ShowWelcome() { welcomePanel.SetActive(true); Time.timeScale = 0; }
    public void HideSettings() { settingsPanel.SetActive(false); }


    public void ShowPause() { pausePanel.SetActive(true); Time.timeScale = 0; RhythmGameManager.sharedInstanceRythmGameManager.PauseAudio(); }
    public void HidePause() { pausePanel.SetActive(false); Time.timeScale = 1; RhythmGameManager.sharedInstanceRythmGameManager.UnpauseAudio(); }

    public void ShowLose() { losePanel.SetActive(true); Time.timeScale = 0; RhythmGameManager.sharedInstanceRythmGameManager.StopAudio(); }
    public void HideLose() { losePanel.SetActive(false); Time.timeScale = 1; }

    public void ShowWin() { winPanel.SetActive(true); Time.timeScale = 0; }
    public void HideWin() { winPanel.SetActive(false); Time.timeScale = 1; }

    public void RestartGame() { SceneManager.LoadScene(0); }
    public void StartGame()
    {
        HideWelcome();
        Time.timeScale = 1;
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
