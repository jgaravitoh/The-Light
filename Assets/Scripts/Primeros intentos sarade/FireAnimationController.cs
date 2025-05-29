using System.Collections;
using UnityEngine;

public class FireAnimationController : MonoBehaviour
{
    public void OnFuegoAnimationEnd()
    {
        // Inicia el fade a blanco cuando termina la animación de fuego
        Object.FindFirstObjectByType<ScreenFade>().StartFadeToWhite();
    }
}
