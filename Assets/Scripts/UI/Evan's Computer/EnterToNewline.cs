using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class EnterToNewline : MonoBehaviour
{
    public TMP_InputField inputField;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
        {
            // Get current text and caret position
            string text = inputField.text;
            int caretPos = inputField.stringPosition;

            // Insert newline at caret
            string newline = "\r\n";
            text = text.Insert(caretPos, newline);
            inputField.text = text;

            // Update caret position (after inserted newline)
            int newCaretPos = caretPos + newline.Length;
            inputField.stringPosition = newCaretPos;
            inputField.caretPosition = newCaretPos;

            // Re-focus the input field to keep it active
            EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
            inputField.OnSelect(null);
        }
    }
}