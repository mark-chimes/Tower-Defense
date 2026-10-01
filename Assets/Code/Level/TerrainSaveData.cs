[System.Serializable]

public class TerrainSaveData
{
    public bool[] IsLand; // 1D flattening of 2D array

    public TerrainSaveData(Lattice<bool> landMap)
    {
        IsLand = landMap.ToFlatArray();
    }

    public Lattice<bool> GetLandMap(int numRings)
    {
        return Lattice<bool>.FromFlatArray(IsLand, numRings);
    }
}
