using UnityEngine;

public class LoadDialogueByID : MonoBehaviour
{
    private bool dialogueStarted = false;
    [SerializeField] private int dialogueID;

    void Update()
    {
        if (!dialogueStarted && DialogueSystemManager.sharedInstanceDialogueManager.dialogueTable != null)
        {
            DialogueSystemManager.sharedInstanceDialogueManager.LoadDialogue(dialogueID); //c�digo de muestra sobre c�mo cargar una secuencia de di�logos
            dialogueStarted = true;
        }
    }
}
