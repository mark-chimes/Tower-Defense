using System.Collections.Generic;

public class TreasureMap
{

    private Erf[,] map;
    public readonly int Width;
    public readonly int Height;
    public Coord SpawnPos { get; }
    public Coord GoalPos { get; }

    private FlowField currentFlow;

    public TreasureMap(int width, int height, Coord spawnPos, Coord goalPos)
    {
        map = new Erf[width, height];
        Width = width;
        Height = height;
        SpawnPos = spawnPos;
        GoalPos = goalPos;

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

        Recompute();
    }

    public void Recompute()
    {
        currentFlow = ComputeFlow();
    }

    public ErfSnapshot At(Coord coord)
    {
        Erf data = map[coord.X, coord.Z];
        return new ErfSnapshot(coord, data.Kind, currentFlow, data.HasWall);
    }

    public ErfSnapshot At(int x, int z)
    {
        Erf data = map[x, z];
        return new ErfSnapshot(data.Coord, data.Kind, currentFlow, data.HasWall);
    }

    public void SetWall(Coord c, bool hasWall) => map[c.X, c.Z].HasWall = hasWall;

    public bool CanPlaceWall(Coord c) => map[c.X, c.Z].Kind == ErfKind.Floor
        && !map[c.X, c.Z].HasWall;


    public FlowField ComputeFlow()
    {
        int[,] distanceToGoal = new int[Width, Height];
        Compass[,] dirToGoal = new Compass[Width, Height];
        bool[,] onCriticalPath = new bool[Width, Height];

        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                distanceToGoal[x, z] = -1;
                dirToGoal[x, z] = Compass.None;
                onCriticalPath[x, z] = false;
            }
        }

        BreadthFirstFromGoal( distanceToGoal, dirToGoal);

        MarkCriticalPath(dirToGoal, onCriticalPath);

        return new FlowField(distanceToGoal, dirToGoal, onCriticalPath);
    }

    private void BreadthFirstFromGoal(int[,] distanceToGoal, Compass[,] dirToGoal)
    {
        bool[,] visited = new bool[Width, Height];
        Queue<Coord> erfQueue = new Queue<Coord>();

        Coord g = GoalPos;
        visited[g.X, g.Z] = true;
        distanceToGoal[g.X, g.Z] = 0;
        erfQueue.Enqueue(g);

        while (erfQueue.TryDequeue(out var coord))
        {
            int dist = distanceToGoal[coord.X, coord.Z];

            foreach (Compass dir in CompassExtension.AllDirs)
            {
                Coord c = coord.InDirection(dir);
                if (!c.InBounds(Width, Height) || visited[c.X, c.Z])
                {
                    continue;
                }

                visited[c.X, c.Z] = true;
                Erf erf = map[c.X, c.Z];
                if (erf.HasWall)
                {
                    distanceToGoal[c.X, c.Z] = -1;
                    continue;
                }

                distanceToGoal[c.X, c.Z] = dist + 1;
                dirToGoal[c.X, c.Z] = dir.Opposite();
                erfQueue.Enqueue(c);
            }
        }
    }

    private void MarkCriticalPath(Compass[,] dirToGoal, bool[,] onCriticalPath)
    {
        Coord? c = SpawnPos;
        while (c != null && c != GoalPos)
        {
            Coord coord = (Coord)c;
            onCriticalPath[coord.X, coord.Z] = true;
            Compass dir = dirToGoal[coord.X, coord.Z];
            c = coord.InDirectionInBoundsNonSelf(dir, Width, Height);
        }
    }


}


