using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFade : MonoBehaviour
{
    public Image fadeImage;
    public CanvasGroup textCanvasGroup; // ?? Para controlar la opacidad del texto
    public float fadeDuration = 2f;
    public float fadeDelay = 1f;
    public float textFadeDelay = 1f; // ? Retraso antes de mostrar el texto
    public float textFadeDuration = 2f;

    private bool hasFaded = false; // ?? Evita que el fade se repita

    public void StartFadeToWhite()
    {
        if (!hasFaded)
        {
            StartCoroutine(FadeToWhite());
            hasFaded = true; // ?? Evita que la animación se repita
        }
    }

    private IEnumerator FadeToWhite()
    {
        // ? Esperamos antes de iniciar el fade
        yield return new WaitForSeconds(fadeDelay);

        Color color = fadeImage.color;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, timer / fadeDuration);
            color.a = t;
            fadeImage.color = color;
            yield return null;
        }

        // ?? Aseguramos que la pantalla quede completamente blanca
        color.a = 1f;
        fadeImage.color = color;
        fadeImage.raycastTarget = true; // Bloquea interacciones detrás del fade

        // ?? Iniciamos el fade del texto después de un delay
        StartCoroutine(FadeInText());
    }

    private IEnumerator FadeInText()
    {
        // ? Esperamos antes de mostrar el texto
        yield return new WaitForSeconds(textFadeDelay);

        float timer = 0f;

        while (timer < textFadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, timer / textFadeDuration);
            textCanvasGroup.alpha = t; // ?? Ajusta la opacidad del texto
            yield return null;
        }

        textCanvasGroup.alpha = 1f; // ?? Aseguramos que el texto sea completamente visible
    }
}
