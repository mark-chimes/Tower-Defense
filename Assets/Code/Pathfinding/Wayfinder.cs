using System.Collections.Generic;

public class Wayfinder
{
    public readonly HexCoord SpawnCoord;
    public readonly HexCoord GoalCoord;

    private Lattice<bool> wallMap; // Do not modify

    public int NumRings => wallMap.NumRings;

    private readonly HexSearch.Dir SearchDirection;


    private FlowField field;

    private HexSearch.Phase phase = HexSearch.Phase.ExpandFrontier;

    private bool isStopOnPathFound = false;



    Lattice<bool> visited;
    Queue<HexCoord> frontier;
    HexCoord? pathC;


    public Wayfinder(Lattice<bool> wallMap, HexCoord spawnCoord, HexCoord goalCoord, HexSearch.Dir searchDirection, bool isStopOnPathFound)
    {
        this.wallMap = wallMap;
        field = new FlowField(NumRings);
        SpawnCoord = spawnCoord;
        GoalCoord = goalCoord;
        SearchDirection = searchDirection;
        this.isStopOnPathFound = isStopOnPathFound;
        ClearField();
    }

    public Wayfinder WithNewSearchDir(HexSearch.Dir searchDirection)
    {
        return new Wayfinder(wallMap, SpawnCoord, GoalCoord, searchDirection, isStopOnPathFound);
    }

    public void ClearField()
    {
        visited = new Lattice<bool>(NumRings);
        frontier = new Queue<HexCoord>();
        field = new FlowField(NumRings);
        phase = HexSearch.Phase.ExpandFrontier;

        HexCoord c;
        switch (SearchDirection)
        {
            case HexSearch.Dir.FromStart:
                {
                    c = SpawnCoord;
                    pathC = GoalCoord;
                    break;
                }
            case HexSearch.Dir.FromEnd:
                {
                    c = GoalCoord;
                    pathC = SpawnCoord;
                    break;
                }
            default:
                {
                    c = new HexCoord(0, 0); // TODO
                    break;
                }
        }

        visited.SetAt(c, true);
        field.SetTile(c, new FlowField.Tile(0, HexCompass.NONE, false));
        frontier.Enqueue(c);
    }

    public IReadOnlyCollection<HexCoord> CurrentFrontier()
    {
        return frontier.ToArray();
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
        IReadOnlyCollection<FlowSample> changed = phase switch
        {
            HexSearch.Phase.ExpandFrontier => BFSOneDirSingleStep(),
            HexSearch.Phase.TracePath => MarkCriticalPathSingleStep(),
            _ => System.Array.Empty<FlowSample>(),
        };
        return new HexSearch.Delta(phaseAtStart, changed);
    }


    private IReadOnlyCollection<FlowSample> BFSOneDirSingleStep()
    {
        var list = new List<FlowSample>();

        if (phase != HexSearch.Phase.ExpandFrontier) return list;

        if (frontier.TryDequeue(out var coord))
        {
            int prevDist = field.TileAt(coord).Distance;

            foreach (HexCompass dir in HexCompassExtension.AllDirs)
            {
                HexCoord c = coord.InDirection(dir);
                if (!visited.Contains(c) || visited.At(c))
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
                            target = GoalCoord;
                            break;
                        }
                    case HexSearch.Dir.FromEnd:
                        {
                            newDir = dir.Opposite();
                            target = SpawnCoord;
                            break;
                        }
                    default: { phase = HexSearch.Phase.Done; return list; } // TODO
                }

                FlowField.Tile newTile = new FlowField.Tile(prevDist + 1, newDir, false);
                field.SetTile(c, newTile);
                list.Add(new FlowSample(c, newTile));

                if (isStopOnPathFound && c == target)
                {
                    phase = HexSearch.Phase.TracePath;
                    return list;
                }

                frontier.Enqueue(c);
            }
        }
        else
        {
            phase = HexSearch.Phase.TracePath;
        }
        return list;
    }

    private IReadOnlyCollection<FlowSample> MarkCriticalPathSingleStep()
    {
        IReadOnlyCollection<FlowSample> empty = System.Array.Empty<FlowSample>();
        if (phase != HexSearch.Phase.TracePath) { phase = HexSearch.Phase.Done; return empty; }

        if (SearchDirection == HexSearch.Dir.Dual) { phase = HexSearch.Phase.Done; return empty; } // TODO

        if (pathC == null) { phase = HexSearch.Phase.Done; return empty; }

        HexCoord coord = (HexCoord)pathC;
        FlowField.Tile oldTile = field.TileAt(coord);
        FlowField.Tile newTile = new FlowField.Tile(oldTile.Distance, oldTile.DirToGoal, true);
        field.SetTile(coord, newTile);

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
            pathC = wallMap.Contains(next) ? next : null;
        }
        return new[] { new FlowSample(coord, newTile) };
    }

    public FlowSample FlowAt(HexCoord coord)
    {
        return new FlowSample(coord, field.TileAt(coord));
    }

    public IReadOnlyCollection<FlowSample> Flows()
    {
        return field.Flows();
    }

}
