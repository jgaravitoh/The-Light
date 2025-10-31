using UnityEngine;
using UnityEngine.Events;

public class DoOnTriggerAndKeyPressed : MonoBehaviour
{
    [Header("Evento a llamar")]
    public UnityEvent onTriggerAndKey;

    [Header("Tecla necesaria")]
    public KeyCode keyToPress = KeyCode.E;

    [Header("Filtrar por tag (opcional)")]
    [Tooltip("D�jalo vac�o para aceptar cualquier collider. Si escribes 'Player', solo el Player podr� activar.")]
    public string requiredTag = "";

    // Para saber si el objeto v�lido est� dentro del trigger
    private bool isInside = false;

    private void Reset()
    {
        // Asegura que el collider sea trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si hay tag requerido y el que entra no lo tiene -> no hacemos nada
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
            return;

        isInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
            return;

        isInside = false;
    }

    private void Update()
    {
        // Solo si alguien v�lido est� dentro y presiona la tecla
        if (isInside && Input.GetKeyDown(keyToPress))
        {
            onTriggerAndKey?.Invoke();
        }
    }
}
