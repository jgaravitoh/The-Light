using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Servicio para cargar mapas rítmicos desde archivos CSV utilizando Addressables.
/// </summary>
public class RhythmMapCSVDataService : CSVDataservice
{
    /// <summary>
    /// Carga un archivo CSV de ritmo desde Addressables y lo convierte en una tabla de ritmo.
    /// </summary>
    /// <typeparam name="T">Tipo del objeto a retornar, debe ser RhythmMapTable.</typeparam>
    /// <param name="address">Ruta Addressable del CSV.</param>
    /// <param name="encrypted">No utilizado actualmente, incluido por compatibilidad.</param>
    /// <param name="onLoaded">Callback que recibe la tabla cargada o null si falla.</param>
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

                // Leer encabezado para determinar el número de columnas de duración
                string[] headerFields = lines[0].Trim().Split(',');
                int durationColumnCount = headerFields.Length - 1; // -1 porque la primera columna es el tiempo

                List<float[]> durationRows = new List<float[]>();
                List<float> timeStamps = new List<float>();

                for (int i = 1; i < lines.Length; i++) // Saltar encabezado
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] fields = line.Split(',');

                    if (fields.Length != durationColumnCount + 1)
                        continue; // Saltar filas mal formateadas

                    // Parsear tiempo
                    float time = float.Parse(fields[0], CultureInfo.InvariantCulture);

                    // Parsear duraciones
                    float[] durations = new float[durationColumnCount];
                    for (int j = 0; j < durationColumnCount; j++)
                    {
                        durations[j] = float.Parse(fields[j + 1], CultureInfo.InvariantCulture);
                    }

                    timeStamps.Add(time);
                    durationRows.Add(durations);
                }

                // Crear la tabla
                RhythmMapTable table = new RhythmMapTable(timeStamps.Count, durationColumnCount);

                for (int i = 0; i < timeStamps.Count; i++)
                {
                    table.hitTimeStamps[i] = timeStamps[i];
                    for (int j = 0; j < durationColumnCount; j++)
                    {
                        table.hitDurations[i, j] = durationRows[i][j];
                    }
                }

                onLoaded?.Invoke((T)(object)table);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing Rhythm CSV: {ex.Message}");
                onLoaded?.Invoke(default);
            }
        };
    }

    /// <summary>
    /// Este método no se usa en esta implementación. Solo se incluye para sobrescribir la clase base.
    /// </summary>
    public override T LoadData<T>(string relativePath, bool encrypted)
    {
        Debug.LogError("Use LoadDataAsync instead for Addressables.");
        return default;
    }
}
