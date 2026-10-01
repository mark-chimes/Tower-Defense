using UnityEngine;

[System.Serializable]
public class TowerSaveData
{
    public bool[] IsTower;

    public TowerSaveData(Lattice<bool> towerMap)
    {
        IsTower = towerMap.ToFlatArray();
    }

    // public string ToJson()
    // {
    //     return JsonUtility.ToJson(this);
    // }

    // public static TowerSaveData FromJson(string json)
    // {
    //     return JsonUtility.FromJson<TowerSaveData>(json);
    // }

    public Lattice<bool> GetTowerMap(int NumRings)
    {
        return Lattice<bool>.FromFlatArray(IsTower, NumRings);
    }
}
