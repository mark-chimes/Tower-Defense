using UnityEngine;

public class HexLayout
{
    public readonly int Width;
    public readonly int Height;
    public const float CellSize = 10f;
    public const float SqrClose = 0.01f; // TODO what's a good value here? 

    public HexLayout(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public Vector3 CoordsToWorld(HexCoord coord)
    {
        float worldX = (coord.X - (Width - 1) / 2f) * CellSize;
        float worldZ = (coord.Z - (Height - 1) / 2f) * CellSize;
        return new Vector3(worldX, 0f, worldZ);
    }

    public Vector3 CompassToVector3(Compass compassDir)
    {
        HexCoord dir = HexCoord.ForDirection(compassDir);
        return new Vector3(dir.X, 0f, dir.Z);
    }

    public Quaternion CompassToQuaternion(Compass compassDir)
    {
        Quaternion targetRotation = Quaternion.LookRotation(CompassToVector3(compassDir));
        return targetRotation;
    }

    public bool AreVector3Close(Vector3 first, Vector3 second)
    {
        return (first - second).sqrMagnitude <= SqrClose;

    }
}