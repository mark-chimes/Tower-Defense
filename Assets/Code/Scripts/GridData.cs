using System.Collections.Generic;

public class GridData 
{

    private GridCell[,] Cells;
    public readonly int width;
    public readonly int height;

    private CellCoord goalPos;


    public GridData(GridCell[,] cells, CellCoord goalPos) { 
        Cells = cells;
        width = Cells.GetLength(0);
        height = Cells.GetLength(1);
    }

    public GridCell At(CellCoord coord) => At(coord.X, coord.Z);
    public GridCell At(int x, int z) => Cells[x, z];

    public bool CanPlaceWall(CellCoord c) => Cells[c.X, c.Z].Kind == CellKind.Floor
        && !Cells[c.X, c.Z].HasWall;

    static readonly (int dx, int dz)[] Dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    public void RecomputeDistances()
    {
        ClearDistances();
        
        bool[,] visited = new bool[width, height];
        Queue<CellCoord> cellQueue = new Queue<CellCoord>();

        CellCoord g = goalPos;
        visited[g.X, g.Z] = true;
        GridCell goalCell = Cells[g.X, g.Z];
        goalCell.DistanceToGoal = 0;
        cellQueue.Enqueue(g);

        while (cellQueue.TryDequeue(out var coord))
        {
            GridCell topCell = Cells[coord.X, coord.Z];
            int dist = topCell.DistanceToGoal;

            foreach (var (dx, dz) in Dirs)
            {
                CellCoord c = coord.Shifted(dx, dz);
                if (!c.InBounds(width, height) || visited[c.X, c.Z])
                {
                    continue;
                }

                visited[c.X, c.Z] = true;
                GridCell cell = Cells[c.X, c.Z];
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

    private void ClearDistances()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridCell cell = Cells[x, z];
                cell.DistanceToGoal = -1;
            }
        }
    }
}


