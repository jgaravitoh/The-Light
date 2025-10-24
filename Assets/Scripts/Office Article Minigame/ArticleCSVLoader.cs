using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class ArticleCSVLoader
{
    // Carga por Addressables (TextAsset) y parsea a ArticleTable.
    public static void LoadAsync(string address, Action<ArticleTable> onLoaded)
    {
        Addressables.LoadAssetAsync<TextAsset>(address).Completed += handle =>
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[ArticleCSVLoader] Falló cargar CSV: {address}");
                onLoaded?.Invoke(null);
                return;
            }
            try
            {
                var table = Parse(handle.Result.text);
                onLoaded?.Invoke(table);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ArticleCSVLoader] Error parseando CSV: {ex}");
                onLoaded?.Invoke(null);
            }
        };
    }

    // Parser CSV -> ArticleTable (usa tu CSVParser.ParseLine).
    public static ArticleTable Parse(string csvText)
    {
        var lines = csvText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        if (lines.Length <= 1) throw new Exception("CSV vacío o sin filas.");

        // --- columnas temporales (listas) ---
        var Ids = new List<int>();
        var Titles = new List<string>();
        var Ledes = new List<string>();
        var Templates = new List<string>();
        var Slot1OptionsRaw = new List<string>();
        var Slot1ViralIndex = new List<int>();
        var Slot2OptionsRaw = new List<string>();
        var Slot2ViralIndex = new List<int>();
        var Slot3OptionsRaw = new List<string>();
        var Slot3ViralIndex = new List<int>();
        var Tags = new List<string>();

        // --- header ---
        int start = 0;
        var header = CSVParser.ParseLine(lines[start++]); // ignora cabecera

        // --- filas ---
        for (int r = start; r < lines.Length; r++)
        {
            var line = lines[r];
            if (string.IsNullOrWhiteSpace(line)) continue;

            var f = CSVParser.ParseLine(line);
            // Esperamos 11 columnas según cabecera
            if (f.Length < 11)
            {
                Debug.LogWarning($"[ArticleCSVLoader] Fila {r + 1} incompleta, se ignora.");
                continue;
            }

            try
            {
                Ids.Add(int.Parse(f[0].Trim()));
                Titles.Add(f[1].Trim());
                Ledes.Add(f[2].Trim());
                Templates.Add(f[3]); // mantener tal cual (puede traer comas)

                Slot1OptionsRaw.Add(f[4].Trim());
                Slot1ViralIndex.Add(ParseIndexSafe(f[5]));

                Slot2OptionsRaw.Add(f[6].Trim());
                Slot2ViralIndex.Add(ParseIndexSafe(f[7]));

                Slot3OptionsRaw.Add(f[8].Trim());
                Slot3ViralIndex.Add(ParseIndexSafe(f[9]));

                Tags.Add(f[10].Trim());
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ArticleCSVLoader] Error fila {r + 1}: {ex}");
            }
        }

        var table = new ArticleTable
        {
            Ids = Ids.ToArray(),
            Titles = Titles.ToArray(),
            Ledes = Ledes.ToArray(),
            Templates = Templates.ToArray(),
            Slot1OptionsRaw = Slot1OptionsRaw.ToArray(),
            Slot1ViralIndex = Slot1ViralIndex.ToArray(),
            Slot2OptionsRaw = Slot2OptionsRaw.ToArray(),
            Slot2ViralIndex = Slot2ViralIndex.ToArray(),
            Slot3OptionsRaw = Slot3OptionsRaw.ToArray(),
            Slot3ViralIndex = Slot3ViralIndex.ToArray(),
            Tags = Tags.ToArray()
        };
        table.BuildIdMap();
        return table;
    }

    private static int ParseIndexSafe(string s)
    {
        if (int.TryParse(s.Trim(), out var idx)) return idx;
        return 0; // por defecto, 0 (primera opción)
    }
}
