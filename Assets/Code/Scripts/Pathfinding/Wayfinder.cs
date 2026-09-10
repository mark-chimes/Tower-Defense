using System.Collections.Generic;
using System.Linq;


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

    readonly struct PathfindingDelta
    {
        readonly Phase phase;
        readonly IReadOnlyList<(Coord, Signpost)> Changed;


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

    // TODO return PathfindingDelta
    public void ComputeSingleStep(Erf[,] map)
    {
        switch (phase)
        {
            case Phase.ExpandFrontier:
                {
                    BFSOneDirSingleStep(map);
                    // TODO this part won't work because phase 
                    // gets changed before this return

                    return; // TODO return PathfindingDelta
                }
            case Phase.TracePath:
                {
                    MarkCriticalPathSingleStep();
                    return; // TODO return PathfindingDelta
                }
            case Phase.Done: return;
        }
    }

    private List<Signpost> BFSOneDirSingleStep(Erf[,] map)
    {
        var list = new List<Signpost>();

        if (phase != Phase.ExpandFrontier) return list;

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
                            list.Append(new Signpost(c, currentFlow));

                            if (isStopOnPathFound && c == GoalPos)
                            {
                                // TODO this part won't work
                                phase = Phase.TracePath;
                                return list;
                            }
                            break;
                        }
                    case SearchDir.FromEnd:
                        {
                            dirsToGoal[c.X, c.Z] = dir.Opposite();
                            list.Append(new Signpost(c, currentFlow));

                            if (isStopOnPathFound && c == SpawnPos)
                            {
                                // TODO this part won't work
                                phase = Phase.TracePath;
                                return list;
                            }
                            break;
                        }
                    default: { phase = Phase.Done; return list; } // TODO
                }
                erfQueue.Enqueue(c);
            }
        }
        else
        {
            // TODO phase change here, too
            phase = Phase.TracePath;
        }
        return list;
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
