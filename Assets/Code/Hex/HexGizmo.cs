using UnityEngine;

public static class HexGizmo
{
    public static void Draw(HexAuthor gridAuthor, Mesh hexMesh, Transform transform, bool isWire)
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Color originalColor = Gizmos.color;
        Gizmos.matrix = transform.localToWorldMatrix;

        HexLayout layout = gridAuthor.Layout;
        
        HexCoord coord1 = new HexCoord(0,0);
        Vector3 pos1 = HexLayout.CoordsToWorld(coord1);
        HexCoord coord2 = new HexCoord(1,0);
        Vector3 pos2 = HexLayout.CoordsToWorld(coord2);
        HexCoord coord3 = new HexCoord(0,1);
        Vector3 pos3 = HexLayout.CoordsToWorld(coord3);

        if (isWire)
        {
            Gizmos.DrawWireMesh(hexMesh, pos1);
            Gizmos.DrawWireMesh(hexMesh, pos2);
            Gizmos.DrawWireMesh(hexMesh, pos3);
        }
        else
        {
            Gizmos.DrawMesh(hexMesh, pos1);
            Gizmos.DrawMesh(hexMesh, pos2);
            Gizmos.DrawMesh(hexMesh, pos3);
        }

        Gizmos.matrix = originalMatrix;
        Gizmos.color = originalColor;
    }
}
