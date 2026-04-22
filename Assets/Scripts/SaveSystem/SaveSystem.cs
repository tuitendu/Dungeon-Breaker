using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Stateless utility for reading and writing save files.
/// Does not depend on any scene objects.
/// </summary>
public static class SaveSystem
{
    private const string FILE_NAME = "save_slot_{0}.json";

    // Returns the full path for a given slot index (0-based).
    public static string GetSavePath(int slot = 0)
    {
        string fileName = string.Format(FILE_NAME, slot);
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    /// <summary>
    /// Serializes <paramref name="data"/> to JSON and writes it to disk.
    /// </summary>
    public static void Save(SaveData data, int slot = 0)
    {
        try
        {
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            string path = GetSavePath(slot);

            File.WriteAllText(path, json);
            Debug.Log("[SaveSystem] Saved to: " + path);
        }
        catch (Exception ex)
        {
            Debug.LogError("[SaveSystem] Save failed: " + ex.Message);
        }
    }

    /// <summary>
    /// Reads the save file for <paramref name="slot"/> and returns a populated
    /// <see cref="SaveData"/>, or <c>null</c> if the file does not exist.
    /// </summary>
    public static SaveData Load(int slot = 0)
    {
        string path = GetSavePath(slot);

        if (!File.Exists(path))
        {
            Debug.Log("[SaveSystem] No save file found at: " + path);
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("[SaveSystem] Loaded from: " + path);
            return data;
        }
        catch (Exception ex)
        {
            Debug.LogError("[SaveSystem] Load failed: " + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Deletes the save file for <paramref name="slot"/> if it exists.
    /// </summary>
    public static void Delete(int slot = 0)
    {
        string path = GetSavePath(slot);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("[SaveSystem] Deleted save at: " + path);
        }
    }

    /// <summary>
    /// Returns true if a save file exists for <paramref name="slot"/>.
    /// </summary>
    public static bool HasSave(int slot = 0)
    {
        return File.Exists(GetSavePath(slot));
    }
}
