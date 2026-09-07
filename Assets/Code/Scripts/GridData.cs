using System.Collections.Generic;

public class GridData
{

    private CellData[,] cells;
    public readonly int width;
    public readonly int height;

    private CellCoord GoalPos;

    static readonly (int dx, int dz)[] Dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    public GridData(CellData[,] cells, CellCoord goalPos)
    {
        this.cells = cells;
        GoalPos = goalPos;
        width = this.cells.GetLength(0);
        height = this.cells.GetLength(1);
    }

    public CellSnapshot At(CellCoord coord) => At(coord.X, coord.Z);

    public CellSnapshot At(int x, int z)
    {
        CellData data = cells[x, z];
        return CellSnapshot.FromData(data);
    }


    public void SetWall(CellCoord c, bool hasWall) => cells[c.X, c.Z].HasWall = hasWall;

    public bool CanPlaceWall(CellCoord c) => cells[c.X, c.Z].Kind == CellKind.Floor
        && !cells[c.X, c.Z].HasWall;


    public void RecomputeDistances()
    {
        ClearDistances();

        bool[,] visited = new bool[width, height];
        Queue<CellCoord> cellQueue = new Queue<CellCoord>();

        CellCoord g = GoalPos;
        visited[g.X, g.Z] = true;
        CellData goalCell = cells[g.X, g.Z];
        goalCell.DistanceToGoal = 0;
        cellQueue.Enqueue(g);

        while (cellQueue.TryDequeue(out var coord))
        {
            CellData topCell = cells[coord.X, coord.Z];
            int dist = topCell.DistanceToGoal;

            foreach (var (dx, dz) in Dirs)
            {
                CellCoord c = coord.Shifted(dx, dz);
                if (!c.InBounds(width, height) || visited[c.X, c.Z])
                {
                    continue;
                }

                visited[c.X, c.Z] = true;
                CellData cell = cells[c.X, c.Z];
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
                CellData cell = cells[x, z];
                cell.DistanceToGoal = -1;
            }
        }
    }
}


