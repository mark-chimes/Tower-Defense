using System.IO;
using UnityEngine;

public class LevelSaveLoadSystem
{
    int testValue = 1;

    // TODO later we'll initialize this with treasureMap so it can use it

    public void OnSave()
    {
        Debug.Log("saving...");
        Write(testValue);
        testValue++;
    }

    public void OnLoad()
    {
        Debug.Log("loading...");
        int readValue = Read();
        Debug.Log($"Loaded value {readValue}");
    }

    // TODO everything below should probably move to file IO class.

    private readonly string filePath = Path.Combine(Application.persistentDataPath, "levelsave.txt");

    public void Write(int value)
    {
        File.WriteAllText(filePath, value.ToString());
    }

    public int Read()
    {
        return int.Parse(File.ReadAllText(filePath));
    }
}
