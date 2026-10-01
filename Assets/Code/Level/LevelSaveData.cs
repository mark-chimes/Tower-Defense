using UnityEngine;

[System.Serializable]

public class LevelSaveData
{
    public int NumRings;

    public TerrainSaveData Terrain;
    public TowerSaveData Towers;

    public LevelSaveData(int numRings, TerrainSaveData terrain, TowerSaveData towers)
    {
        NumRings = numRings;
        Terrain = terrain;
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
