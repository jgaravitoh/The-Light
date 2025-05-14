using UnityEngine;
using TMPro;
using System;

public class DialogueHistorySystemManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueHistoryText;
    [SerializeField] private string dialogueHistory;
    [SerializeField] private GameObject historyCanva;
    public static DialogueHistorySystemManager sharedInstanceDialogueHistoryManager;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (sharedInstanceDialogueHistoryManager == null)
        {
            sharedInstanceDialogueHistoryManager = this;
        }
    }
    public void RegisterDialogue(string separator, string characterName, string dialogueText, string colorName, string colorDialogue, float speedDialogue, string imageName)
    {

        // Register chacter's name if participating in the dialogue
        dialogueHistory += ("<margin-left=3%><color="+ colorName + ">-" + characterName + "</color></margin>");
        // Register text from the conversation in the dialogue
        dialogueHistory += ("\n\n" + "<margin-left=7%><color=" + colorDialogue + ">" + dialogueText + "</color></margin>"+ "\n\n");
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
