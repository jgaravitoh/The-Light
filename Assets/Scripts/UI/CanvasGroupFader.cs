using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupFader : MonoBehaviour
{
    [Header("Default Fade Settings")]
    public float fadeDuration = 0.5f;
    public float fadeDelay = 0f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CanvasGroup canvasGroup;
    private Coroutine currentFadeRoutine;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Fade in to alpha = 1 using default settings.
    /// </summary>
    public void FadeIn() => FadeTo(1f, fadeDuration, fadeDelay, fadeCurve);

    /// <summary>
    /// Fade out to alpha = 0 using default settings.
    /// </summary>
    public void FadeOut() => FadeTo(0f, fadeDuration, fadeDelay, fadeCurve);

    /// <summary>
    /// Set alpha instantly with optional interactability control.
    /// </summary>
    public void SetAlpha(float alpha)
    {
        if (currentFadeRoutine != null)
            StopCoroutine(currentFadeRoutine);

        canvasGroup.alpha = Mathf.Clamp01(alpha);
        canvasGroup.interactable = alpha > 0.9f;
        canvasGroup.blocksRaycasts = alpha > 0.9f;
    }

    /// <summary>
    /// Start a fade to any target alpha with full control.
    /// </summary>
    public void FadeTo(float targetAlpha, float duration, float delay, AnimationCurve curve, Action onComplete = null)
    {
        if (currentFadeRoutine != null)
            StopCoroutine(currentFadeRoutine);

        currentFadeRoutine = StartCoroutine(FadeCanvasGroup(targetAlpha, duration, delay, curve, onComplete));
    }

    private IEnumerator FadeCanvasGroup(float targetAlpha, float duration, float delay, AnimationCurve curve, Action onComplete)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curveValue = curve.Evaluate(t);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = targetAlpha > 0.9f;
        canvasGroup.blocksRaycasts = targetAlpha > 0.9f;

        currentFadeRoutine = null;
        onComplete?.Invoke();
    }
}
