using System.Collections.Generic;


public class Wayfinder
{
    public Coord SpawnPos { get; }
    public Coord GoalPos { get; }

    public readonly int Width;
    public readonly int Height;

    private SearchDir SearchDirection;


    private FlowField currentFlow;


    // TODO State Machine (enum?)
    private SearchState searchState = SearchState.DistanceCalc;

    private bool isStopOnPathFound = false;

    private enum SearchState
    {
        DistanceCalc,
        PathCalc,
        Complete,
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

    // TODO not sure if wayfinder should clear, or if we should actually spawn
    // A brand new wayfinder from the old one - see below - or something else.
    public void SetSearchDirAndClear(SearchDir searchDirection) {
        SearchDirection = searchDirection;
        ClearField();
    }

    public Wayfinder WithNewSearchDir( SearchDir searchDirection) { 
        return new Wayfinder(Width, Height, SpawnPos, GoalPos, searchDirection);
    }

    public void ClearField()
    {
        visited = new bool[Width, Height];
        erfQueue = new Queue<Coord>();
        currentFlow = new FlowField(Width, Height);
        searchState = SearchState.DistanceCalc;

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
        while (searchState != SearchState.Complete)
        {
            ComputeSingleStep(map);
        }
    }


    public void ComputeSingleStep(Erf[,] map)
    {
        switch (searchState)
        {
            case SearchState.DistanceCalc: BFSSingleStep(map); break;
            case SearchState.PathCalc: MarkCriticalPathSingleStep(); break;
            case SearchState.Complete: break;
        }
    }

    private void BFSSingleStep(Erf[,] map)
    {
        switch (SearchDirection)
        {
            case SearchDir.FromStart: BFSOneDirSingleStep(map); break;
            case SearchDir.FromEnd: BFSOneDirSingleStep(map); break;
            default: return; // TODO
        }
    }

    private void BFSOneDirSingleStep(Erf[,] map)
    {
        if (searchState != SearchState.DistanceCalc) return;

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
                                searchState = SearchState.PathCalc;
                                return;
                            }
                            break;
                        }
                    case SearchDir.FromEnd:
                        {
                            dirsToGoal[c.X, c.Z] = dir.Opposite();
                            if (isStopOnPathFound && c == SpawnPos)
                            {
                                searchState = SearchState.PathCalc;
                                return;

                            }
                            break;
                        }
                    default: return; // TODO
                }
                erfQueue.Enqueue(c);
            }
        }
        else
        {
            searchState = SearchState.PathCalc;
        }
    }

    private void MarkCriticalPathSingleStep()
    {
        if (searchState != SearchState.PathCalc) return;

        bool[,] onCriticalPath = currentFlow.onCriticalPath;
        Compass[,] dirsToGoal = currentFlow.dirToGoal;

        if (SearchDirection == SearchDir.Dual) { searchState = SearchState.Complete; return; } // TODO

        if (pathC == null) { searchState = SearchState.Complete; return; }

        Coord coord = (Coord)pathC;
        onCriticalPath[coord.X, coord.Z] = true;
        Compass dir = dirsToGoal[coord.X, coord.Z];
        pathC = coord.InDirectionInBoundsNonSelf(dir, Width, Height);

        if (SearchDirection == SearchDir.FromStart && pathC == GoalPos) { searchState = SearchState.Complete; return; }
        if (SearchDirection == SearchDir.Dual && pathC == SpawnPos) { searchState = SearchState.Complete; return; }
    }

    public Signpost SignpostAt(Coord coord)
    {
        return new Signpost(coord, currentFlow);
    }

    public readonly struct Signpost
    {
        public int DistanceToGoal { get; }

        public Compass DirToGoal { get; }

        public bool OnCriticalPath { get; }

        // Stepshot should never return the FlowField or any of its components directly.
        public Signpost(Coord coord, FlowField flow)
        {
            DistanceToGoal = flow.DistanceAt(coord);
            DirToGoal = flow.DirectionAt(coord);
            OnCriticalPath = flow.OnCriticalPath(coord);
        }
    }

}
