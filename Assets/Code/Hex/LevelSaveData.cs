using UnityEngine;

[System.Serializable]

// Rename to LevelSaveData
public class HexSaveableLevel
{
    public int NumRings;
    public bool[] IsWall; // 1D flattening of 2D array

    public HexSaveableLevel(HexMap<bool> wallMap)
    {
        NumRings = wallMap.NumRings;
        IsWall = wallMap.MapAsFlatArray();
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static HexSaveableLevel FromJson(string json)
    {
        return JsonUtility.FromJson<HexSaveableLevel>(json);
    }

    public HexMap<bool> LoadMap()
    {
        return HexMap<bool>.MapFromArray(IsWall, NumRings);
    }

    public override string ToString()
    {
        return $"SaveableLevel NumRings: {NumRings}";
    }

}
