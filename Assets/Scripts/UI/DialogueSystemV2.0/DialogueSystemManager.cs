using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class DialogueSystemManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI dialogueSystemText, characterNameText;

    [SerializeField]
    private GameObject triangleImage, continueDialogueButton, DialogueCanva;

    [SerializeField]
    private bool continueDialogueBool = false;

    Dialogue currentDialogueObject;
    DialogueHistorySystemManager dialogueHistoryManager;

    public static DialogueSystemManager sharedInstanceDialogueManager;

    public DialogueTable dialogueTable;
    public int globalDialogueID;
    public string globalSeparator = string.Empty;

    // --- Eventos públicos ---
    public static event Action DialogueEnded;            // Se dispara al terminar una secuencia de diálogo
    public static event Action DialogueTableReady;       // Se dispara cuando la tabla de diálogos está lista

    private static bool _isDialogueTableReady = false;
    public static bool IsDialogueTableReady => _isDialogueTableReady;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (sharedInstanceDialogueManager == null)
        {
            sharedInstanceDialogueManager = this;
        }
    }

    void Start()
    {
        var service = new DialogueCSVDataService();

        // Carga asíncrona de la tabla de diálogos
        service.LoadDataAsync<DialogueTable>(
            "Assets/CSV Files/DialoguesTest1.csv",
            false,
            (varTable) =>
            {
                if (varTable == null)
                {
                    Debug.LogError("Could not load dialogue.");
                    return;
                }

                dialogueTable = varTable;

                // Marca la tabla como lista y lanza el evento una sola vez
                if (!_isDialogueTableReady)
                {
                    _isDialogueTableReady = true;
                    DialogueTableReady?.Invoke();
                }
            });

        // Intenta obtener el gestor de historial de diálogos
        try
        {
            dialogueHistoryManager = GameObject
                .Find("DialogueHistoryManagerV2")
                .GetComponent<DialogueHistorySystemManager>();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Nothing to worry about, dialogue history manager just not present: " + ex.Message);
        }
    }

    public void LoadDialogue(int id)
    {
        PlayerMovement.sharedInstancePlayerMovement.allowMovement = false;

        // Mostrar el canvas de diálogo
        DialogueCanva.SetActive(true);

        setActiveTriangle(false);
        globalDialogueID = id;

        // Si aún no se ha asignado separador global, usar el del primer diálogo
        if (string.IsNullOrEmpty(globalSeparator))
        {
            globalSeparator = dialogueTable.Separators[id];
        }

        // Inicia la corrutina del tipeo
        StartCoroutine(DialogueTyper(
            dialogueTable.Separators[id],
            dialogueTable.CharacterNames[id],
            dialogueTable.Dialogues[id],
            dialogueTable.ColorNames[id],
            dialogueTable.ColorDialogues[id],
            dialogueTable.SpeedDialogues[id],
            dialogueTable.ImageNames[id]
        ));
    }

    IEnumerator DialogueTyper(
        string separator,
        string characterName,
        string dialogueText,
        string colorName,
        string colorDialogue,
        float speedDialogue,
        string imageName)
    {
        // Configurar nombre de personaje y colores
        Color colorParsed;

        ColorUtility.TryParseHtmlString(colorName, out colorParsed);
        characterNameText.color = colorParsed;
        characterNameText.SetText(characterName);

        ColorUtility.TryParseHtmlString(colorDialogue, out colorParsed);
        dialogueSystemText.color = colorParsed;

        // Construir el texto progresivamente
        string constructedDialogue = "";

        for (int i = 0; i < dialogueText.Length; i++)
        {
            if (dialogueText[i] != ' ') // Solo esperar si no es espacio
            {
                yield return new WaitForSeconds(speedDialogue);
            }

            constructedDialogue += dialogueText[i];
            dialogueSystemText.SetText(constructedDialogue);
        }

        // Al terminar de escribir
        continueDialogueBool = true;
        setActiveTriangle(true);

        // Registrar en historial
        try
        {
            dialogueHistoryManager.RegisterDialogue(
                dialogueTable.Separators[globalDialogueID],
                dialogueTable.CharacterNames[globalDialogueID],
                dialogueTable.Dialogues[globalDialogueID],
                dialogueTable.ColorNames[globalDialogueID],
                dialogueTable.ColorDialogues[globalDialogueID],
                dialogueTable.SpeedDialogues[globalDialogueID],
                dialogueTable.ImageNames[globalDialogueID]
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine("Nothing to worry about, dialogue history manager just not present: " + ex.Message);
        }
    }

    // Botón que cubre todo el sistema de diálogo
    public void dialogueButton()
    {
        continueSkipDialogue();
    }

    void continueSkipDialogue()
    {
        // Si el texto ya terminó de escribirse, pasamos al siguiente diálogo
        if (continueDialogueBool)
        {
            setActiveTriangle(false);
            dialogueSystemText.SetText("");
            continueDialogueBool = false;

            // Pasar al siguiente ID
            globalDialogueID += 1;

            bool hasDialoguesLeft = (dialogueTable.Ids.Length > globalDialogueID);

            if (hasDialoguesLeft)
            {
                // Mientras el separador sea el mismo que el global, seguimos la cadena
                if (dialogueTable.Separators[globalDialogueID] == globalSeparator)
                {
                    LoadDialogue(globalDialogueID);
                }
                else
                {
                    // Separador distinto → terminar secuencia
                    endDialogueSequence();
                }
            }
            else
            {
                // No quedan más diálogos
                endDialogueSequence();
            }
        }
        else
        {
            // Si aún se está escribiendo, saltamos al final del diálogo actual
            try
            {
                dialogueHistoryManager.RegisterDialogue(
                    dialogueTable.Separators[globalDialogueID],
                    dialogueTable.CharacterNames[globalDialogueID],
                    dialogueTable.Dialogues[globalDialogueID],
                    dialogueTable.ColorNames[globalDialogueID],
                    dialogueTable.ColorDialogues[globalDialogueID],
                    dialogueTable.SpeedDialogues[globalDialogueID],
                    dialogueTable.ImageNames[globalDialogueID]
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("Nothing to worry about, dialogue history manager just not present: " + ex.Message);
            }

            StopAllCoroutines();
            dialogueSystemText.SetText(dialogueTable.Dialogues[globalDialogueID]);
            setActiveTriangle(true);
            continueDialogueBool = true;
        }
    }

    void setActiveTriangle(bool active)
    {
        triangleImage.SetActive(active);
    }

    void endDialogueSequence()
    {
        DialogueCanva.SetActive(false);
        globalSeparator = string.Empty;

        PlayerMovement.sharedInstancePlayerMovement.allowMovement = true;
        StopAllCoroutines();

        DialogueEnded?.Invoke();
    }
}
