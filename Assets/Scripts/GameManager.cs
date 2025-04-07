using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public enum GameState
{
    inBattle,
    exploring,
    inMenu,
    inInventory,
    gameOver,
}
public class GameManager : MonoBehaviour
{
    public GameState currentGameState = GameState.inMenu;

    public static GameManager sharedInstanceGameManager;
    public GameObject loadingPanel;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (sharedInstanceGameManager == null)
        {
            sharedInstanceGameManager = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            InBattle();
        }
        else if (keyboard.digit2Key.wasPressedThisFrame)
        {  
            Exploring();
        }
        else if (keyboard.digit3Key.wasPressedThisFrame)
        {
            BackToMenu();
        }
        else if (keyboard.digit4Key.wasPressedThisFrame)
        {
            inInventory();
        }
        else if (keyboard.digit5Key.wasPressedThisFrame)
        {
            GameOver();
        }
    }
    public void InBattle()
    {
        SetGameState(GameState.inBattle);
    }

    public void Exploring()
    {
        SetGameState(GameState.exploring);
    }

    public void BackToMenu()
    {
        SetGameState(GameState.inMenu);
    }

    public void inInventory()
    {
        SetGameState(GameState.inInventory);
    }

    public void GameOver()
    {
        SetGameState(GameState.gameOver);
    }

    private void SetGameState(GameState newGameState)
    {
        if (newGameState == GameState.inBattle)
        {
            //TODO: Logica al estar en batalla.

        }
        else if (newGameState == GameState.exploring)
        {
            //TO-DO: Lógica al explorar

            /*LevelManager.sharedInstanceLevelManager.RemoveAllLevelBlocks();
            ReloadGame();
            Time.timeScale = 1f;
            playerController.StartGame();
            MenuManager.sharedInstance.HideMainMenu();
            MenuManager.sharedInstance.HidePausedGameMenu();
            MenuManager.sharedInstance.showInGameMenu();
            MenuManager.sharedInstance.HideGameOverMenu();*/
        }
        else if (newGameState == GameState.inMenu)
        {
            //TODO:Lógica al estar en el menu de pausa y el de inicio, sera la misma por lo que se unen.

            /*Time.timeScale = 0f;
            MenuManager.sharedInstance.showPausedGameMenu();
            MenuManager.sharedInstance.HideInGameMenu();*/
        }
        else if (newGameState == GameState.inInventory)
        {
            //TODO: Lógica al estar en el menu, esta sera especial por lo que muy probablemente no podamos poner tiempo 0, sin embargo dependiendo de la lógica elegida se harán cambios.
        }
        else if (newGameState == GameState.gameOver)
        {
            //TODO: Lógica al morir, perder la partida.
            /*MenuManager.sharedInstance.showGameOverMenu();
            MenuManager.sharedInstance.HideInGameMenu();*/
        }
        this.currentGameState = newGameState;
    }
    private void ReloadGame()
    {
        //TO-DO: logica al reiniciar el juego, nueva partida.
    }

    private void LoadGame()
    {
        //TO-DO: logica al cargar partida.
    }

    private void SaveGame()
    {
        //TO-DO: logica al guardar partida.
    }
    public void LoadScene(int sceneIndex = -1, string sceneName = null)
    {
        StartCoroutine(LoadSceneCoroutine(sceneIndex, sceneName));
    }

    IEnumerator LoadSceneCoroutine(int sceneIndex, string sceneName) 
    {
        yield return null;

        AsyncOperation asyncOperation;

        // Determine whether to load by index or by name
        if (sceneIndex >= 0)
        {
            // Activate loading screen
            loadingPanel.SetActive(true);
            asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);
        }
        else if (!string.IsNullOrEmpty(sceneName))
        {
            // Activate loading screen
            loadingPanel.SetActive(true);
            asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        }
        else
        {
            Debug.LogError("Invalid scene index or name");
            yield break;
        }

        // Don't let the Scene activate until you allow it to
        asyncOperation.allowSceneActivation = false;
        Debug.Log("Progress: " + asyncOperation.progress);

        // When the load is still in progress, output the progress
        while (!asyncOperation.isDone)
        {
            // Output the current progress
            Debug.Log("Loading progress: " + (asyncOperation.progress * 100) + "%");

            // Check if the load has finished
            if (asyncOperation.progress >= 0.9f)
            {
                asyncOperation.allowSceneActivation = true; // Activate the Scene
            }
            yield return null;
        }
        Debug.Log("Loading progress: " + (asyncOperation.progress * 100) + "%");
        // De-activate loading screen
        loadingPanel.SetActive(false);
    }
}
