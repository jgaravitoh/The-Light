using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FunctionSequenceRunner : MonoBehaviour
{
    [System.Serializable]
    public class SequenceStep
    {
        [Tooltip("Solo para identificar el paso en logs.")]
        public string stepName;

        [Tooltip("Funciones que se ejecutan en este paso.")]
        public UnityEvent onStep;
    }

    [Header("Pasos de la secuencia")]
    public List<SequenceStep> steps = new List<SequenceStep>();

    [Tooltip("Esperar 1 frame entre paso y paso (opcional).")]
    public bool waitOneFrameBetweenSteps = false;

    /// <summary>
    /// Ejecuta toda la secuencia en orden.
    /// Llama a este método desde OTROS scripts.
    /// </summary>
    public void RunSequence()
    {
        StartCoroutine(RunSequenceCoroutine());
    }

    private IEnumerator RunSequenceCoroutine()
    {
        if (steps == null || steps.Count == 0)
        {
            Debug.LogWarning("[FunctionSequenceRunner] No hay pasos configurados.");
            yield break;
        }

        foreach (var step in steps)
        {
            if (step == null) continue;

            Debug.Log($"[FunctionSequenceRunner] Ejecutando paso: {step.stepName}");
            step.onStep?.Invoke();   // 👉 Aquí se ejecutan TODAS las funciones asignadas al paso

            if (waitOneFrameBetweenSteps)
                yield return null;
        }

        Debug.Log("[FunctionSequenceRunner] Secuencia terminada.");
    }
}
