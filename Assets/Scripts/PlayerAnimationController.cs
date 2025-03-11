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
        if (vertical > 0.1f) animator.Play("adelante");
        else if (vertical < -0.1f) animator.Play("Idle");
        else if (horizontal > 0.1f) animator.Play("Izquierda");
        else if (horizontal < -0.1f) animator.Play("Derecha");
        else animator.Play("parao");
    }

}