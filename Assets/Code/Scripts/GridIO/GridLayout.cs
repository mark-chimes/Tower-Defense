using UnityEngine;

public class GridLayout
{
    public readonly int Width;
    public readonly int Height;
    public const float CellSize = 10f;

    public GridLayout(int width, int height) { 
        Width = width;
        Height = height;
    }

    public Vector3 CoordsToWorld(Coord coord)
    {
        float worldX = (coord.X - (Width - 1) / 2f) * CellSize;
        float worldZ = (coord.Z - (Height - 1) / 2f) * CellSize;
        return new Vector3(worldX, 0f, worldZ);
    }

    public Quaternion CompassToQuaternion(Compass compassDir)
    {
        return new Quaternion(30f, 30f, 30f, 30f); // TODO implement
    }
}