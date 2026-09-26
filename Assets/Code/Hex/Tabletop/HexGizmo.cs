using UnityEngine;

public static class HexGizmo
{
    public static void Draw(HexAuthor hexAuthor, Mesh hexMesh, Transform transform, bool isWire, bool isDrawingColors)
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Color originalColor = Gizmos.color;
        Gizmos.matrix = transform.localToWorldMatrix;

        foreach (HexCoord coord in HexCoord.AllWithinRings(hexAuthor.NumRings))
        {
            Vector3 pos = HexLayout.CoordsToWorld(coord);
            Gizmos.color = Color.white;
            if (isDrawingColors)
            {
                if (coord.Q == 0)
                    Gizmos.color = Color.green;
                if (coord.R == 0)
                    Gizmos.color = Color.blue;
                if (coord.S == 0)
                    Gizmos.color = Color.magenta;
                // Center
                if (coord.Q == 0 && coord.R == 0)
                    Gizmos.color = Color.yellowNice;
            }

            if (isWire)
                Gizmos.DrawWireMesh(hexMesh, pos);
            else
                Gizmos.DrawMesh(hexMesh, pos);
        }

        Gizmos.matrix = originalMatrix;
        Gizmos.color = originalColor;
    }
}
