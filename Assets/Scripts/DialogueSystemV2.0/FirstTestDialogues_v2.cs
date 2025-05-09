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
            dialogueManager.LoadDialogue(0);
            dialogueStarted = true;
        }
    }
}
