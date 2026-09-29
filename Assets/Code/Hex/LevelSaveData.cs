using UnityEngine;

[System.Serializable]

public class LevelSaveData
{
    public int NumRings;
    public bool[] IsWall; // 1D flattening of 2D array

    public LevelSaveData(Lattice<bool> wallMap)
    {
        NumRings = wallMap.NumRings;
        IsWall = wallMap.MapAsFlatArray();
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static LevelSaveData FromJson(string json)
    {
        return JsonUtility.FromJson<LevelSaveData>(json);
    }

    public Lattice<bool> LoadMap()
    {
        return Lattice<bool>.MapFromArray(IsWall, NumRings);
    }

    public override string ToString()
    {
        return $"LevelSaveData NumRings: {NumRings}";
    }

}
