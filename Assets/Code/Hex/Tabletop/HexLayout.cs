using UnityEngine;

public static class HexLayout
{
    public const float CellWidth = 10f; // center of one cell to center of another - the small diameter
    public static readonly float CellHeight = CellWidth * 2f / Mathf.Sqrt(3f); 
    // large diamater / diagonal 


    public const float SqrClose = 0.01f; // TODO what's a good value here? 

    public static Vector3 CoordsToWorld(HexCoord coord)
    {
        float posX = CellWidth * coord.Q + CellWidth / 2f * coord.R; // Horizontal spacing W
        float posZ = 3f/4f * CellHeight  * coord.R; // Vertical spacing: 3/4 * H
        return new Vector3 (posX, 0f, posZ);
    }

    // public Vector3 CompassToVector3(HexCompass compassDir)
    // {
    //     HexCoord dir = HexCoord.ForDirection(compassDir);
    //     return new Vector3(dir.X, 0f, dir.Z);
    // }

    // public Quaternion CompassToQuaternion(HexCompass compassDir)
    // {
    //     Quaternion targetRotation = Quaternion.LookRotation(CompassToVector3(compassDir));
    //     return targetRotation;
    // }

    public static bool AreVector3Close(Vector3 first, Vector3 second)
    {
        return (first - second).sqrMagnitude <= SqrClose;

    }
}