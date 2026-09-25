using UnityEngine;

public static class HexGizmo
{
    public static void Draw(HexAuthor hexAuthor, Mesh hexMesh, Transform transform, bool isWire)
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Color originalColor = Gizmos.color;
        Gizmos.matrix = transform.localToWorldMatrix;

        foreach (HexCoord coord in HexCoord.AllWithinRings(hexAuthor.NumRings))
        {
            Vector3 pos = HexLayout.CoordsToWorld(coord);
            if (isWire)
                Gizmos.DrawWireMesh(hexMesh, pos);
            else
                Gizmos.DrawMesh(hexMesh, pos);
        }

        Gizmos.matrix = originalMatrix;
        Gizmos.color = originalColor;
    }
}
