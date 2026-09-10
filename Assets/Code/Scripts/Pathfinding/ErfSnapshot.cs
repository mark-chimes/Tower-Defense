public readonly struct ErfSnapshot
{
    public Coord Coord { get; }
    public ErfKind Kind { get; }
    public bool HasWall { get; }

    // Snapshot should never return the FlowField or any of its components directly.
    public ErfSnapshot(Coord coord, ErfKind kind, bool hasWall)
    {
        Coord = coord;
        Kind = kind;
        HasWall = hasWall;
    }
}

