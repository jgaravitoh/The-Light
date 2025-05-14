using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
public class LoadingScreenText : MonoBehaviour
{
    // THIS CODE HAST TO BE ATTACHED TO A TEXTMESHPRO GUI GAMEOBJECT!!!
    #region Variables
    [SerializeField]
    [TextArea(2, 15)]
    public List<string> texts;
    public bool useSameTimeToWaitForAllTexts = true;
    public List<float> timesToWaitPerText;
    private TextMeshProUGUI textToChange;
    #endregion

    #region Awake and OnEnable
    private void Awake()
    {
        textToChange = gameObject.GetComponent<TextMeshProUGUI>();
        if (textToChange == null)
        {
            Debug.LogError("TextMeshPro component is missing from this GameObject.");
        }
    }
    private void OnEnable()
    {
        if (texts == null || texts.Count < 1)
        {
            Debug.LogError("Debe haber al menos un texto en la lista 'texts'");
            return;
        }
        if (timesToWaitPerText == null || timesToWaitPerText.Count < 1)
        {
            Debug.LogError("Debe haber al menos un tiempo de espera en la lista 'timesToWaitPerText'");
            return;
        }
        if (useSameTimeToWaitForAllTexts)
        {
            if (timesToWaitPerText.Count > 1)
            {
                timesToWaitPerText = new List<float> { timesToWaitPerText[0] };
            }
        }
        else if (texts.Count != timesToWaitPerText.Count)
        {
            Debug.LogError("Las listas 'texts' y 'timesToWaitPerText' deben tener la misma cantidad de elementos");
            return;
        }

        // Iniciar la corrutina para cambiar el texto
        StartCoroutine(ChangeTextCoroutine());
    }
    #endregion

    #region ChangeTextCoroutine
    private IEnumerator ChangeTextCoroutine()
    {
        while (true)
        {
            for (int i = 0; i < texts.Count; i++)
            {
                // Cambiar el texto
                textToChange.SetText(texts[i]);

                // Esperar el tiempo especificado
                if (useSameTimeToWaitForAllTexts)
                {
                    yield return new WaitForSeconds(timesToWaitPerText[0]);
                }
                else
                {
                    yield return new WaitForSeconds(timesToWaitPerText[i]);
                }
            }
        }
    }
    #endregion
}
