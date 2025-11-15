using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArticleSystemManager : MonoBehaviour
{
    // Variables para el minijuego
    [SerializeField] private List<Button> buttonList;
    [SerializeField] private TMP_InputField minigameText;
    [SerializeField] private GameObject minigameButtonsUI;
    [SerializeField] private GameObject minigameWrongAnswerPanel;
    [SerializeField] private GameObject minigameMakeTimePassPanel;
    [SerializeField] private GameObject computerPanel;
    [SerializeField] private GameObject computerCameraZone;

    private const int DIALOGUE_EVAN_MISTAKE_SELECTION = 18;
    private const int DIALOGUE_EVAN_WANTS_TO_GO_HOME_SEQUENCE = 19;
    private const int ARTICLES_MAXIMUM_TO_WRITE = 3;

    private int minigameArticlesWritten = 0;
    private int[] randomizedArticleIndexes;

    private int[] _currentOptionIndexByButton;
    private int _currentCorrectButtonSlot = -1;

    // STATE FOR OPTION SELECTION
    private bool _awaitingSelection = false;
    private string _selectedOptionText = null;

    // Variables para eventos y control de estados
    private bool _subscribedToTable = false;
    private bool _subscribedToDialogue = false;        // DialogueEnded
    private bool _subscribedToDialogueReady = false;   // DialogueTableReady

    private bool _articlesReady = false;
    private bool _dialoguesReady = false;
    private bool _minigameStarted = false;            // one-shot
    private bool _tutorialEnded = false;              // ← se pondrá en true desde NotifyTutorialEnded()

    // Prop opcional por si quieres consultarlo desde fuera
    public bool TutorialEnded => _tutorialEnded;

    public GameObject afterMinigameHitboxDialogue, afterMinigameHitboxLoadScene;

    #region Loading CSV ARTICLE DATA
    // --- Singleton ---
    public static ArticleSystemManager Instance { get; private set; }

    // Reset static state when entering Play Mode with "Enter Play Mode Options" (no domain reload).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
        TableLoaded = null;
    }

    // --- Config / Data ---
    private string address = "Assets/CSV Files/ArticlesMinigame.csv"; // Addressables TextAsset key
    public ArticleTable Table { get; private set; }

    // Fired when the CSV is loaded and parsed successfully.
    public static event Action<ArticleTable> TableLoaded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Load(); // auto-load on startup
    }

    // Public: force reload (e.g., after changing Addressables key at runtime)
    public void Load() => LoadFromAddress(address);

    public void SetAddressAndReload(string newAddress)
    {
        address = newAddress;
        Load();
    }

    private void LoadFromAddress(string addr)
    {
        ArticleCSVLoader.LoadAsync(addr, table =>
        {
            Table = table;
            if (Table == null)
            {
                Debug.LogError("[ArticleSystemManager] Failed to load ArticleTable.");
                return;
            }
            TableLoaded?.Invoke(Table);
        });
    }

    // Convenience: compose the full article body from selected options.
    public bool TryCompose(int id, int opt1, int opt2, int opt3,
                           out string title, out string lede, out string body)
    {
        title = lede = body = null;
        if (Table == null) return false;
        if (!Table.TryGetIndex(id, out var idx)) return false;

        title = Table.Titles[idx];
        lede = Table.Ledes[idx];
        body = Table.ComposeBody(id, opt1, opt2, opt3);
        return true;
    }

    // Helpers to fetch slot options / viral index if you want to drive UI easily
    public string[] GetOptions(int id, int slot) =>
        Table == null ? Array.Empty<string>() : Table.GetSlotOptions(id, slot);

    public int GetViralIndex(int id, int slot) =>
        Table == null ? -1 : Table.GetViralIndex(id, slot);
    #endregion


    //----------------------- MINIGAME LOGIC -----------------------//
    private void OnEnable()
    {
        // Evalúa estados actuales
        _articlesReady = (Table != null);
        _dialoguesReady = DialogueSystemManager.IsDialogueTableReady;

        // Suscripciones solo si falta algo
        if (!_articlesReady && !_subscribedToTable)
        {
            TableLoaded += OnArticleTableLoaded;
            _subscribedToTable = true;
        }
        if (!_dialoguesReady && !_subscribedToDialogueReady)
        {
            DialogueSystemManager.DialogueTableReady += OnDialogueTableReady;
            _subscribedToDialogueReady = true;
        }

        // Seguir escuchando cierre de diálogos
        if (!_subscribedToDialogue)
        {
            DialogueSystemManager.DialogueEnded += OnDialogueEnded;
            _subscribedToDialogue = true;
        }

        // Intento inicial por si todo ya estaba listo (artículos + diálogos + tutorial)
        TryStartMinigameIfReady();
    }

    private void OnDisable()
    {
        if (_subscribedToTable)
        {
            TableLoaded -= OnArticleTableLoaded;
            _subscribedToTable = false;
        }
        if (_subscribedToDialogueReady)
        {
            DialogueSystemManager.DialogueTableReady -= OnDialogueTableReady;
            _subscribedToDialogueReady = false;
        }
        if (_subscribedToDialogue)
        {
            DialogueSystemManager.DialogueEnded -= OnDialogueEnded;
            _subscribedToDialogue = false;
        }
    }

    /// <summary>
    /// Punto único para arrancar de forma segura el minijuego.
    /// Solo empieza si:
    /// - artículos listos,
    /// - diálogos listos,
    /// - tutorial terminado,
    /// - y aún no se ha iniciado antes.
    /// </summary>
    public void TryStartMinigameIfReady()
    {
        if (_minigameStarted) return;                  // one-shot
        if (!_articlesReady || !_dialoguesReady) return;
        if (!_tutorialEnded) return;                   // requiere que el tutorial haya terminado

        ResetMinigame();
        StartMinigame();
        _minigameStarted = true;                       // evita dobles arranques
    }

    /// <summary>
    /// Llama esto desde cualquier otro script cuando el tutorial haya terminado.
    /// Ejemplo: ArticleSystemManager.Instance.NotifyTutorialEnded();
    /// </summary>
    public void NotifyTutorialEnded()
    {
        if (_tutorialEnded) return;        // ya se notificó antes

        _tutorialEnded = true;
        TryStartMinigameIfReady();
    }

    #region Events: OnArticleTableLoaded, OnDialogueTableReady & OnDialogueEnded
    private void OnArticleTableLoaded(ArticleTable _)
    {
        _articlesReady = true;
        if (_subscribedToTable)
        {
            TableLoaded -= OnArticleTableLoaded;
            _subscribedToTable = false;
        }
        TryStartMinigameIfReady();
    }

    private void OnDialogueTableReady()
    {
        _dialoguesReady = true;
        if (_subscribedToDialogueReady)
        {
            DialogueSystemManager.DialogueTableReady -= OnDialogueTableReady;
            _subscribedToDialogueReady = false;
        }
        TryStartMinigameIfReady();
    }

    private void OnDialogueEnded()
    {
        // Lógica a ejecutar cuando el diálogo termina
        minigameWrongAnswerPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }
    #endregion

    public void ResetMinigame()
    {
        // Agarrar botones y zonas de texto para reiniciar
        foreach (Button button in buttonList)
        {
            button.onClick.RemoveAllListeners();
            TMP_Text tmp = button.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null)
            {
                tmp.text = string.Empty;
                continue;
            }

            // Si no hay TMP, intenta con el Text de UI legacy
            Text uiText = button.GetComponentInChildren<Text>(true);
            if (uiText != null)
            {
                uiText.text = string.Empty;
            }
        }
        minigameText.text = string.Empty;
        minigameArticlesWritten = 0;
        EventSystem.current.SetSelectedGameObject(null);
        minigameButtonsUI.SetActive(false);
        minigameWrongAnswerPanel.SetActive(false);
    }

    public void StartMinigame()
    {
        // Hacer la selección aleatoria del orden de los artículos (Fisher–Yates)
        int articlesAmmount = Table.Ids.Length;
        if (articlesAmmount == 0)
        {
            Debug.LogError("No hay artículos disponibles para el minijuego.");
            return;
        }
        randomizedArticleIndexes = new int[articlesAmmount];
        for (int i = 0; i < articlesAmmount; i++)
            randomizedArticleIndexes[i] = i;

        for (int i = articlesAmmount - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1); // ints: max es exclusivo, por eso i+1
            int tmp = randomizedArticleIndexes[i];
            randomizedArticleIndexes[i] = randomizedArticleIndexes[j];
            randomizedArticleIndexes[j] = tmp;
        }
        minigameArticlesWritten = 0;
        StartCoroutine(ArticleTyper(0.02f));
        // Comenzar a escribir el artículo
    }

    IEnumerator ArticleTyper(float characterTypingDelay)
    {

        yield return new WaitForSeconds(1);

        // Indices base
        int currentIndex = randomizedArticleIndexes[minigameArticlesWritten];
        string title = Table.Titles[currentIndex];
        string template = Table.Templates[currentIndex];

        // Construcción progresiva
        string constructedArticle = "";
        minigameText.caretPosition = 0;

        // --- Título ---
        for (int i = 0; i < title.Length; i++)
        {
            if (title[i] != ' ')
                yield return new WaitForSeconds(characterTypingDelay);

            constructedArticle += title[i];
            minigameText.text = constructedArticle;
        }
        constructedArticle += "\n\n";
        minigameText.text = constructedArticle;

        // --- Cuerpo con marcadores {n} ---
        for (int i = 0; i < template.Length; i++)
        {
            char c = template[i];

            if (c == '{')
            {
                int close = template.IndexOf('}', i + 1);
                if (close > i + 1)
                {
                    string token = template.Substring(i + 1, close - i - 1);
                    if (int.TryParse(token, out int slot) && slot >= 1)
                    {
                        // Pausar escritura y esperar selección correcta para este slot
                        yield return StartCoroutine(WaitForCorrectSelection(currentIndex, slot));

                        // Escribir el texto elegido en el artículo
                        if (!string.IsNullOrEmpty(_selectedOptionText))
                        {
                            foreach (char ch in _selectedOptionText)
                            {
                                if (ch != ' ')
                                    yield return new WaitForSeconds(characterTypingDelay);

                                constructedArticle += ch;
                                minigameText.text = constructedArticle;
                            }
                        }

                        // Saltar el marcador completo "{n}"
                        i = close;
                        continue;
                    }
                }
            }

            // Escritura normal (sin marcador)
            if (c != ' ')
                yield return new WaitForSeconds(characterTypingDelay);

            constructedArticle += c;
            minigameText.text = constructedArticle;
        }

        // Fin del artículo
        minigameArticlesWritten += 1;
        if (minigameArticlesWritten < ARTICLES_MAXIMUM_TO_WRITE)
        {
            minigameMakeTimePassPanel.SetActive(true);
            yield return new WaitForSeconds(2.5f);
            minigameText.text = "";

            yield return new WaitForSeconds(2f);
            StartCoroutine(ArticleTyper(0.02f));
        }
        else
        {
            yield return new WaitForSeconds(3);
            StopMinigame();
        }
    }

    // Espera hasta que el jugador seleccione la opción correcta del slot {slot}
    IEnumerator WaitForCorrectSelection(int articleId, int slot)
    {
        string[] options = Table.GetSlotOptions(articleId, slot);
        if (options == null || options.Length == 0)
        {
            Debug.LogError($"No hay opciones para el slot {{{slot}}} del artículo {articleId}.");
            yield break;
        }

        int correctIndex = Table.GetViralIndex(articleId, slot); // "correcta" = la más amarillista/viral
        if (correctIndex < 0 || correctIndex >= options.Length)
        {
            Debug.LogWarning($"Índice viral inválido para slot {{{slot}}}. Se tomará 0 por defecto.");
            correctIndex = 0;
        }

        // Preparar botones
        SetupOptionButtonsRandomized(options, correctIndex);

        // Mostrar UI de botones y esperar a la elección correcta
        _selectedOptionText = null;
        _awaitingSelection = true;
        minigameButtonsUI.SetActive(true);
        EventSystem.current?.SetSelectedGameObject(null);

        // Espera activa hasta que se elija la correcta
        yield return new WaitUntil(() => _awaitingSelection == false && !string.IsNullOrEmpty(_selectedOptionText));

        // Ocultar y limpiar
        minigameButtonsUI.SetActive(false);
        foreach (var b in buttonList) b.onClick.RemoveAllListeners();
    }

    // Carga textos y listeners en los botones
    private void SetupOptionButtonsRandomized(string[] options, int correctIndex)
    {
        // Limpia y oculta todo primero
        foreach (var b in buttonList)
        {
            b.onClick.RemoveAllListeners();
            var tmp = b.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null) tmp.text = string.Empty;
            else
            {
                var legacy = b.GetComponentInChildren<Text>(true);
                if (legacy != null) legacy.text = string.Empty;
            }
            b.gameObject.SetActive(false);
        }

        if (options == null || options.Length == 0)
        {
            Debug.LogError("No hay opciones para mostrar.");
            return;
        }
        if (buttonList == null || buttonList.Count == 0)
        {
            Debug.LogError("buttonList está vacío: asigna los botones en el Inspector.");
            return;
        }

        int totalOptions = options.Length;
        int uiCount = Mathf.Min(buttonList.Count, totalOptions);

        // Candidatos incorrectos (excluye la correcta)
        int[] wrong = new int[totalOptions - 1];
        for (int i = 0, w = 0; i < totalOptions; i++)
            if (i != correctIndex) wrong[w++] = i;

        // Barajar candidatos incorrectos
        for (int i = wrong.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (wrong[i], wrong[j]) = (wrong[j], wrong[i]);
        }

        // Selección final: siempre incluye la correcta + (uiCount-1) incorrectas
        int[] selected = new int[uiCount];
        selected[0] = correctIndex;
        for (int i = 1; i < uiCount; i++) selected[i] = wrong[i - 1];

        // Barajar posiciones finales
        for (int i = uiCount - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (selected[i], selected[j]) = (selected[j], selected[i]);
        }

        // Asignar a botones
        _currentOptionIndexByButton = new int[uiCount];
        _currentCorrectButtonSlot = -1;
        _selectedOptionText = options[correctIndex]; // usada por el typer tras acertar

        for (int slot = 0; slot < uiCount; slot++)
        {
            var btn = buttonList[slot];
            int optionIdx = selected[slot];

            _currentOptionIndexByButton[slot] = optionIdx;
            btn.gameObject.SetActive(true);
            SetButtonLabel(btn, options[optionIdx]);

            if (optionIdx == correctIndex)
            {
                _currentCorrectButtonSlot = slot;

                // 1) Primero el "tick" interno (reanuda la corrutina)
                btn.onClick.AddListener(() =>
                {
                    _selectedOptionText = options[optionIdx];
                    _awaitingSelection = false;
                });

                // 2) Después el listener que pediste explícitamente
                btn.onClick.AddListener(CorrectAnswerButton);
            }
            else
            {
                btn.onClick.AddListener(WrongAnswerButton);
            }
        }
    }

    // Pone el texto del botón (TMP o Text clásico)
    private void SetButtonLabel(Button button, string text)
    {
        var tmp = button.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null) { tmp.text = text; return; }

        var legacy = button.GetComponentInChildren<Text>(true);
        if (legacy != null) legacy.text = text;
    }

    public void CorrectAnswerButton()
    {
        foreach (Button button in buttonList)
        {
            button.onClick.RemoveAllListeners();
        }
    }

    public void WrongAnswerButton()
    {
        if (DialogueSystemManager.sharedInstanceDialogueManager.dialogueTable != null)
        {
            DialogueSystemManager.sharedInstanceDialogueManager.LoadDialogue(DIALOGUE_EVAN_MISTAKE_SELECTION);
            minigameWrongAnswerPanel.SetActive(true);
        }

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void StopMinigame()
    {
        ResetMinigame();
        computerCameraZone.SetActive(false);
        afterMinigameHitboxDialogue.SetActive(true);
        afterMinigameHitboxLoadScene.SetActive(true);
        gameObject.GetComponent<MakeEvanMove>().MoveEvan();

        PlayerMovement.sharedInstancePlayerMovement.allowMovement = true;
        DialogueSystemManager.sharedInstanceDialogueManager.LoadDialogue(DIALOGUE_EVAN_WANTS_TO_GO_HOME_SEQUENCE);
        computerPanel.SetActive(false);
    }
}
