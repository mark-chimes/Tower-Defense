public readonly struct ErfSnapshot 
{ 
    public Coord Coord { get; }
    public ErfKind Kind { get; }
    public int DistanceToGoal { get; }

    public Cardinal CameFrom { get; }

    public bool HasWall { get; }

    public ErfSnapshot(Coord coord, ErfKind kind, int distanceToGoal, Cardinal cameFrom, bool hasWall)
    {
        Coord = coord;
        Kind = kind;
        DistanceToGoal = distanceToGoal;
        CameFrom = cameFrom;
        HasWall = hasWall;
    }
}

