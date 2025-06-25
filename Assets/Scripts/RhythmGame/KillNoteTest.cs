using UnityEngine;

public class KillNoteTest : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Note")
        {
            other.gameObject.SetActive(false);

        }
    }
}
