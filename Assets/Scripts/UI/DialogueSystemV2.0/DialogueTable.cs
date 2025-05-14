using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTable
{
    public int[] Ids;
    public string[] Separators;
    public string[] CharacterNames;
    public string[] Dialogues;
    public string[] ColorNames;
    public string[] ColorDialogues;
    public float[] SpeedDialogues;
    public string[] ImageNames;

    private Dictionary<int, int> idToIndexMap;

    public DialogueTable(int size)
    {
        Ids = new int[size];
        Separators = new string[size];
        CharacterNames = new string[size];
        Dialogues = new string[size];
        ColorNames = new string[size];
        ColorDialogues = new string[size];
        SpeedDialogues = new float[size];
        ImageNames = new string[size];
    }

    public void ClearData()
    {
        Array.Clear(Ids, 0, Ids.Length);
        Array.Clear(Separators, 0, Separators.Length);
        Array.Clear(CharacterNames, 0, CharacterNames.Length);
        Array.Clear(Dialogues, 0, Dialogues.Length);
        Array.Clear(ColorNames, 0, ColorNames.Length);
        Array.Clear(ColorDialogues, 0, ColorDialogues.Length);
        Array.Clear(SpeedDialogues, 0, SpeedDialogues.Length);
        Array.Clear(ImageNames, 0, ImageNames.Length);
    }

    public void PrintDialogueAt(int index)
    {
        if (index < 0 || index >= Ids.Length) return;
        Debug.Log($"[{index}] {CharacterNames[index]} says: \"{Dialogues[index]}\"");
    }

    public string GetDialogueById(int id)
    {
        if (idToIndexMap == null)
            BuildIdMap();

        if (idToIndexMap.TryGetValue(id, out int index))
            return Dialogues[index];
        else
            return null;
    }

    private void BuildIdMap()
    {
        idToIndexMap = new Dictionary<int, int>();
        for (int i = 0; i < Ids.Length; i++)
        {
            idToIndexMap[Ids[i]] = i;
        }
    }
}
