using System.Collections.Generic;
using Unity.VisualScripting;

public class TreasureMap
{

    private Erf[,] map;
    public readonly int Width;
    public readonly int Height;
    public Coord SpawnPos { get; }
    public Coord GoalPos { get; }

    private FlowField currentFlow;
    public bool isDistanceCalcComplete = false;

    bool[,] visited;
    Queue<Coord> erfQueue;
    Coord? pathC;



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

    public void ClearField()
    {
        visited = new bool[Width, Height];
        erfQueue = new Queue<Coord>();
        currentFlow = new FlowField(Width, Height);
        isDistanceCalcComplete = false;

        Coord g = GoalPos;
        visited[g.X, g.Z] = true;
        currentFlow.distanceToGoal[g.X, g.Z] = 0;
        erfQueue.Enqueue(g);
        pathC = SpawnPos;
    }

    public void SingleStep()
    {
        ComputeSingleStep();
    }

    public void Recompute()
    {
        ClearField();
        ComputeFlow();
    }

    public ErfSnapshot At(Coord coord)
    {
        Erf data = map[coord.X, coord.Z];
        return new ErfSnapshot(coord, data.Kind, currentFlow, data.HasWall);
    }

    public ErfSnapshot At(int x, int z) => At(new Coord(x, z));

    public void SetWall(Coord c, bool hasWall) => map[c.X, c.Z].HasWall = hasWall;

    public bool CanPlaceWall(Coord c) => map[c.X, c.Z].Kind == ErfKind.Floor
        && !map[c.X, c.Z].HasWall;


    private void ComputeSingleStep()
    {
        if (!isDistanceCalcComplete)
        {
            BFSSingleStep(currentFlow.distanceToGoal, currentFlow.dirToGoal);
        }
        else
        {
            MarkCriticalPathSingleStep(currentFlow.dirToGoal, currentFlow.onCriticalPath);
        }
    }

    private void ComputeFlow()
    {
        BreadthFirstFromGoal(currentFlow.distanceToGoal, currentFlow.dirToGoal);
        MarkCriticalPath(currentFlow.dirToGoal, currentFlow.onCriticalPath);
    }

    private void BFSSingleStep(int[,] distanceToGoal, Compass[,] dirToGoal)
    {
        if (isDistanceCalcComplete) return;

        if (erfQueue.TryDequeue(out var coord))
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
        } else { 
            isDistanceCalcComplete = true;
        }
    }

    private void MarkCriticalPathSingleStep(Compass[,] dirToGoal, bool[,] onCriticalPath)
    {
        if (!isDistanceCalcComplete) return;
        if (pathC != null && pathC != GoalPos)
        {
            Coord coord = (Coord)pathC;
            onCriticalPath[coord.X, coord.Z] = true;
            Compass dir = dirToGoal[coord.X, coord.Z];
            pathC = coord.InDirectionInBoundsNonSelf(dir, Width, Height);
        }
    }

    // Breadth-first search
    // Cannot use when multiple tile-costs are involved
    private void BreadthFirstFromGoal(int[,] distanceToGoal, Compass[,] dirToGoal)
    {
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
        isDistanceCalcComplete = true;
    }

    private void MarkCriticalPath(Compass[,] dirToGoal, bool[,] onCriticalPath)
    {
        while (pathC != null && pathC != GoalPos)
        {
            Coord coord = (Coord)pathC;
            onCriticalPath[coord.X, coord.Z] = true;
            Compass dir = dirToGoal[coord.X, coord.Z];
            pathC = coord.InDirectionInBoundsNonSelf(dir, Width, Height);
        }
    }

}


