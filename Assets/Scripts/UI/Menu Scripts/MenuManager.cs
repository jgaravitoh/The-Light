using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#region Menu's state cases
public enum MenuState
{
    notInMenu,
    inMainMenu,
    inPauseMenu,
    inSettingsMenu,
    inDeathMenu,
}
#endregion

public class MenuManager : MonoBehaviour
{
    #region Variables
    [Header("----------- Menu's State -----------")]
    public MenuState currentMenuState; 
    public static MenuManager sharedInstanceMenuManager;


    [Header("----------- UI Panels -----------")]
    // menus game objects (panels, these need to be added inside the unity editor)
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject deathMenuPanel;
    [SerializeField] private GameObject settingsMenuPanel;

    [Header("----------- Managers -----------")]
    private AudioManager audioManager;
    private GameManager gameManager;
    [Header("----------- Settings Scroll View -----------")]
    [SerializeField] private ScrollRect settingsScrollRect;
    // meow
    private Button selectedButton;  // Variable for the last selected button
    #endregion

    #region Awake, Start and Update Methods
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>(); // Gets the audio manager in the scene.
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>(); // Gets the game manager in the scene.
        DontDestroyOnLoad(gameObject);
        if (sharedInstanceMenuManager == null)
        {
            sharedInstanceMenuManager = this;
        } else {
            Destroy(gameObject);
        }

    }
    private void Start()
    {
        //currentMenuState = MenuState.inMainMenu; // By default it's set to no menu active
        if(currentMenuState != MenuState.notInMenu) { ShowMainMenu(); }
    }
    // Update is called once per frame
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            if(currentMenuState == MenuState.notInMenu)
            {
                // Activate pause menu and pause game (sets timescale to 0)
                ShowPauseMenu();
            }
            else if (currentMenuState == MenuState.inPauseMenu)
            {
                // Deactivate pause menu and unpause game (set timescale to 1)
                CloseMenu();
            }
            else if (currentMenuState == MenuState.inSettingsMenu)
            {
                // Deactivate settings menu, this leaves the pause menu active
                CloseMenu();
            }
        }
    }
    #endregion

    #region Menu's state changes
    // Change menu state functions
    public void InDeathMenu()
    {
        SetMenuState(MenuState.inDeathMenu);
    }
    public void InMainMenu()
    {
        SetMenuState(MenuState.inMainMenu);
    }
    public void InPauseMenu()
    {
        SetMenuState(MenuState.inPauseMenu);
    }
    public void InSettingsMenu()
    {
        SetMenuState(MenuState.inSettingsMenu);
    }
    public void NotInMenu()
    {
        SetMenuState(MenuState.notInMenu);
    }

    private void SetMenuState(MenuState newMenuState)
    {
        if(newMenuState == MenuState.inMainMenu)
        {
            // Go to main menu scene and activate panel
            Time.timeScale = 1;
        }
        else if (newMenuState == MenuState.notInMenu)
        {
            // Deactivate all menu panels and set TimeScale to 1
            Time.timeScale = 1;
        }
        else if (newMenuState == MenuState.inDeathMenu)
        {
            // Activate death panel and set TimeScale to 0
            Time.timeScale = 0;
        }
        else if (newMenuState == MenuState.inPauseMenu)
        {
            // Activate pause panel and set TimeScale to 0
            Time.timeScale = 0;
        }
        else if (newMenuState == MenuState.inSettingsMenu)
        {
            // Activate settings panel
        }

        this.currentMenuState = newMenuState;
    }

    #endregion
    
    #region Show and Close Panel Functions
    public void ShowSettingsMenu()
    {
        ResetButton(); // Fixes bug of button getting selected and not coming back to normal state again
        Debug.Log("Botón de opciones funciona");
        // TO DO: DESCOMENTAR LAS LINEAS DE ABAJO UNA VEZ EXISTA EL PANEL DE OPCIONES
        settingsMenuPanel.SetActive(true);  // Activate the panel
        InSettingsMenu();       // Changing Menu State
    }
    public void ShowPauseMenu()
    {
        pauseMenuPanel.SetActive(true);  // Activate the panel
        InPauseMenu();       // Changing Menu State
    }
    public void ShowDeathMenu()
    {
        deathMenuPanel.SetActive(true);  // Activate the panel
        InPauseMenu();       // Changing Menu State
    }
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        InMainMenu();
        audioManager.PlayMusicMainMenu();
    }
    // Close menus function
    public void CloseMenu()
    {
        switch (currentMenuState)
        {
            case MenuState.inPauseMenu:
                pauseMenuPanel.SetActive(false);
                NotInMenu();
                break;

            case MenuState.inMainMenu:
                mainMenuPanel.SetActive(false);
                Debug.Log("ya deberia haberse cerrado");
                NotInMenu();
                break;

            case MenuState.inSettingsMenu:
                settingsMenuPanel.SetActive(false);
                settingsScrollRect.verticalNormalizedPosition = 1f; // Resets the scroll view for the settings panel
                if      (mainMenuPanel.activeInHierarchy)  { InMainMenu();  }
                else if (deathMenuPanel.activeInHierarchy) { InDeathMenu(); }
                else if (pauseMenuPanel.activeInHierarchy) { InPauseMenu(); }
                break;

            case MenuState.inDeathMenu:
                NotInMenu();
                deathMenuPanel.SetActive(false);
                NewGame();
                break;
        }
    }

    #endregion

    #region NewGame, GoToMainMenu, ExitGame, ResetButton, UnpauseButton and Dummy Functions
    public void NewGame()
    {
        audioManager.StopAllSound();
        CloseMenu();
        gameManager.LoadScene(1); // colocar índice o nombre de la primera escena
    }
    public void GoToMainMenu()
    {
        CloseMenu();
        gameManager.LoadScene(0); // colocar índice o nombre del menú principal
        ShowMainMenu();
    }
    public void UnpauseButton()
    {
        ResetButton(); // Fixes bug of button getting selected and not coming back to normal state again
        CloseMenu();
    }
    // Quit game function
    public void ExitGame()
    {
        ResetButton(); // Fixes bug of button getting selected and not coming back to normal state again
        Debug.Log("Exiting the game...");
        Application.Quit();
    }
    public void DummyButton()
    {
        ResetButton(); // Fixes bug of button getting selected and not coming back to normal state again
        Debug.Log("Este botón sirve bien");
    }
    public void ResetButton() // Resets a button state back to the normal state
    {
        selectedButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        selectedButton.interactable = false;
        selectedButton.interactable = true;
    }
    #endregion
}