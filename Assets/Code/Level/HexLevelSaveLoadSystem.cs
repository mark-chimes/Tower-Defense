
using System.IO;
using UnityEngine;

// TODO this class should be reworked when I figure it out. Rename when it's reworked
public class HexLevelSaveLoadSystem
{
    // TODO should I save the map, or something else?
    public void Save(LevelSaveData saveable)
    {
        WriteToFile(saveable.ToJson());
    }

    public LevelSaveData Load()
    {
        Debug.Log("loading...");
        string json = ReadJson();
        LevelSaveData saveable = LevelSaveData.FromJson(json);
        Debug.Log($"Loaded: {saveable}");

        return saveable;

    }

    private readonly string filePath = Path.Combine(Application.persistentDataPath, 
        "level_save.json");

    public void WriteToFile(string jsonData)
    {
        // TODO try-catch
        File.WriteAllText(filePath, jsonData.ToString());
    }

    public string ReadJson()
    {
        // TODO try-catch
        return File.ReadAllText(filePath);
    }
}
