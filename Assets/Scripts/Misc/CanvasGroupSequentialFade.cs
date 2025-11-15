using System.Collections;
using UnityEngine;

public class CanvasGroupSequentialFade : MonoBehaviour
{
    [Header("Referencia")]
    [SerializeField] private GameObject canvasGroupGO;
    private CanvasGroup canvasGroup;

    [Header("Control de secuencia")]
    [Tooltip("Si está en true, se ejecutará el fade in cuando se lance la secuencia.")]
    public bool enableFadeIn = true;

    [Tooltip("Si está en true, se ejecutará el fade out después del fade in (si también está activo).")]
    public bool enableFadeOut = false;

    [Tooltip("Si está en true, la secuencia se ejecuta automáticamente en Start().")]
    public bool runOnStart = false;

    [Header("Parámetros Fade In")]
    public float fadeInStartAlpha = 0f;
    public float fadeInEndAlpha = 1f;
    public float fadeInDuration = 1f;

    [Header("Parámetros Fade Out")]
    public float fadeOutStartAlpha = 1f;
    public float fadeOutEndAlpha = 0f;
    public float fadeOutDuration = 1f;

    [Header("Delay entre In y Out")]
    [Tooltip("Tiempo de espera (en segundos) antes de comenzar el fade out, después de terminar el fade in.")]
    public float delayBeforeFadeOut = 0f;

    [Header("Opciones")]
    [Tooltip("Usar Time.unscaledDeltaTime (por ejemplo para menús de pausa).")]
    public bool useUnscaledTime = false;

    private bool isRunning = false;

    private void Awake()
    {
        if (canvasGroup == null && canvasGroupGO != null)
        {
            canvasGroup = canvasGroupGO.GetComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        if (runOnStart)
        {
            StartFadeSequence();
        }
    }

    /// <summary>
    /// Método público para lanzar la secuencia desde otros scripts.
    /// Respeta los bools enableFadeIn / enableFadeOut y el orden.
    /// </summary>
    public void StartFadeSequence()
    {
        if (!isRunning)
        {
            if (canvasGroupGO != null)
                canvasGroupGO.SetActive(true);

            StartCoroutine(FadeSequenceCoroutine());
        }
    }

    private IEnumerator FadeSequenceCoroutine()
    {
        if (canvasGroup == null)
        {
            Debug.LogError("[CanvasGroupSequentialFade] No hay CanvasGroup asignado.");
            yield break;
        }

        isRunning = true;

        // 👉 1) F A D E   I N
        if (enableFadeIn)
        {
            yield return StartCoroutine(FadeCanvas(
                fadeInStartAlpha,
                fadeInEndAlpha,
                fadeInDuration
            ));
        }

        // 👉 Espera opcional antes del fade out
        if (enableFadeOut && delayBeforeFadeOut > 0f)
        {
            float elapsed = 0f;
            while (elapsed < delayBeforeFadeOut)
            {
                float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += delta;
                yield return null;
            }
        }

        // 👉 2) F A D E   O U T (solo si está habilitado)
        if (enableFadeOut)
        {
            yield return StartCoroutine(FadeCanvas(
                fadeOutStartAlpha,
                fadeOutEndAlpha,
                fadeOutDuration
            ));
        }

        isRunning = false;
    }

    /// <summary>
    /// Corrutina genérica de fade.
    /// </summary>
    private IEnumerator FadeCanvas(float fromAlpha, float toAlpha, float duration)
    {
        if (duration <= 0f)
        {
            canvasGroup.alpha = toAlpha;
            yield break;
        }

        float time = 0f;
        canvasGroup.alpha = fromAlpha;

        while (time < duration)
        {
            float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            time += delta;

            float t = Mathf.Clamp01(time / duration);
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);

            yield return null;
        }

        canvasGroup.alpha = toAlpha;
    }
}
