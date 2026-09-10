public readonly struct ErfSnapshot
{
    public Coord Coord { get; }
    public ErfKind Kind { get; }
    public int DistanceToGoal { get; }

    public Compass DirToGoal { get; }

    public bool OnCriticalPath { get; }

    public bool HasWall { get; }

    // Snapshot should never return the FlowField or any of its components directly.
    public ErfSnapshot(Coord coord, ErfKind kind, FlowField flow, bool hasWall)
    {
        Coord = coord;
        Kind = kind;
        DistanceToGoal = flow.DistanceAt(coord);
        DirToGoal = flow.DirectionAt(coord);
        OnCriticalPath = flow.OnCriticalPath(coord);
        HasWall = hasWall;
    }

    public ErfSnapshot(Coord coord, ErfKind kind, Wayfinder.Signpost sign, bool hasWall)
    {
        Coord = coord;
        Kind = kind;
        DistanceToGoal = sign.DistanceToGoal;
        DirToGoal = sign.DirToGoal;
        OnCriticalPath = sign.OnCriticalPath;
        HasWall = hasWall;
    }
}

