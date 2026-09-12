public readonly struct ErfSnapshot
{
    public Coord Coord { get; }
    public SpawnGoalKind Kind { get; }
    public bool HasWall { get; }

    // Snapshot should never return the FlowField or any of its components directly.
    public ErfSnapshot(Coord coord, SpawnGoalKind kind, bool hasWall)
    {
        Coord = coord;
        Kind = kind;
        HasWall = hasWall;
    }
}

