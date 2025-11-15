using UnityEngine;
using System.Collections.Generic;
using System;

public class LoadDialogueById_GENERIC : MonoBehaviour
{
    [SerializeField] private List<int> dialogueIds = new();
    [SerializeField] private int dialogueId;
    private int[] dialogueIdsArray;
    private bool isDialogueTableReady;
    private bool isSubscribedToDialogueReady;
    private Mode currentOperation;
    public enum Mode
    {
        None,
        LoadDialogueWithVariable,
        LoadDialogueWithArrayVariable,
    }


    public void LoadDialogue()
    {
        isDialogueTableReady = DialogueSystemManager.IsDialogueTableReady;
        currentOperation = Mode.LoadDialogueWithVariable;
        if (isDialogueTableReady)
        {
            ExecuteCurrentOperation();
        }
        else if (!isSubscribedToDialogueReady)
        {
            DialogueSystemManager.DialogueTableReady += OnDialogueTableReady;
            isSubscribedToDialogueReady = true;
            Debug.LogError("Dialogue Manager is not instantiated or fully loaded");
        }
    }
    public void LoadRandomDialogue()
    {
        isDialogueTableReady = DialogueSystemManager.IsDialogueTableReady;
        currentOperation = Mode.LoadDialogueWithArrayVariable;
        dialogueIdsArray = dialogueIds.ToArray();
        if (isDialogueTableReady)
        {
            ExecuteCurrentOperation();
        }
        if (!isSubscribedToDialogueReady)
        {
            DialogueSystemManager.DialogueTableReady += OnDialogueTableReady;
            isSubscribedToDialogueReady = true;
            Debug.LogWarning("Dialogue Manager is not instantiated or fully loaded");
        }
    }
 
    private void OnDialogueTableReady()
    {
        isDialogueTableReady = true;

        if (isSubscribedToDialogueReady)
        {
            DialogueSystemManager.DialogueTableReady -= OnDialogueTableReady;
            isSubscribedToDialogueReady = false;
        }

        ExecuteCurrentOperation();
    }

    private void ExecuteCurrentOperation()
    {
        switch (currentOperation)
        {
            case Mode.LoadDialogueWithVariable:
                Debug.Log("Loading dialogue");
                currentOperation = Mode.None;
                DialogueSystemManager.sharedInstanceDialogueManager.LoadDialogue(dialogueId);
                break;
            
            case Mode.LoadDialogueWithArrayVariable:
                Debug.Log("Loading random dialogue");
                DialogueSystemManager.sharedInstanceDialogueManager.LoadDialogue(dialogueIdsArray[UnityEngine.Random.Range(0,dialogueIdsArray.Length)]);
                currentOperation = Mode.None;
                break;


            default:
                Debug.LogWarning("Unknown dialogue load mode");
                currentOperation = Mode.None;
                break;
        }
    }

    private void OnDisable()
    {
        // Por si el objeto se destruye mientras estás suscrito
        if (isSubscribedToDialogueReady)
        {
            DialogueSystemManager.DialogueTableReady -= OnDialogueTableReady;
            isSubscribedToDialogueReady = false;
        }
    }
}
