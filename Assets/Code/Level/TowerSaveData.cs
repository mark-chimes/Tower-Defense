using UnityEngine;

[System.Serializable]
public class TowerSaveData
{
    public bool[] IsTower;

    public TowerSaveData(Lattice<bool> towerMap)
    {
        IsTower = towerMap.ToFlatArray();
    }

    public Lattice<bool> GetTowerMap(int numRings)
    {
        return Lattice<bool>.FromFlatArray(IsTower, numRings);
    }
}
