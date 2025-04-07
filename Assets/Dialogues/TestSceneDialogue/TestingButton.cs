using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingButton : MonoBehaviour
{
    public Dialogue startingDialogue;
    public DialogueManager dialogueManager;
    public GameObject playerMovement;
    public bool talk;
    private bool inDialogue;

    public void Start()
    {
        playerMovement = GameObject.FindGameObjectWithTag("Player");
        dialogueManager = GameObject.Find("DialogueManager").GetComponent<DialogueManager>(); // Gets reference of Dialogue Manager.
    }

    private void Update()
    {
        if (talk)
        {
            if (Input.GetKey(KeyCode.E))
            {
                StartTestingDialogue();
                StopPlayerMovement();
                inDialogue = true;
            }
            if (inDialogue)
            {
                Debug.Log("Indialogue");
                if (DialogueManager
                    .sharedInstanceDialogueManager
                    .CurrentDialogueObject.nextDialogue == null)
                {
                    FreePlayerMovement();
                    inDialogue = false;
                    //talk = false;
                }
            }
        }
    }

    public void StartTestingDialogue()
    {

        dialogueManager.LoadDialogue(startingDialogue); // Loads starting dialogue.
        //gameObject.SetActive(false); // Turns off Button.
    }

    public void StopPlayerMovement()
    {
        /*
        playerMovement.GetComponent<ToMoveAWSD>().move = 
            false;
        */
    }
    public void FreePlayerMovement()
    {
        /*
        Debug.Log("moveNow");
        playerMovement.GetComponent<ToMoveAWSD>().move =
            true;
        */
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") )
        {
            playerMovement = other.gameObject;
            talk = true;
        }
        else
        {
            talk = false;
        }
    }

}
