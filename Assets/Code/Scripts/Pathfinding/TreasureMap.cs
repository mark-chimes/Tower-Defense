using System.Collections.Generic;

public class TreasureMap
{

    private Erf[,] map;
    public readonly int Width;
    public readonly int Height;

    public Coord SpawnPos { get; }
    public Coord GoalPos { get; }

    static readonly (int dx, int dz)[] Dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    public TreasureMap(int width, int height, Coord spawnPos, Coord goalPos) { 
        Width = width;
        Height = height;
        SpawnPos = spawnPos;
        GoalPos = goalPos;

        map = new Erf[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Coord coord = new Coord(x, z);
                Erf erf = new Erf(coord);
                
                if (coord == spawnPos)
                {
                    erf.Kind = ErfKind.Spawn;
                }
                else if (coord == goalPos)
                {
                    erf.Kind = ErfKind.Goal;
                }
                map[x, z] = erf;
            }
        }
    }

    public ErfSnapshot At(Coord coord) => At(coord.X, coord.Z);

    public ErfSnapshot At(int x, int z)
    {
        Erf data = map[x, z];
        return new ErfSnapshot(data.Coord, data.Kind, data.DistanceToGoal, data.HasWall);
    }

    public void SetWall(Coord c, bool hasWall) => map[c.X, c.Z].HasWall = hasWall;

    public bool CanPlaceWall(Coord c) => map[c.X, c.Z].Kind == ErfKind.Floor
        && !map[c.X, c.Z].HasWall;


    public void RecomputeDistances()
    {
        ClearDistances();

        bool[,] visited = new bool[Width, Height];
        Queue<Coord> cellQueue = new Queue<Coord>();

        Coord g = GoalPos;
        visited[g.X, g.Z] = true;
        Erf goalCell = map[g.X, g.Z];
        goalCell.DistanceToGoal = 0;
        cellQueue.Enqueue(g);

        while (cellQueue.TryDequeue(out var coord))
        {
            Erf topCell = map[coord.X, coord.Z];
            int dist = topCell.DistanceToGoal;

            foreach (var (dx, dz) in Dirs)
            {
                Coord c = coord.Shifted(dx, dz);
                if (!c.InBounds(Width, Height) || visited[c.X, c.Z])
                {
                    continue;
                }

                visited[c.X, c.Z] = true;
                Erf cell = map[c.X, c.Z];
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
                Erf cell = map[x, z];
                cell.DistanceToGoal = -1;
            }
        }
    }
}


