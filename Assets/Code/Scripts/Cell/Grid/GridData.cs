using System.Collections.Generic;

public class GridData
{

    private CellData[,] cells;
    public readonly int Width;
    public readonly int Height;

    public CellCoord SpawnPos { get; }
    public CellCoord GoalPos { get; }

    static readonly (int dx, int dz)[] Dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    public GridData(int width, int height, CellCoord spawnPos, CellCoord goalPos) { 
        Width = width;
        Height = height;
        SpawnPos = spawnPos;
        GoalPos = goalPos;

        cells = new CellData[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                CellCoord coord = new CellCoord(x, z);
                CellData cell = new CellData(coord);
                
                if (coord == spawnPos)
                {
                    cell.Kind = CellKind.Spawn;
                }
                else if (coord == goalPos)
                {
                    cell.Kind = CellKind.Goal;
                }
                cells[x, z] = cell;
            }
        }
    }

    public CellSnapshot At(CellCoord coord) => At(coord.X, coord.Z);

    public CellSnapshot At(int x, int z)
    {
        CellData data = cells[x, z];
        return new CellSnapshot(data.Coord, data.Kind, data.DistanceToGoal, data.HasWall);
    }

    public void SetWall(CellCoord c, bool hasWall) => cells[c.X, c.Z].HasWall = hasWall;

    public bool CanPlaceWall(CellCoord c) => cells[c.X, c.Z].Kind == CellKind.Floor
        && !cells[c.X, c.Z].HasWall;


    public void RecomputeDistances()
    {
        ClearDistances();

        bool[,] visited = new bool[Width, Height];
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
                if (!c.InBounds(Width, Height) || visited[c.X, c.Z])
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
        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                CellData cell = cells[x, z];
                cell.DistanceToGoal = -1;
            }
        }
    }
}


