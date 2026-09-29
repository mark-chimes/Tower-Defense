using System;
using UnityEngine;

// Rename to HexProjection
public static class HexLayout
{
    public const float CellWidth = 10f; // center of one cell to center of another - the small diameter
    public static readonly float CellHeight = CellWidth * 2f / Mathf.Sqrt(3f);
    // large diamater / diagonal 


    public const float SqrClose = 0.01f; // TODO what's a good value here? 

    public static Vector3 CoordsToWorld(HexCoord coord)
    {
        float posX = CellWidth * coord.Q + CellWidth / 2f * coord.R; // Horizontal spacing W
        float posZ = 3f / 4f * CellHeight * coord.R; // Vertical spacing: 3/4 * H
        return new Vector3(posX, 0f, posZ);
    }

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
}