using System.Collections.Generic;

public class Wayfinder
{
    public Coord SpawnPos { get; }
    public Coord GoalPos { get; }

    public readonly int Width;
    public readonly int Height;

    private readonly Search.Dir SearchDirection;


    private FlowField currentFlow;

    private Search.Phase phase = Search.Phase.ExpandFrontier;

    // TODO should pass this in as a parameter
    private bool isStopOnPathFound = false;



    bool[,] visited;
    Queue<Coord> erfQueue;
    Coord? pathC;


    public Wayfinder(int width, int height, Coord spawnPos, Coord goalPos, Search.Dir searchDirection, bool isStopOnPathFound)
    {
        currentFlow = new FlowField(width, height);
        SpawnPos = spawnPos;
        GoalPos = goalPos;
        Width = width;
        Height = height;
        SearchDirection = searchDirection;
        this.isStopOnPathFound = isStopOnPathFound;
    }

    public Wayfinder WithNewSearchDir(Search.Dir searchDirection)
    {
        return new Wayfinder(Width, Height, SpawnPos, GoalPos, searchDirection, isStopOnPathFound);
    }

    public void ClearField()
    {
        visited = new bool[Width, Height];
        erfQueue = new Queue<Coord>();
        currentFlow = new FlowField(Width, Height);
        phase = Search.Phase.ExpandFrontier;

        Coord c;
        switch (SearchDirection)
        {
            case Search.Dir.FromStart:
                {
                    c = SpawnPos;
                    pathC = GoalPos;
                    break;
                }
            case Search.Dir.FromEnd:
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

    public IReadOnlyCollection<Coord> CurrentFrontier()
    {
        return erfQueue.ToArray();
    }

    public void ComputeFlow(Erf[,] map)
    {
        while (phase != Search.Phase.Done)
        {
            ComputeSingleStep(map);
        }
    }

    public Search.Delta ComputeSingleStep(Erf[,] map)
    {
        Search.Phase phaseAtStart = phase;
        IReadOnlyCollection<Signpost> changed = phase switch
        {
            Search.Phase.ExpandFrontier => BFSOneDirSingleStep(map),
            Search.Phase.TracePath => MarkCriticalPathSingleStep(),
            _ => System.Array.Empty<Signpost>(),
        };
        return new Search.Delta(phaseAtStart, changed);
    }


    private IReadOnlyCollection<Signpost> BFSOneDirSingleStep(Erf[,] map)
    {
        var list = new List<Signpost>();

        if (phase != Search.Phase.ExpandFrontier) return list;

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
                    case Search.Dir.FromStart:
                        {
                            dirsToGoal[c.X, c.Z] = dir;
                            list.Add(new Signpost(c, currentFlow));

                            if (isStopOnPathFound && c == GoalPos)
                            {
                                phase = Search.Phase.TracePath;
                                return list;
                            }
                            break;
                        }
                    case Search.Dir.FromEnd:
                        {
                            dirsToGoal[c.X, c.Z] = dir.Opposite();
                            list.Add(new Signpost(c, currentFlow));

                            if (isStopOnPathFound && c == SpawnPos)
                            {
                                phase = Search.Phase.TracePath;
                                return list;
                            }
                            break;
                        }
                    default: { phase = Search.Phase.Done; return list; } // TODO
                }
                erfQueue.Enqueue(c);
            }
        }
        else
        {
            phase = Search.Phase.TracePath;
        }
        return list;
    }

    private IReadOnlyCollection<Signpost> MarkCriticalPathSingleStep()
    {
        IReadOnlyCollection<Signpost> empty = System.Array.Empty<Signpost>();
        if (phase != Search.Phase.TracePath) { phase = Search.Phase.Done; return empty; }

        bool[,] onCriticalPath = currentFlow.onCriticalPath;
        Compass[,] dirsToGoal = currentFlow.dirToGoal;

        if (SearchDirection == Search.Dir.Dual) { phase = Search.Phase.Done; return empty; } // TODO

        if (pathC == null) { phase = Search.Phase.Done; return empty; }

        Coord coord = (Coord)pathC;
        onCriticalPath[coord.X, coord.Z] = true;
        Compass dir;
        switch (SearchDirection)
        {
            case Search.Dir.FromStart: dir = dirsToGoal[coord.X, coord.Z].Opposite(); break;
            case Search.Dir.FromEnd: dir = dirsToGoal[coord.X, coord.Z]; break;
            default: dir = Compass.None; break; // TODO
        }
        pathC = coord.InDirectionInBoundsNonSelf(dir, Width, Height);
        return new[] { new Signpost(coord, currentFlow) };
    }

    public Signpost SignpostAt(Coord coord)
    {
        return new Signpost(coord, currentFlow);
    }

    public IReadOnlyCollection<Signpost> Signposts()
    {
        return currentFlow.Signposts();
    }

}
