using System.Globalization;
using System.IO;
using CsvHelper;
using UnityEngine;
using System;
using System.Collections.Generic;

public class CSVDataservice : IDataService
{
    public bool SaveData<T>(string relativePath, T data, bool encrypted)
    {
        string path = Application.persistentDataPath + relativePath;

        try
        {
            if (File.Exists(path))
            {
                Debug.Log("Data exists, deleting old file and writing new one!");
                File.Delete(path);
            }

            using var writer = new StreamWriter(path);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            if (data is IEnumerable<object> list)
            {
                csv.WriteRecords(list);
            }
            else
            {
                csv.WriteRecord(data);
                writer.WriteLine();
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save CSV: {ex.Message}");
            return false;
        }
    }

    public virtual T LoadData<T>(string relativePath, bool encrypted)
    {
        string path = Application.persistentDataPath + relativePath;

        try
        {
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var data = csv.GetRecords<T>();
            return data != null ? (T)(object)data : default;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load CSV: {ex.Message}");
            return default;
        }
    }
}
