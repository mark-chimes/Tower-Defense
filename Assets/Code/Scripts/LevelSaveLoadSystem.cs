using System;
using System.IO;
using UnityEngine;

public class LevelSaveLoadSystem
{
    int testValue = 1;

    // TODO later we'll initialize this with treasureMap so it can use it
    private TreasureMap treasureMap;

    public void Initialize(TreasureMap treasureMap) // TODO
    {
        this.treasureMap = treasureMap;
    }

    public void OnSave()
    {
        Debug.Log("saving...");
        SaveableLevel saveable = treasureMap.AsSaveableData();
        WriteToFile(saveable.ToJson());
        testValue++;
    }

    public void OnLoad()
    {
        Debug.Log("loading...");
        string json = ReadJson();
        SaveableLevel saveable = SaveableLevel.FromJson(json);
        // TODO what to do with this loaded data? 
        Debug.Log($"Loaded value {json}");
        Debug.Log($"Loaded value {saveable}");

    }

    // TODO everything below should probably move to file IO class.

    private readonly string filePath = Path.Combine(Application.persistentDataPath, "walls_save.json");

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
