using UnityEngine;

public class FirstTestDialogues_v2 : MonoBehaviour
{
    public DialogueSystemManager dialogueManager;
    private bool dialogueStarted = false;

    void Start()
    {
        dialogueManager = GameObject.Find("DialogueManagerV2").GetComponent<DialogueSystemManager>();
    }

    void Update()
    {
        if (!dialogueStarted && dialogueManager.dialogueTable != null)
        {
            dialogueManager.LoadDialogue(0); //c�digo de muestra sobre c�mo cargar una secuencia de di�logos
            dialogueStarted = true;
        }
    }
}
