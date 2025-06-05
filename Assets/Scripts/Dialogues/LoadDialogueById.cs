using UnityEngine;

public class LoadDialogueByID : MonoBehaviour
{
    private bool dialogueStarted = false;
    [SerializeField] private int dialogueID;

    void Update()
    {
        if (!dialogueStarted && DialogueSystemManager.sharedInstanceDialogueManager.dialogueTable != null)
        {
            DialogueSystemManager.sharedInstanceDialogueManager.LoadDialogue(dialogueID); //código de muestra sobre cómo cargar una secuencia de diálogos
            dialogueStarted = true;
        }
    }
}
