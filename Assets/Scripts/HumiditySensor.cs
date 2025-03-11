using UnityEngine;

public class HumiditySensor : MonoBehaviour
{
    public Animator animator;
    private bool isActive = false;
    private bool playerInTrigger = false;

    void Update()
    {
        // Simulación: Si el jugador está en el trigger y presiona "Q", activa la animación
        if (Input.GetKeyDown(KeyCode.Q) && playerInTrigger && !isActive)
        {
            isActive = true;
            animator.Play("fuego");
        }
    }

    // Método para cambiar el estado del trigger (llamado desde PlayerTriggerDetector)
    public void SetPlayerInTrigger(bool state)
    {
        playerInTrigger = state;
    }

    // Se llama desde el evento de la animación "fuego"
    public void OnFuegoFinished()
    {
        if (isActive)
        {
            animator.Play("fuegostill");
        }
    }
}
