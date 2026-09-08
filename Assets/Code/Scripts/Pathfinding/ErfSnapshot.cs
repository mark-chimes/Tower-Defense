public readonly struct ErfSnapshot 
{ 
    public Coord Coord { get; }
    public ErfKind Kind { get; }
    public int DistanceToGoal { get; }

    public Cardinal DirToGoal { get; }

    public bool OnCriticalPath { get; }

    public bool HasWall { get; }

    public ErfSnapshot(Coord coord, ErfKind kind, int distanceToGoal, Cardinal dirToGoal, bool onCriticalPath, bool hasWall)
    {
        Coord = coord;
        Kind = kind;
        DistanceToGoal = distanceToGoal;
        DirToGoal = dirToGoal;
        OnCriticalPath = onCriticalPath;
        HasWall = hasWall;
    }
}

