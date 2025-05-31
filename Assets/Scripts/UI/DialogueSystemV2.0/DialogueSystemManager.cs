using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueSystemManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI dialogueSystemText, characterNameText;
    [SerializeField]
    private GameObject triangleImage, continueDialogueButton, DialogueCanva;
    [SerializeField]
    [TextArea(5, 15)]
    private bool continueDialogueBool = false;
    Dialogue currentDialogueObject;
    DialogueHistorySystemManager dialogueHistoryManager;

    public static DialogueSystemManager sharedInstanceDialogueManager;

    //public Dialogue CurrentDialogueObject { get => currentDialogueObject; set => currentDialogueObject = value; }
    public DialogueTable dialogueTable;
    public int globalDialogueID;
    public string globalSeparator = string.Empty;
    public bool globalSeparatorFlag = false;
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
        service.LoadDataAsync<DialogueTable>("Assets/Dialogue/DialoguesTest1.csv", false, (varTable) => //LOAD dialogue table
        {
            if (varTable == null)
            {
                Debug.LogError("Could not load dialogue.");
                return;
            }
            dialogueTable = varTable;
        });
        
        dialogueHistoryManager = GameObject.Find("DialogueHistoryManagerV2").GetComponent<DialogueHistorySystemManager>(); // Gets reference of Dialogue History Manager. (duh)
    }

    public void LoadDialogue(int id)
    {

        PlayerMovement.sharedInstancePlayerMovement.allowMovement = false;
        Debug.Log(dialogueTable.Dialogues[0]);
        Debug.Log(dialogueTable.Separators.Length);
        Debug.Log(dialogueTable.CharacterNames.Length);
        Debug.Log(dialogueTable.Dialogues.Length);
        Debug.Log(dialogueTable.ColorNames.Length);
        Debug.Log(dialogueTable.ColorDialogues.Length);
        Debug.Log(dialogueTable.SpeedDialogues.Length);
        Debug.Log(dialogueTable.ImageNames.Length);
        Debug.Log(dialogueTable.Ids.Length);
        Debug.Log(dialogueTable.Ids[id]+dialogueTable.Separators[id]+
                                     dialogueTable.CharacterNames[id]+ dialogueTable.Dialogues[id]+
                                     dialogueTable.ColorNames[id]+ dialogueTable.ColorDialogues[id]+
                                     dialogueTable.SpeedDialogues[id]+ dialogueTable.ImageNames[id]);
        // Display the dialogue system (if it was previously off).
        DialogueCanva.SetActive(true);
        
        setActiveTriangle(false);
        globalDialogueID = id;
        globalSeparator = (globalSeparator == string.Empty) ? dialogueTable.Separators[id] : globalSeparator; //Check if the separator has been asigned, if not, assign it.

        // Calls the typer coroutine.
        StartCoroutine(DialogueTyper(dialogueTable.Separators[id], 
                                     dialogueTable.CharacterNames[id], dialogueTable.Dialogues[id], 
                                     dialogueTable.ColorNames[id], dialogueTable.ColorDialogues[id], 
                                     dialogueTable.SpeedDialogues[id], dialogueTable.ImageNames[id]));
}

    IEnumerator DialogueTyper(string separator, string characterName, string dialogueText, string colorName, string colorDialogue, float speedDialogue, string imageName)
    {
        // Loads name and properties of text to the dialogue system.
        UnityEngine.Color colorParsed;
        UnityEngine.ColorUtility.TryParseHtmlString(colorName, out colorParsed);
        characterNameText.color = colorParsed;
        characterNameText.SetText(characterName);

        UnityEngine.ColorUtility.TryParseHtmlString(colorDialogue, out colorParsed);
        dialogueSystemText.color = colorParsed;

        // Create the string we are going to build progressively
        string constructedDialogue = "";

        // Iterate for every character of the Dialogue.
        for (int i = 0; i < dialogueText.Length; i++) 
        {
            if (dialogueText[i] != char.Parse(" ")) // Only wait time if theres a symbol different from whitespaces.
            {
                yield return new WaitForSeconds(speedDialogue);
            }
            // Progressively replaces the text inside the dialogue system everytime.
            constructedDialogue += dialogueText[i];
            dialogueSystemText.SetText(constructedDialogue);
        }
        // Turns on the continue dialogue flag
        continueDialogueBool = true;
        setActiveTriangle(true);
        // Registers dialogue in the Dialogue History
        dialogueHistoryManager.RegisterDialogue(dialogueTable.Separators[globalDialogueID],
                                     dialogueTable.CharacterNames[globalDialogueID], dialogueTable.Dialogues[globalDialogueID],
                                     dialogueTable.ColorNames[globalDialogueID], dialogueTable.ColorDialogues[globalDialogueID],
                                     dialogueTable.SpeedDialogues[globalDialogueID], dialogueTable.ImageNames[globalDialogueID]);
    }

    public void dialogueButton() //Button that covers the whole dialogue system and continues or skips the dialogue
    {
        continueSkipDialogue();
    }

    void continueSkipDialogue()
    {
        // checks if its posible to continue with next dialogue, otherwise it skips the coroutine and fills out the text.

        if (continueDialogueBool)
        {
            setActiveTriangle(false);
            dialogueSystemText.SetText("");
            continueDialogueBool = false;
            // Load next dialogue
            globalDialogueID += 1;
            // Loads a dialogue if: there are any dialogues left, it has not met any different separators, and has not met the same separator twice

            bool hasDialoguesLeft = (dialogueTable.Ids.Length > globalDialogueID);
            //Debug.Log("hasDialoguesLeft: " + hasDialoguesLeft);
            if (hasDialoguesLeft)
            {
                bool hasMetDifferentSeparator = (globalSeparator != dialogueTable.Separators[globalDialogueID] && !string.IsNullOrEmpty(dialogueTable.Separators[globalDialogueID]));

                //Debug.Log("hasMetDifferentSeparator: " + hasMetDifferentSeparator);
                
                //                                                          AND    the global separator is not an empty string
                if (!hasMetDifferentSeparator && globalSeparatorFlag == false && !string.IsNullOrEmpty(globalSeparator))
                {
                    globalSeparatorFlag = globalSeparator == dialogueTable.Separators[globalDialogueID];
                    LoadDialogue(globalDialogueID);
                }
                else
                { endDialogueSequence(); }
            }
            else { endDialogueSequence(); }
        }
        else
        {
            //Debug.Log("SKIPPED DIALOGUE");
            // Registers dialogue in the Dialogue History
            dialogueHistoryManager.RegisterDialogue(dialogueTable.Separators[globalDialogueID],
                                     dialogueTable.CharacterNames[globalDialogueID], dialogueTable.Dialogues[globalDialogueID],
                                     dialogueTable.ColorNames[globalDialogueID], dialogueTable.ColorDialogues[globalDialogueID],
                                     dialogueTable.SpeedDialogues[globalDialogueID], dialogueTable.ImageNames[globalDialogueID]);
            StopAllCoroutines();
            dialogueSystemText.SetText(dialogueTable.Dialogues[globalDialogueID]);
            setActiveTriangle(true);
            continueDialogueBool = true;
        }
    }
    void setActiveTriangle(bool Active) // Turns on/off the triangle animation
    {
        triangleImage.SetActive(Active);
    }
    void endDialogueSequence()
    {

        DialogueCanva.SetActive(false);
        globalSeparator = string.Empty;
        globalSeparatorFlag = false;

        PlayerMovement.sharedInstancePlayerMovement.allowMovement = true;
        StopAllCoroutines();
    }
}
