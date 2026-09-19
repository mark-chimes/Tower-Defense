using UnityEngine;

public static class HexGizmo
{
    public static void Draw(HexAuthor gridAuthor, Transform transform)
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Color originalColor = Gizmos.color;

        HexLayout layout = gridAuthor.Layout;
        int width = layout.Width;
        int height = layout.Height;

        Gizmos.matrix = transform.localToWorldMatrix;
        Vector3 size = new Vector3(GridLayout.CellSize, 1f, GridLayout.CellSize);

        HexCoord spawnPos = gridAuthor.SpawnPos;
        HexCoord goalPos = gridAuthor.GoalPos;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Gizmos.color = Color.grey;
                HexCoord c = new HexCoord(x, z);
                if (c == spawnPos)
                {
                    Gizmos.color = Color.lightBlue;
                    Gizmos.DrawCube(layout.CoordsToWorld(c), size);
                }
                else if (c == goalPos)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawCube(layout.CoordsToWorld(c), size);
                }
                else
                {
                    Gizmos.DrawWireCube(layout.CoordsToWorld(c), size);
                }


            }
        }
        Gizmos.matrix = originalMatrix;
        Gizmos.color = originalColor;
    }
}
