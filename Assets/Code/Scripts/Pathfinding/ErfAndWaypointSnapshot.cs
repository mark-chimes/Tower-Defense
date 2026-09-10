public readonly struct ErfAndWaypointSnapshot
{
    public Coord Coord { get; }
    public ErfKind Kind { get; }
    public int DistanceToGoal { get; }

    public Compass DirToGoal { get; }

    public bool OnCriticalPath { get; }

    public bool HasWall { get; }

    // Snapshot should never return the FlowField or any of its components directly.
    public ErfAndWaypointSnapshot(Coord coord, ErfKind kind, FlowField flow, bool hasWall)
    {
        Coord = coord;
        Kind = kind;
        DistanceToGoal = flow.DistanceAt(coord);
        DirToGoal = flow.DirectionAt(coord);
        OnCriticalPath = flow.OnCriticalPath(coord);
        HasWall = hasWall;
    }

    public ErfAndWaypointSnapshot(Coord coord, ErfKind kind, Signpost sign, bool hasWall)
    {
        Coord = coord;
        Kind = kind;
        DistanceToGoal = sign.DistanceToGoal;
        DirToGoal = sign.DirToGoal;
        OnCriticalPath = sign.OnCriticalPath;
        HasWall = hasWall;
    }
}

