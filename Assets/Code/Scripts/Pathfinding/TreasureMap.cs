using System.Collections.Generic;

public class TreasureMap
{

    private Erf[,] map;
    private int[,] distanceToGoal;
    private Cardinal[,] cameFrom;


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
        distanceToGoal = new int[width, height];
        cameFrom = new Cardinal[width, height];

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
        return new ErfSnapshot(data.Coord, data.Kind, distanceToGoal[x,z], cameFrom[x,z], data.HasWall);
    }

    public int DistanceToGoal(int x, int z)
    {
        return distanceToGoal[x,z];
    }

    public Cardinal CameFrom(int x, int z) 
    {
        return cameFrom[x,z];
    }

    public void SetWall(Coord c, bool hasWall) => map[c.X, c.Z].HasWall = hasWall;

    public bool CanPlaceWall(Coord c) => map[c.X, c.Z].Kind == ErfKind.Floor
        && !map[c.X, c.Z].HasWall;


    public void RecomputeDistances()
    {
        ClearMarkings();

        bool[,] visited = new bool[Width, Height];
        Queue<Coord> erfQueue = new Queue<Coord>();

        Coord g = GoalPos;
        visited[g.X, g.Z] = true;
        distanceToGoal[g.X, g.Z] = 0;
        erfQueue.Enqueue(g);

        while (erfQueue.TryDequeue(out var coord))
        {
            int dist = distanceToGoal[coord.X, coord.Z];

            foreach (var (dx, dz) in Dirs)
            {
                Coord c = coord.Shifted(dx, dz);
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
                cameFrom[c.X, c.Z] = c.DirTo(coord);
                erfQueue.Enqueue(c);
            }
        }

    }

    private void ClearMarkings()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                distanceToGoal[x, z]= -1;
                cameFrom[x,z] = Cardinal.None;
            }
        }
    }
}


