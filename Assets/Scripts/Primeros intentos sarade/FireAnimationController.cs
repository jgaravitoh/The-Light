using System.Collections;
using UnityEngine;

public class FireAnimationController : MonoBehaviour
{
    public void OnFuegoAnimationEnd()
    {
        // Inicia el fade a blanco cuando termina la animaci�n de fuego
        Object.FindFirstObjectByType<ScreenFade>().StartFadeToWhite();
    }
}
