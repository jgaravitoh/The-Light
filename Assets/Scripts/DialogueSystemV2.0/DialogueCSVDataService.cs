using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DialogueCSVDataService : CSVDataservice
{
    public void LoadDataAsync<T>(string address, bool encrypted, Action<T> onLoaded)
    {
        Addressables.LoadAssetAsync<TextAsset>(address).Completed += handle =>
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load CSV at address: {address}");
                onLoaded?.Invoke(default);
                return;
            }

            try
            {
                string[] lines = handle.Result.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length <= 1)
                {
                    onLoaded?.Invoke(default);
                    return;
                }

                List<string[]> validRows = new List<string[]>();

                for (int i = 1; i < lines.Length; i++) // Skip header
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] fields = CSVParser.ParseLine(line);

                    if (fields.Length != 8)
                        continue;

                    if (AreAllFieldsEmpty(fields))
                        continue;

                    validRows.Add(fields);
                }

                DialogueTable table = new DialogueTable(validRows.Count);

                for (int i = 0; i < validRows.Count; i++)
                {
                    string[] fields = validRows[i];

                    table.Ids[i] = int.Parse(fields[0]);
                    table.Separators[i] = fields[1].Trim();
                    table.CharacterNames[i] = fields[2].Trim();
                    table.Dialogues[i] = fields[3].Trim();
                    table.ColorNames[i] = fields[4].Trim();
                    table.ColorDialogues[i] = fields[5].Trim();
                    table.SpeedDialogues[i] = float.Parse(fields[6], CultureInfo.InvariantCulture);
                    table.ImageNames[i] = fields[7].Trim().ToLower() == "n/a" ? null : fields[7].Trim();
                }

                onLoaded?.Invoke((T)(object)table);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing CSV: {ex.Message}");
                onLoaded?.Invoke(default);
            }
        };
    }

    public override T LoadData<T>(string relativePath, bool encrypted)
    {
        Debug.LogError("Use LoadDataAsync instead for Addressables.");
        return default;
    }

    private bool AreAllFieldsEmpty(string[] fields)
    {
        foreach (var field in fields)
        {
            if (!string.IsNullOrWhiteSpace(field) && field.Trim().ToLower() != "n/a")
                return false;
        }
        return true;
    }
}
