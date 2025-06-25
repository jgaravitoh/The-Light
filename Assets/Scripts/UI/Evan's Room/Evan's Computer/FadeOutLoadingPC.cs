using UnityEngine;
using System.Collections;

public class FadeLoadingPC : MonoBehaviour
{
    CanvasGroupFader canvasGroupFader;
    [SerializeField] GameObject loadingPanel;
    [SerializeField] float targetAlpha;
    [SerializeField] float duration;
    [SerializeField] float delay;
    [SerializeField] float timeTilFade;
    private void OnEnable()
    {
        loadingPanel.SetActive(true);
        canvasGroupFader = loadingPanel.GetComponent<CanvasGroupFader>();
        StartCoroutine(DelayedFade());
    }
    private IEnumerator DelayedFade()
    {
        yield return new WaitForSeconds(timeTilFade);
        FadeOut();
    }
    private void FadeOut()
    {
        canvasGroupFader.FadeTo(
                targetAlpha: targetAlpha,
                duration: duration,
                delay: delay,
                curve: AnimationCurve.EaseInOut(0, 0, 1, 1),
                onComplete: ResetLoadingPanel
                );
    }
    private void ResetLoadingPanel()
    {
        Debug.Log("Panel got reset!");
        canvasGroupFader.SetAlpha(1);
        loadingPanel.SetActive(false);
    }



}
