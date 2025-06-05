using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FadePanelAndText : MonoBehaviour
{
    public CanvasGroup panelCanvasGroup;
    public GameObject panelObject; // GameObject for enabling/disabling the panel
    public List<TextMeshProUGUI> panelTexts; // Now supports multiple texts
    public float fadeDurationPanel = 1f, fadeDurationText = 1f;
    public int hoursAdded = 3;

    private void OnEnable()
    {
        FadePanelAndTextSequence();
    }

    public void FadePanelAndTextSequence()
    {
        StartCoroutine(FadeInThenOutSequence());
    }

    private IEnumerator FadeInThenOutSequence()
    {
        panelObject.SetActive(true); // Ensure panel is active

        // Fade in panel
        yield return StartCoroutine(FadeCanvasGroup(panelCanvasGroup, 0f, 1f, false));

        // Fade in all texts
        foreach (var text in panelTexts)
        {
            yield return StartCoroutine(FadeTextAlpha(text, text.color.a, 1f));
        }

        // Optional delay before fading out
        yield return new WaitForSeconds(1f);
        TimeManager.sharedInstanceTimeManager.AddHours(hoursAdded);

        // Fade out all texts
        foreach (var text in panelTexts)
        {
            yield return StartCoroutine(FadeTextAlpha(text, 1f, 0f));
        }

        // Fade out panel and disable
        yield return StartCoroutine(FadeCanvasGroup(panelCanvasGroup, 1f, 0f, true));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, bool disableAtEnd)
    {
        float elapsed = 0f;
        cg.interactable = endAlpha > 0f;
        cg.blocksRaycasts = endAlpha > 0f;

        while (elapsed < fadeDurationPanel)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDurationPanel);
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        cg.alpha = endAlpha;

        if (disableAtEnd && panelObject != null)
        {
            panelObject.SetActive(false);
        }
    }

    private IEnumerator FadeTextAlpha(TextMeshProUGUI text, float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color color = text.color;

        while (elapsed < fadeDurationText)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDurationText);
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            text.color = color;
            yield return null;
        }

        color.a = endAlpha;
        text.color = color;
    }
}
