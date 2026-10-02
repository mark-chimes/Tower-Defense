using System;
using UnityEngine;

public static class HexProjection
{
    public const float CellWidth = 10f; // center of one cell to center of another - the small diameter
    public static readonly float CellHeight = CellWidth * 2f / Mathf.Sqrt(3f);
    // large diamater / diagonal 
    public const float LandSurfaceY = 1f;

    public const float SqrClose = 0.01f; // TODO what's a good value here? 

    public static Vector3 CoordsToWorld(HexCoord coord)
    {
        float posX = CellWidth * coord.Q + CellWidth / 2f * coord.R; // Horizontal spacing W
        float posZ = 3f / 4f * CellHeight * coord.R; // Vertical spacing: 3/4 * H
        return new Vector3(posX, 0f, posZ);
    }

    public static Vector3 Vector2ToWorld(Vector2 pos) => new Vector3(pos.x, 0f, pos.y);

    public static Vector2 CoordsToVector2(HexCoord coord)
    {
        Vector3 worldPos = CoordsToWorld(coord);
        return new Vector2(worldPos.x, worldPos.z);
    }

    public static Vector3 CoordsToLandSurface(HexCoord coord) => CoordsToWorld(coord) + Vector3.up * LandSurfaceY;

    public static Vector3 CompassToVector3(HexCompass dir) => CoordsToWorld(dir.Offset());

    public static Quaternion CompassToQuaternion(HexCompass dir)
    {
        if (dir == HexCompass.NONE) { throw new ArgumentOutOfRangeException(nameof(dir)); }
        return Quaternion.LookRotation(CompassToVector3(dir), Vector3.up);
    }

    public static bool AreVector3Close(Vector3 first, Vector3 second)
    {
        return (first - second).sqrMagnitude <= SqrClose;
    }

    // See https://www.redblobgames.com/grids/hexagons/#pixel-to-hex
    public static HexCoord WorldToCoords(float x, float z)
    {
        float rf = z / (0.75f * CellHeight);
        float qf = (x - CellWidth / 2 * rf) / CellWidth;

        return HexCoord.AxialRound(qf, rf);
    }
}