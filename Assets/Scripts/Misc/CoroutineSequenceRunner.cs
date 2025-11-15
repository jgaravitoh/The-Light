using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class CoroutineSequenceRunner : MonoBehaviour
{
    [System.Serializable]
    public class CoroutineStep
    {
        [Tooltip("Nombre descriptivo del paso (solo para debug).")]
        public string stepName;

        [Tooltip("Objeto que contiene la corutina a ejecutar.")]
        public MonoBehaviour target;

        [Tooltip("Nombre del método-corutina (IEnumerator) en el target.")]
        public string coroutineMethodName;
    }

    [Header("Pasos de la secuencia (en orden)")]
    public List<CoroutineStep> steps = new List<CoroutineStep>();

    [Tooltip("Esperar 1 frame extra entre corutinas (opcional).")]
    public bool waitOneFrameBetweenSteps = false;

    /// <summary>
    /// Llama a este método desde OTROS scripts para ejecutar la secuencia.
    /// </summary>
    public void RunSequence()
    {
        StartCoroutine(RunSequenceCoroutine());
    }

    private IEnumerator RunSequenceCoroutine()
    {
        if (steps == null || steps.Count == 0)
        {
            Debug.LogWarning("[CoroutineSequenceRunner] No hay pasos configurados.");
            yield break;
        }

        foreach (var step in steps)
        {
            if (step == null || step.target == null || string.IsNullOrEmpty(step.coroutineMethodName))
            {
                Debug.LogWarning("[CoroutineSequenceRunner] Paso inválido, se omite.");
                continue;
            }

            Debug.Log($"[CoroutineSequenceRunner] Ejecutando paso: {step.stepName}");

            // Buscar el método por reflexión
            MethodInfo method = step.target.GetType().GetMethod(
                step.coroutineMethodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (method == null)
            {
                Debug.LogError($"[CoroutineSequenceRunner] No se encontró el método '{step.coroutineMethodName}' en {step.target.name}");
                continue;
            }

            // Invocar el método y obtener el IEnumerator
            var enumerator = method.Invoke(step.target, null) as IEnumerator;
            if (enumerator == null)
            {
                Debug.LogError($"[CoroutineSequenceRunner] El método '{step.coroutineMethodName}' de {step.target.name} no devuelve IEnumerator.");
                continue;
            }

            // 👉 Aquí SÍ esperamos a que la corutina termine antes de pasar al siguiente paso
            yield return StartCoroutine(enumerator);

            if (waitOneFrameBetweenSteps)
                yield return null;
        }

        Debug.Log("[CoroutineSequenceRunner] Secuencia completa.");
    }
}
