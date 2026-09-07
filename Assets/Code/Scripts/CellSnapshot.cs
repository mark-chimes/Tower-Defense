public readonly struct CellSnapshot 
{ 
    public CellCoord Coord { get; }
    public CellKind Kind { get; }
    public int DistanceToGoal { get; }
    public bool HasWall { get; }

    public CellSnapshot(CellCoord coord, CellKind kind, int distanceToGoal, bool hasWall)
    {
        Coord = coord;
        Kind = kind;
        DistanceToGoal = distanceToGoal;
        HasWall = hasWall;
    }

    public static CellSnapshot FromData(CellData data)
    {
        return new CellSnapshot(data.Coord, data.Kind, data.DistanceToGoal, data.HasWall);
    }
}

