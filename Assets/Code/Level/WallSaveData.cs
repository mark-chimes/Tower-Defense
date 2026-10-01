using UnityEngine;

// Will rename to LandSaveData or something after wall-rename
    [System.Serializable]

public class WallSaveData
{
    public bool[] IsWall; // 1D flattening of 2D array

    public WallSaveData(Lattice<bool> wallMap)
    {
        IsWall = wallMap.ToFlatArray();
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static LevelSaveData FromJson(string json)
    {
        return JsonUtility.FromJson<LevelSaveData>(json);
    }

    public Lattice<bool> ToWallMap(int NumRings)
    {
        return Lattice<bool>.FromFlatArray(IsWall, NumRings);
    }
}
