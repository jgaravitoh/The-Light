using UnityEngine;
using UnityEngine.Events;

public class DoOnTrigger : MonoBehaviour
{
    public UnityEvent onEnter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") onEnter?.Invoke();
    }

}
