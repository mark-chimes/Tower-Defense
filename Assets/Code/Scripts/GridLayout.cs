using UnityEngine;

public class GridLayout
{
    public readonly int Width;
    public readonly int Height;
    public readonly float CellSize;

    public Vector3 CoordsToWorld(Coord coord)
    {
        float worldX = (coord.X - (Width - 1) / 2f) * CellSize;
        float worldZ = (coord.Z - (Height - 1) / 2f) * CellSize;
        return new Vector3(worldX, 0f, worldZ);
    }
}