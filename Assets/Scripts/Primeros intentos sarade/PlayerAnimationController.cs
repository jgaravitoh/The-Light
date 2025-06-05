using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);

        // Forzar una animación manualmente
        if (vertical > 0.1f) animator.Play("EvanAtras");
        else if (vertical < -0.1f) animator.Play("EvanAdelante");
        else if (horizontal > 0.1f) animator.Play("EvanIzquierda");
        else if (horizontal < -0.1f) animator.Play("EvanDerecha");
        else animator.Play("EvanIdle");
    }

}