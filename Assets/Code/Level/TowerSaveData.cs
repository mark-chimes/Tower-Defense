using UnityEngine;

[System.Serializable]
public class TowerSaveData
{
    public int[] IsTower;

    public TowerSaveData(Lattice<int> towerMap)
    {
        IsTower = towerMap.ToFlatArray();
    }

    public Lattice<int> GetTowerMap(int numRings)
    {
        return Lattice<int>.FromFlatArray(IsTower, numRings);
    }
}
