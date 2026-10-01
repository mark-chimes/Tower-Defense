using UnityEngine;

public class TowerSaveData
{
    public bool[] IsTower;

    public TowerSaveData(Lattice<bool> towerMap)
    {
        IsTower = towerMap.ToFlatArray();
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
        return Lattice<bool>.FromFlatArray(IsTower, NumRings);
    } 
}
