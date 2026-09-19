using UnityEngine;

public static class HexGizmo
{
    public static void Draw(HexAuthor hexAuthor, Mesh hexMesh, Transform transform, bool isWire)
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Color originalColor = Gizmos.color;
        Gizmos.matrix = transform.localToWorldMatrix;

        HexLayout layout = hexAuthor.Layout;
        
        HexCoord coord0 = new HexCoord(0,0);
        Vector3 pos0 = layout.CoordsToWorld(coord0);

        HexCoord[] neighbors = coord0.Neighbours();

        if (isWire)
        {
            Gizmos.DrawWireMesh(hexMesh, pos0);
            foreach (HexCoord neighbor in neighbors) {
                Vector3 pos = layout.CoordsToWorld(neighbor);
                Gizmos.DrawWireMesh(hexMesh, pos);
            }
        }
        else
        {
            Gizmos.DrawMesh(hexMesh, pos0);
            foreach (HexCoord neighbor in neighbors) {
                Vector3 pos = layout.CoordsToWorld(neighbor);
                Gizmos.DrawMesh(hexMesh, pos);
            }
        }

        Gizmos.matrix = originalMatrix;
        Gizmos.color = originalColor;
    }
}
