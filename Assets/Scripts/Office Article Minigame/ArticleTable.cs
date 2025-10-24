using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ArticleTable
{
    public int[] Ids;
    public string[] Titles;
    public string[] Ledes;
    public string[] Templates;
    public string[] Slot1OptionsRaw;  // "A|B|C"
    public int[] Slot1ViralIndex;
    public string[] Slot2OptionsRaw;
    public int[] Slot2ViralIndex;
    public string[] Slot3OptionsRaw;
    public int[] Slot3ViralIndex;
    public string[] Tags;

    private Dictionary<int, int> idToIndexMap;

    public void BuildIdMap()
    {
        idToIndexMap = new Dictionary<int, int>();
        for (int i = 0; i < Ids.Length; i++) idToIndexMap[Ids[i]] = i;
    }

    public bool TryGetIndex(int id, out int index)
    {
        if (idToIndexMap == null)
        {
            index = -1;              // valor seguro si no hay mapa
            return false;
        }
        return idToIndexMap.TryGetValue(id, out index);
    }

    public string[] GetSlotOptions(int id, int slot)
    {
        if (!TryGetIndex(id, out var i)) return Array.Empty<string>();
        var raw = slot switch
        {
            1 => Slot1OptionsRaw[i],
            2 => Slot2OptionsRaw[i],
            3 => Slot3OptionsRaw[i],
            _ => null
        };
        if (string.IsNullOrEmpty(raw)) return Array.Empty<string>();
        var parts = raw.Split('|');
        for (int k = 0; k < parts.Length; k++) parts[k] = parts[k].Trim();
        return parts;
    }

    public int GetViralIndex(int id, int slot)
    {
        if (!TryGetIndex(id, out var i)) return -1;
        return slot switch
        {
            1 => Slot1ViralIndex[i],
            2 => Slot2ViralIndex[i],
            3 => Slot3ViralIndex[i],
            _ => -1
        };
    }

    public string ComposeBody(int id, int opt1, int opt2, int opt3)
    {
        if (!TryGetIndex(id, out var i)) return null;
        string body = Templates[i];
        string o1 = GetSafe(GetSlotOptions(id, 1), opt1);
        string o2 = GetSafe(GetSlotOptions(id, 2), opt2);
        string o3 = GetSafe(GetSlotOptions(id, 3), opt3);
        return body.Replace("{1}", o1).Replace("{2}", o2).Replace("{3}", o3);
    }

    private string GetSafe(string[] arr, int idx) =>
        (arr != null && idx >= 0 && idx < arr.Length) ? arr[idx] : "";
}
