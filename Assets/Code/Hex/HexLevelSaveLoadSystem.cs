
using System.IO;
using UnityEngine;

// TODO this class should be reworked when I figure it out. Rename when it's reworked
public class HexLevelSaveLoadSystem
{
    // TODO should I save the map, or something else?
    public void SaveMap(HexTreasureMap map)
    {
        Debug.Log("saving...");
        HexSaveableLevel saveable = map.AsSaveableData();
        Debug.Log($"Saving: {saveable}");

        WriteToFile(saveable.ToJson());
    }

    public HexSaveableLevel OnLoad()
    {
        Debug.Log("loading...");
        string json = ReadJson();
        HexSaveableLevel saveable = HexSaveableLevel.FromJson(json);
        Debug.Log($"Loaded: {saveable}");

        return saveable;

    }

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
