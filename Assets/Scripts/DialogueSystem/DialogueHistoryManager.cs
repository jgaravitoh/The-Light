using UnityEngine;
using TMPro;
using System;

public class DialogueHistoryManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueHistoryText;
    [SerializeField] private string dialogueHistory;
    [SerializeField] private GameObject historyCanva;
    public static DialogueHistoryManager sharedInstanceDialogueHistoryManager;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (sharedInstanceDialogueHistoryManager == null)
        {
            sharedInstanceDialogueHistoryManager = this;
        }
    }
    public void RegisterDialogue(Dialogue dialogue)
    {
        string hexNameColor = ConvertToHexColor(dialogue.nameColor.r, dialogue.nameColor.g, dialogue.nameColor.b, dialogue.nameColor.a);
        string hexTextColor = ConvertToHexColor(dialogue.textColor.r, dialogue.textColor.g, dialogue.textColor.b, dialogue.textColor.a);
        // Register chacter's name if participating in the dialogue
        dialogueHistory += ("<margin-left=3%><color="+ hexNameColor + ">-" + dialogue.CharacterName + "</color></margin>");
        // Register text from the conversation in the dialogue
        dialogueHistory += ("\n\n" + "<margin-left=7%><color=" + hexTextColor + ">" + dialogue.DialogueText + "</color></margin>"+ "\n\n");
        dialogueHistoryText.SetText(dialogueHistory);
    }
    // Buttons
    public void CloseHistoryPanel() 
    {
        historyCanva.SetActive(false);
    }
    public void OpenHistoryPanel()
    {
        historyCanva.SetActive(true);
    }

    static string ConvertToHexColor(float red, float green, float blue, float alpha)
    {
        // Ensure values are within the valid range
        red = Math.Clamp(red, 0.0f, 1.0f);
        green = Math.Clamp(green, 0.0f, 1.0f);
        blue = Math.Clamp(blue, 0.0f, 1.0f);
        alpha = Math.Clamp(alpha, 0.0f, 1.0f);

        // Convert to byte values (0-255)
        byte r = (byte)(red * 255);
        byte g = (byte)(green * 255);
        byte b = (byte)(blue * 255);
        byte a = (byte)(alpha * 255);

        // Convert to hexadecimal format
        string hexColor = $"#{r:X2}{g:X2}{b:X2}{a:X2}";

        return hexColor;
    }
}
