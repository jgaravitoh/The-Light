using UnityEngine;

public class TextAnimationTrigger : MonoBehaviour
{
    public Animator textAnimator;
    private bool isPlayerInside = false;
    private bool hasPlayedAppear = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasPlayedAppear)
        {
            isPlayerInside = true;
            textAnimator.Play("Aparicion"); // Se activa solo la primera vez
            hasPlayedAppear = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            textAnimator.Play("Desaparicion");
            Invoke("SetNoPantalla", 1.0f); // Espera 1s antes de poner "NoPantalla"
        }
    }

    void SetNoPantalla()
    {
        if (!isPlayerInside)
        {
            textAnimator.Play("NoPantalla");
            hasPlayedAppear = false; // Permite que "Aparicion" se ejecute de nuevo la próxima vez
        }
    }

    // 🔥 Este método es llamado por el evento en la animación "Aparicion"
    public void OnAparicionFinished()
    {
        if (isPlayerInside)
        {
            textAnimator.Play("Pantalla"); // Cambia a "Pantalla" si el jugador sigue dentro
        }
    }
}
