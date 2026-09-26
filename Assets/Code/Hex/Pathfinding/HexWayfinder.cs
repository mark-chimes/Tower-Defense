using System.Collections.Generic;

public class HexWayfinder
{
    public readonly HexCoord SpawnPos;
    public readonly HexCoord GoalPos;

    private HexMap<bool> wallMap; // Do not modify

    public int NumRings => wallMap.NumRings;

    private readonly HexSearch.Dir SearchDirection;


    private HexFlowField currentFlow;

    private HexSearch.Phase phase = HexSearch.Phase.ExpandFrontier;

    private bool isStopOnPathFound = false;



    HexMap<bool> visited;
    Queue<HexCoord> erfQueue;
    HexCoord? pathC;


    public HexWayfinder(HexMap<bool> wallMap, HexCoord spawnPos, HexCoord goalPos, HexSearch.Dir searchDirection, bool isStopOnPathFound)
    {
        this.wallMap = wallMap;
        currentFlow = new HexFlowField(NumRings);
        SpawnPos = spawnPos;
        GoalPos = goalPos;
        SearchDirection = searchDirection;
        this.isStopOnPathFound = isStopOnPathFound;
        ClearField();
    }

    public HexWayfinder WithNewSearchDir(HexSearch.Dir searchDirection)
    {
        return new HexWayfinder(wallMap, SpawnPos, GoalPos, searchDirection, isStopOnPathFound);
    }

    public void ClearField()
    {
        visited = new HexMap<bool>(NumRings);
        erfQueue = new Queue<HexCoord>();
        currentFlow = new HexFlowField(NumRings);
        phase = HexSearch.Phase.ExpandFrontier;

        HexCoord c;
        switch (SearchDirection)
        {
            case HexSearch.Dir.FromStart:
                {
                    c = SpawnPos;
                    pathC = GoalPos;
                    break;
                }
            case HexSearch.Dir.FromEnd:
                {
                    c = GoalPos;
                    pathC = SpawnPos;
                    break;
                }
            default:
                {
                    c = new HexCoord(0, 0); // TODO
                    break;
                }
        }

        visited.SetAt(c, true);
        currentFlow.SetTile(c, new HexFlowField.Tile(0, HexCompass.NONE, false));
        erfQueue.Enqueue(c);
    }

    public IReadOnlyCollection<HexCoord> CurrentFrontier()
    {
        return erfQueue.ToArray();
    }

    public void ComputeFlow()
    {
        while (phase != HexSearch.Phase.Done)
        {
            ComputeSingleStep();
        }
    }

    public HexSearch.Delta ComputeSingleStep()
    {
        HexSearch.Phase phaseAtStart = phase;
        IReadOnlyCollection<HexSignpost> changed = phase switch
        {
            HexSearch.Phase.ExpandFrontier => BFSOneDirSingleStep(),
            HexSearch.Phase.TracePath => MarkCriticalPathSingleStep(),
            _ => System.Array.Empty<HexSignpost>(),
        };
        return new HexSearch.Delta(phaseAtStart, changed);
    }


    private IReadOnlyCollection<HexSignpost> BFSOneDirSingleStep()
    {
        var list = new List<HexSignpost>();

        if (phase != HexSearch.Phase.ExpandFrontier) return list;

        if (erfQueue.TryDequeue(out var coord))
        {
            int prevDist = currentFlow.TileAt(coord).Distance;

            foreach (HexCompass dir in HexCompassExtension.AllDirs)
            {
                HexCoord c = coord.InDirection(dir);
                if (!visited.IsCoordOnMap(c) || visited.At(c))
                {
                    continue;
                }

                visited.SetAt(c, true);
                if (wallMap.At(c))
                {
                    continue;
                }

                HexCompass newDir;
                HexCoord target;
                switch (SearchDirection)
                {
                    case HexSearch.Dir.FromStart:
                        {
                            newDir = dir;
                            target = GoalPos;
                            break;
                        }
                    case HexSearch.Dir.FromEnd:
                        {
                            newDir = dir.Opposite();
                            target = SpawnPos;
                            break;
                        }
                    default: { phase = HexSearch.Phase.Done; return list; } // TODO
                }

                HexFlowField.Tile newTile = new HexFlowField.Tile(prevDist + 1, newDir, false);
                currentFlow.SetTile(c, newTile);
                list.Add(new HexSignpost(c, newTile));

                if (isStopOnPathFound && c == target)
                {
                    phase = HexSearch.Phase.TracePath;
                    return list;
                }

                erfQueue.Enqueue(c);
            }
        }
        else
        {
            phase = HexSearch.Phase.TracePath;
        }
        return list;
    }

    private IReadOnlyCollection<HexSignpost> MarkCriticalPathSingleStep()
    {
        IReadOnlyCollection<HexSignpost> empty = System.Array.Empty<HexSignpost>();
        if (phase != HexSearch.Phase.TracePath) { phase = HexSearch.Phase.Done; return empty; }

        if (SearchDirection == HexSearch.Dir.Dual) { phase = HexSearch.Phase.Done; return empty; } // TODO

        if (pathC == null) { phase = HexSearch.Phase.Done; return empty; }

        HexCoord coord = (HexCoord)pathC;
        HexFlowField.Tile oldTile = currentFlow.TileAt(coord);
        HexFlowField.Tile newTile = new HexFlowField.Tile(oldTile.Distance, oldTile.DirToGoal, true);
        currentFlow.SetTile(coord, newTile);

        HexCompass dir;
        switch (SearchDirection)
        {
            case HexSearch.Dir.FromStart: dir = oldTile.DirToGoal.Opposite(); break;
            case HexSearch.Dir.FromEnd: dir = oldTile.DirToGoal; break;
            default: dir = HexCompass.NONE; break; // TODO
        }
        if (dir == HexCompass.NONE)
        {
            pathC = null;
        }
        else
        {
            HexCoord next = coord.InDirection(dir);
            pathC = wallMap.IsCoordOnMap(next) ? next : null;
        }
        return new[] { new HexSignpost(coord, newTile) };
    }

    public HexSignpost SignpostAt(HexCoord coord)
    {
        return new HexSignpost(coord, currentFlow.TileAt(coord));
    }

    public IReadOnlyCollection<HexSignpost> Signposts()
    {
        return currentFlow.Signposts();
    }

}
