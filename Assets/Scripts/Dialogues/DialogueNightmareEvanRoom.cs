using UnityEngine;

public class DialogueNightmareEvanRoom : MonoBehaviour
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
            dialogueManager.LoadDialogue(12); //código de muestra sobre cómo cargar una secuencia de diálogos
            dialogueStarted = true;
        }
    }
}
