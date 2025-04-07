using UnityEngine;

public class HumiditySensor : MonoBehaviour
{
    public Animator animator;
    private bool isActive = false;
    private bool playerInTrigger = false;

    void Update()
    {
        // Simulaci�n: Si el jugador est� en el trigger y presiona "Q", activa la animaci�n
        if (Input.GetKeyDown(KeyCode.Q) && playerInTrigger && !isActive)
        {
            isActive = true;
            animator.Play("fuego");
        }
    }

    // M�todo para cambiar el estado del trigger (llamado desde PlayerTriggerDetector)
    public void SetPlayerInTrigger(bool state)
    {
        playerInTrigger = state;
    }

    // Se llama desde el evento de la animaci�n "fuego"
    public void OnFuegoFinished()
    {
        if (isActive)
        {
            animator.Play("fuegostill");
        }
    }
}
