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

    public Lattice<bool> GetWallMap(int NumRings)
    {
        return Lattice<bool>.FromFlatArray(IsWall, NumRings);
    }
}
