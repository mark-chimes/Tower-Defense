using System.Collections.Generic;


public class Wayfinder
{
    public Coord SpawnPos { get; }
    public Coord GoalPos { get; }

    public readonly int Width;
    public readonly int Height;

    private readonly SearchDir SearchDirection;


    private FlowField currentFlow;

    private Phase phase = Phase.ExpandFrontier;

    private bool isStopOnPathFound = false;

    public enum Phase
    {
        ExpandFrontier,
        TracePath,
        Done,
    }

    public enum SearchDir
    {
        FromStart,
        FromEnd,
        Dual
    }

    bool[,] visited;
    Queue<Coord> erfQueue;
    Coord? pathC;


    public Wayfinder(int width, int height, Coord spawnPos, Coord goalPos, SearchDir searchDirection)
    {
        currentFlow = new FlowField(width, height);
        SpawnPos = spawnPos;
        GoalPos = goalPos;
        Width = width;
        Height = height;
        SearchDirection = searchDirection;
        isStopOnPathFound = true; // TODO pass as parameter
    }

    public Wayfinder WithNewSearchDir(SearchDir searchDirection)
    {
        return new Wayfinder(Width, Height, SpawnPos, GoalPos, searchDirection);
    }

    public void ClearField()
    {
        visited = new bool[Width, Height];
        erfQueue = new Queue<Coord>();
        currentFlow = new FlowField(Width, Height);
        phase = Phase.ExpandFrontier;

        Coord c;
        switch (SearchDirection)
        {
            case SearchDir.FromStart:
                {
                    c = SpawnPos;
                    pathC = GoalPos;
                    break;
                }
            case SearchDir.FromEnd:
                {
                    c = GoalPos;
                    pathC = SpawnPos;
                    break;
                }
            default:
                {
                    c = new Coord(0, 0); // TODO
                    break;
                }
        }

        visited[c.X, c.Z] = true;
        currentFlow.distance[c.X, c.Z] = 0;
        erfQueue.Enqueue(c);
    }

    public void ComputeFlow(Erf[,] map)
    {
        while (phase != Phase.Done)
        {
            ComputeSingleStep(map);
        }
    }


    public void ComputeSingleStep(Erf[,] map)
    {
        switch (phase)
        {
            case Phase.ExpandFrontier: BFSOneDirSingleStep(map); break;
            case Phase.TracePath: MarkCriticalPathSingleStep(); break;
            case Phase.Done: break;
        }
    }

    private void BFSOneDirSingleStep(Erf[,] map)
    {
        if (phase != Phase.ExpandFrontier) return;

        int[,] distances = currentFlow.distance;
        Compass[,] dirsToGoal = currentFlow.dirToGoal;

        if (erfQueue.TryDequeue(out var coord))
        {
            int dist = distances[coord.X, coord.Z];

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
                    distances[c.X, c.Z] = -1;
                    continue;
                }

                distances[c.X, c.Z] = dist + 1;
                switch (SearchDirection)
                {
                    case SearchDir.FromStart:
                        {
                            dirsToGoal[c.X, c.Z] = dir;
                            if (isStopOnPathFound && c == GoalPos)
                            {
                                phase = Phase.TracePath;
                                return;
                            }
                            break;
                        }
                    case SearchDir.FromEnd:
                        {
                            dirsToGoal[c.X, c.Z] = dir.Opposite();
                            if (isStopOnPathFound && c == SpawnPos)
                            {
                                phase = Phase.TracePath;
                                return;

                            }
                            break;
                        }
                    default: { phase = Phase.Done; return; } // TODO
                }
                erfQueue.Enqueue(c);
            }
        }
        else
        {
            phase = Phase.TracePath;
        }
    }

    private void MarkCriticalPathSingleStep()
    {
        if (phase != Phase.TracePath) { phase = Phase.Done; return; }

        bool[,] onCriticalPath = currentFlow.onCriticalPath;
        Compass[,] dirsToGoal = currentFlow.dirToGoal;

        if (SearchDirection == SearchDir.Dual) { phase = Phase.Done; return; } // TODO

        if (pathC == null) { phase = Phase.Done; return; }

        Coord coord = (Coord)pathC;
        onCriticalPath[coord.X, coord.Z] = true;
        Compass dir;
        switch (SearchDirection)
        {
            case SearchDir.FromStart: dir = dirsToGoal[coord.X, coord.Z].Opposite(); break;
            case SearchDir.FromEnd: dir = dirsToGoal[coord.X, coord.Z]; break;
            default: dir = Compass.None; break; // TODO
        }
        pathC = coord.InDirectionInBoundsNonSelf(dir, Width, Height);
    }

    public Signpost SignpostAt(Coord coord)
    {
        return new Signpost(coord, currentFlow);
    }
}
