using System;
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
                    Debug.LogError("CSV contains no data rows.");
                    onLoaded?.Invoke(default);
                    return;
                }

                int rowCount = lines.Length - 1; // Exclude header
                DialogueTable table = new DialogueTable(rowCount);

                for (int i = 1; i < lines.Length; i++)
                {
                    string[] fields = CSVParser.ParseLine(lines[i]);
                    int index = i - 1;

                    table.Ids[index] = int.Parse(fields[0]);
                    table.Separators[index] = fields[1].Trim();
                    table.CharacterNames[index] = fields[2].Trim();
                    table.Dialogues[index] = fields[3].Trim();
                    table.ColorNames[index] = fields[4].Trim();
                    table.ColorDialogues[index] = fields[5].Trim();
                    table.SpeedDialogues[index] = float.Parse(fields[6], CultureInfo.InvariantCulture);
                    table.ImageNames[index] = fields[7].Trim().ToLower() == "n/a" ? null : fields[7].Trim();
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
}
