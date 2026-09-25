using UnityEngine;

public static class HexGizmo
{
    public static void Draw(HexAuthor hexAuthor, Mesh hexMesh, Transform transform, bool isWire)
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Color originalColor = Gizmos.color;
        Gizmos.matrix = transform.localToWorldMatrix;
        
        HexCoord coord0 = new HexCoord(0,0);
        Vector3 pos0 = HexLayout.CoordsToWorld(coord0);

        HexCoord[] neighbors = coord0.Neighbours();



        // TODO draw num rings depending on hexAuthor

        if (isWire)
        {
            Gizmos.DrawWireMesh(hexMesh, pos0);
            foreach (HexCoord coord in HexCoord.AllWithinRings(hexAuthor.NumRings)) {
                Vector3 pos = HexLayout.CoordsToWorld(coord);
                Gizmos.DrawWireMesh(hexMesh, pos);
            }
        }
        else
        {
            Gizmos.DrawMesh(hexMesh, pos0);
            foreach (HexCoord coord in HexCoord.AllWithinRings(hexAuthor.NumRings)) {
                Vector3 pos = HexLayout.CoordsToWorld(coord);
                Gizmos.DrawMesh(hexMesh, pos);
            }
        }

        Gizmos.matrix = originalMatrix;
        Gizmos.color = originalColor;
    }
}
