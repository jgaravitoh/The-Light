using UnityEngine;

public class CanvasGroupProximityFade : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform target;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Distancias")]
    [SerializeField] private float distanciaMax = 10f; // Más lejos que esto → invisible
    [SerializeField] private float distanciaMin = 2f;  // Más cerca que esto → totalmente visible

    [Header("Suavizado")]
    [SerializeField] private float velocidadFade = 5f;

    private void Reset()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (target == null || canvasGroup == null) return;

        float distancia = Vector3.Distance(transform.position, target.position);

        // 0 = lejos (distanciaMax), 1 = cerca (distanciaMin)
        float t = Mathf.InverseLerp(distanciaMax, distanciaMin, distancia);
        float alphaObjetivo = t;

        canvasGroup.alpha = Mathf.Lerp(
            canvasGroup.alpha,
            alphaObjetivo,
            Time.deltaTime * velocidadFade
        );
    }
}
