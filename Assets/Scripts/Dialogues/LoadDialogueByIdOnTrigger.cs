using System;
using UnityEngine;

public class LoadDialogueByIDOnTrigger : MonoBehaviour
{
    [SerializeField] private string _targetTag;
    [SerializeField] private int dialogueID;
    [SerializeField] private bool randomDialogueBool;
    [SerializeField] private int lowerBound, upperBound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(_targetTag))
        {
            int selectedDialogueID = dialogueID;

            if (randomDialogueBool)
            {
                System.Random rnd = new System.Random(); // No necesitas DateTime para sembrar.
                selectedDialogueID = rnd.Next(lowerBound, upperBound + 1); // +1 para que sea inclusivo
            }

            DialogueSystemManager.sharedInstanceDialogueManager.LoadDialogue(selectedDialogueID);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(_targetTag))
        {
            int selectedDialogueID = dialogueID;

            if (randomDialogueBool)
            {
                System.Random rnd = new System.Random();
                selectedDialogueID = rnd.Next(lowerBound, upperBound + 1);
            }

            DialogueSystemManager.sharedInstanceDialogueManager.LoadDialogue(selectedDialogueID);
        }
    }
}
