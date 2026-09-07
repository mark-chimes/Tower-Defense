public readonly struct ErfSnapshot 
{ 
    public Coord Coord { get; }
    public ErfKind Kind { get; }
    public int DistanceToGoal { get; }
    public bool HasWall { get; }

    public ErfSnapshot(Coord coord, ErfKind kind, int distanceToGoal, bool hasWall)
    {
        Coord = coord;
        Kind = kind;
        DistanceToGoal = distanceToGoal;
        HasWall = hasWall;
    }
}

