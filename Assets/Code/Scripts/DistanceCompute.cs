using System.Collections.Generic;

public static class DistanceCompute
{

    static readonly (int dx, int dz)[] Dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    private static void ClearDistances(GridCell[,] cells,  int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridCell cell = cells[x, z];
                cell.DistanceToGoal = -1;
            }
        }
    }

    public static void RecomputeDistances(GridCell[,] cells, int width, int height, CellCoord goalPos)
    {
        ClearDistances(cells, width, height);
        
        bool[,] visited = new bool[width, height];
        Queue<CellCoord> cellQueue = new Queue<CellCoord>();

        CellCoord g = goalPos;
        visited[g.X, g.Z] = true;
        GridCell goalCell = cells[g.X, g.Z];
        goalCell.DistanceToGoal = 0;
        cellQueue.Enqueue(g);

        while (cellQueue.TryDequeue(out var coord))
        {
            GridCell topCell = cells[coord.X, coord.Z];
            int dist = topCell.DistanceToGoal;

            foreach (var (dx, dz) in Dirs)
            {
                CellCoord c = coord.Shifted(dx, dz);
                if (!c.InBounds(width, height) || visited[c.X, c.Z])
                {
                    continue;
                }

                visited[c.X, c.Z] = true;
                GridCell cell = cells[c.X, c.Z];
                if (cell.HasWall)
                {
                    cell.DistanceToGoal = -1;
                    continue;
                }

                cell.DistanceToGoal = dist + 1;
                cellQueue.Enqueue(c);
            }
        }

    }
}