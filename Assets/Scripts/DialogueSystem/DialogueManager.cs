using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
public class DialogueManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI dialogueSystemText, characterNameText;
    [SerializeField]
    private GameObject triangleImage, continueDialogueButton, DialogueCanva;
    [SerializeField]
    [TextArea(5, 15)]
    private bool continueDialogueBool = false;
    Dialogue currentDialogueObject;
    DialogueHistoryManager dialogueHistoryManager;

    public static DialogueManager sharedInstanceDialogueManager;

    public Dialogue CurrentDialogueObject { get => currentDialogueObject; set => currentDialogueObject = value; }

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
        dialogueHistoryManager = GameObject.Find("DialogueHistoryManager").GetComponent<DialogueHistoryManager>(); // Gets reference of Dialogue History Manager. (duh)
    }

    public void LoadDialogue(Dialogue DialogueObject)
    {
        // Display the dialogue system (if it was previously off).
        DialogueCanva.SetActive(true);
        // Assigns the scriptable object to a local variable and turns off "triangle continue animation".
        CurrentDialogueObject = DialogueObject;
        setActiveTriangle(false);
        // Calls the typer coroutine.
        StartCoroutine(DialogueTyper(DialogueObject));
    }

    IEnumerator DialogueTyper(Dialogue DialogueObject)
    {
        // Loads name and properties of text to the dialogue system.
        characterNameText.color = CurrentDialogueObject.nameColor;
        characterNameText.SetText(CurrentDialogueObject.CharacterName);
        dialogueSystemText.color = CurrentDialogueObject.textColor;

        // Create the string we are going to build progressively
        string constructedDialogue = "";

        // Iterate for every character of the Dialogue.
        for (int i = 0; i < CurrentDialogueObject.DialogueText.Length; i++) 
        {
            if (CurrentDialogueObject.DialogueText[i] != char.Parse(" ")) // Only wait time if theres a symbol different from whitespaces.
            {
                yield return new WaitForSeconds(CurrentDialogueObject.waitTime);
            }
            // Progressively replaces the text inside the dialogue system everytime.
            constructedDialogue += CurrentDialogueObject.DialogueText[i];
            dialogueSystemText.SetText(constructedDialogue);
        }
        // Turns on the continue dialogue flag
        continueDialogueBool = true;
        setActiveTriangle(true);
        // Registers dialogue in the Dialogue History
        dialogueHistoryManager.RegisterDialogue(CurrentDialogueObject);
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
            if(CurrentDialogueObject.nextDialogue != null)
            {
                LoadDialogue(CurrentDialogueObject.nextDialogue);
            }
            else
            {
                DialogueCanva.SetActive(false);
            }
        }
        else
        {
            // Registers dialogue in the Dialogue History
            dialogueHistoryManager.RegisterDialogue(CurrentDialogueObject);
            StopAllCoroutines();
            dialogueSystemText.SetText(CurrentDialogueObject.DialogueText);
            setActiveTriangle(true);
            continueDialogueBool = true;
        }
    }
    void setActiveTriangle(bool Active) // Turns on/off the triangle animation
    {
        triangleImage.SetActive(Active);
    }
}
