using System;
using UnityEngine;
using Unity.Mathematics;

// TODO probably we want to split this into the 2D Hex Projection with float2 used for the fleet
// And the 3D hex projection with Vector3 used for Unity stuff 
// And only have the 3D version calling the 2D version but not vice-versa
public static class HexProjection
{
    public const float CellWidth = 10f; // center of one cell to center of another - the small diameter
    public static readonly float CellHeight = CellWidth * 2f / Mathf.Sqrt(3f);
    // large diamater / diagonal 
    public const float LandSurfaceY = 1f;

    public static Vector3 CoordsToWorld(HexCoord coord)
    {
        float posX = CellWidth * coord.Q + CellWidth / 2f * coord.R; // Horizontal spacing W
        float posZ = 3f / 4f * CellHeight * coord.R; // Vertical spacing: 3/4 * H
        return new Vector3(posX, 0f, posZ);
    }

    public static Vector3 Float2ToWorld(float2 pos) => new Vector3(pos.x, 0f, pos.y);

    public static float2 CoordsToFloat2(HexCoord coord)
    {
        Vector3 worldPos = CoordsToWorld(coord);
        return new float2(worldPos.x, worldPos.z);
    }

    public static Vector3 CoordsToLandSurface(HexCoord coord) => CoordsToWorld(coord) + Vector3.up * LandSurfaceY;

    private static Vector3 CompassToVector3(HexCompass dir) => CoordsToWorld(dir.Offset());

    // 4. Later, in the smoothing step: 
    // the target angle is something like Mathf.Atan2(dx, dz) * Mathf.Rad2Deg, with dx first
    // but using math, not Mathf
    public static float Float2ToDegreesHeading(float2 f2)
    {
        return math.degrees(math.atan2(f2.x, f2.y));
    }

    public static Quaternion CompassToQuaternion(HexCompass dir)
    {
        if (dir == HexCompass.NONE) { throw new ArgumentOutOfRangeException(nameof(dir)); }
        return Vector3ToQuaternion(CompassToVector3(dir));
    }

    public static Quaternion Vector3ToQuaternion(Vector3 vector3)
    {
        return Quaternion.LookRotation(vector3, Vector3.up);
    }

    public static Quaternion DegreesToQuaternion(float degrees) => Quaternion.Euler(0f, degrees, 0f);

    // See https://www.redblobgames.com/grids/hexagons/#pixel-to-hex
    public static HexCoord WorldToCoords(float x, float z)
    {
        float rf = z / (0.75f * CellHeight);
        float qf = (x - CellWidth / 2 * rf) / CellWidth;

        return HexCoord.AxialRound(qf, rf);
    }
}