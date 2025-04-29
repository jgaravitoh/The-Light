using System;
using UnityEngine;

public class FirstTestDialogues_v2 : MonoBehaviour
{
    void Start()
    {
        var service = new DialogueCSVDataService();
        service.LoadDataAsync<DialogueTable>("Assets/Dialogue/DialoguesTest1.csv", false, (table) =>
        {
            if (table == null)
            {
                Debug.LogError("Could not load dialogue.");
                return;
            }
        for(int i=0; i<table.Dialogues.Length; i++) {
                if (table.ImageNames[i] == null)
                {
                    Debug.Log($"[{table.Ids[i]}] [{table.Separators[i]}] image: [null] {table.CharacterNames[i]} says: \"{table.Dialogues[i]}\"");
                }
                else
                {
                    Debug.Log($"[{table.Ids[i]}] [{table.Separators[i]}] image: [{table.ImageNames[i]}] {table.CharacterNames[i]} says: \"{table.Dialogues[i]}\"");

                }
            }
            
        });
    }
}
