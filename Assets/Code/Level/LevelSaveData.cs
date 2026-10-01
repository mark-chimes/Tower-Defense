using UnityEngine;

[System.Serializable]

public class LevelSaveData
{
    public int NumRings;

    public WallSaveData Walls; // TODO will be terrain or something later
    public TowerSaveData Towers;

    public LevelSaveData(int numRings, WallSaveData walls, TowerSaveData towers)
    {
        NumRings = numRings;
        Walls = walls;
        Towers = towers;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static LevelSaveData FromJson(string json)
    {
        return JsonUtility.FromJson<LevelSaveData>(json);
    }
}
