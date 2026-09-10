using System.Collections.Generic;


public class Wayfinder
{
    public Coord SpawnPos { get; }
    public Coord GoalPos { get; }

    public readonly int Width;
    public readonly int Height;


    private FlowField currentFlow;


    // TODO State Machine (enum?)
    private SearchState searchState = SearchState.DistanceCalc;


    private enum SearchState
    {
        DistanceCalc,
        PathCalc,
        Complete,
    }

    public enum SearchDirection
    {
        FromStart,
        FromEnd,
        Dual
    }

    bool[,] visited;
    Queue<Coord> erfQueue;
    Coord? pathC;


    public Wayfinder(int width, int height, Coord spawnPos, Coord goalPos)
    {
        currentFlow = new FlowField(width, height);
        SpawnPos = spawnPos;
        GoalPos = goalPos;
        Width = width;
        Height = height;
    }

    public void ClearField()
    {
        visited = new bool[Width, Height];
        erfQueue = new Queue<Coord>();
        currentFlow = new FlowField(Width, Height);
        searchState = SearchState.DistanceCalc;

        Coord g = GoalPos;
        visited[g.X, g.Z] = true;
        currentFlow.distanceToGoal[g.X, g.Z] = 0;
        erfQueue.Enqueue(g);
        pathC = SpawnPos;
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
        if (searchState != SearchState.DistanceCalc) return;

        int[,] distancesToGoal = currentFlow.distanceToGoal;
        Compass[,] dirsToGoal = currentFlow.dirToGoal;

        if (erfQueue.TryDequeue(out var coord))
        {
            int dist = distancesToGoal[coord.X, coord.Z];

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
                    distancesToGoal[c.X, c.Z] = -1;
                    continue;
                }

                distancesToGoal[c.X, c.Z] = dist + 1;
                dirsToGoal[c.X, c.Z] = dir.Opposite();
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

        if (pathC != null && pathC != GoalPos)
        {
            Coord coord = (Coord)pathC;
            onCriticalPath[coord.X, coord.Z] = true;
            Compass dir = dirsToGoal[coord.X, coord.Z];
            pathC = coord.InDirectionInBoundsNonSelf(dir, Width, Height);
        }
        else
        {
            searchState = SearchState.Complete;
        }
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
