using System.Collections.Generic;

public class Wayfinder
{
    public readonly Coord SpawnPos;
    public readonly Coord GoalPos;

    private RectMap<bool> wallMap; // Do not modify

    public int Width => wallMap.Width;
    public int Height => wallMap.Height;

    private readonly Search.Dir SearchDirection;


    private FlowField currentFlow;

    private Search.Phase phase = Search.Phase.ExpandFrontier;

    private bool isStopOnPathFound = false;



    RectMap<bool> visited;
    Queue<Coord> erfQueue;
    Coord? pathC;


    public Wayfinder(RectMap<bool> wallMap, Coord spawnPos, Coord goalPos, Search.Dir searchDirection, bool isStopOnPathFound)
    {
        this.wallMap = wallMap;
        currentFlow = new FlowField(Width, Height);
        SpawnPos = spawnPos;
        GoalPos = goalPos;
        SearchDirection = searchDirection;
        this.isStopOnPathFound = isStopOnPathFound;
        ClearField();
    }

    public Wayfinder WithNewSearchDir(Search.Dir searchDirection)
    {
        return new Wayfinder(wallMap, SpawnPos, GoalPos, searchDirection, isStopOnPathFound);
    }

    public void ClearField()
    {
        visited = new RectMap<bool>(Width, Height);
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

        visited.SetAt(c, true);
        currentFlow.SetTile(c, new FlowField.Tile(0, Compass.None, false));
        erfQueue.Enqueue(c);
    }

    public IReadOnlyCollection<Coord> CurrentFrontier()
    {
        return erfQueue.ToArray();
    }

    public void ComputeFlow()
    {
        while (phase != Search.Phase.Done)
        {
            ComputeSingleStep();
        }
    }

    public Search.Delta ComputeSingleStep()
    {
        Search.Phase phaseAtStart = phase;
        IReadOnlyCollection<Signpost> changed = phase switch
        {
            Search.Phase.ExpandFrontier => BFSOneDirSingleStep(),
            Search.Phase.TracePath => MarkCriticalPathSingleStep(),
            _ => System.Array.Empty<Signpost>(),
        };
        return new Search.Delta(phaseAtStart, changed);
    }


    private IReadOnlyCollection<Signpost> BFSOneDirSingleStep()
    {
        var list = new List<Signpost>();

        if (phase != Search.Phase.ExpandFrontier) return list;

        if (erfQueue.TryDequeue(out var coord))
        {
            int prevDist = currentFlow.TileAt(coord).Distance;

            foreach (Compass dir in CompassExtension.AllDirs)
            {
                Coord c = coord.InDirection(dir);
                if (!c.InBounds(Width, Height) || visited.At(c))
                {
                    continue;
                }

                visited.SetAt(c, true);
                if (wallMap.At(c))
                {
                    continue;
                }

                Compass newDir;
                Coord target;
                switch (SearchDirection)
                {
                    case Search.Dir.FromStart:
                        {
                            newDir = dir;
                            target = GoalPos;
                            break;
                        }
                    case Search.Dir.FromEnd:
                        {
                            newDir = dir.Opposite();
                            target = SpawnPos;
                            break;
                        }
                    default: { phase = Search.Phase.Done; return list; } // TODO
                }

                FlowField.Tile newTile = new FlowField.Tile(prevDist + 1, newDir, false);
                currentFlow.SetTile(c, newTile);
                list.Add(new Signpost(c, newTile));

                if (isStopOnPathFound && c == target)
                {
                    phase = Search.Phase.TracePath;
                    return list;
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

        if (SearchDirection == Search.Dir.Dual) { phase = Search.Phase.Done; return empty; } // TODO

        if (pathC == null) { phase = Search.Phase.Done; return empty; }

        Coord coord = (Coord)pathC;
        FlowField.Tile oldTile = currentFlow.TileAt(coord);
        FlowField.Tile newTile = new FlowField.Tile(oldTile.Distance, oldTile.DirToGoal, true);
        currentFlow.SetTile(coord, newTile);

        Compass dir;
        switch (SearchDirection)
        {
            case Search.Dir.FromStart: dir = oldTile.DirToGoal.Opposite(); break;
            case Search.Dir.FromEnd: dir = oldTile.DirToGoal; break;
            default: dir = Compass.None; break; // TODO
        }
        pathC = coord.InDirectionInBoundsNonSelf(dir, Width, Height);
        return new[] { new Signpost(coord, newTile) };
    }

    public Signpost SignpostAt(Coord coord)
    {
        return new Signpost(coord, currentFlow.TileAt(coord));
    }

    public IReadOnlyCollection<Signpost> Signposts()
    {
        return currentFlow.Signposts();
    }

}
