public readonly struct HexSnapshot
{
    public HexCoord Coord { get; }
    public TreasureMap.Landmark Landmark { get; }
    public bool IsLand { get; }

    // Snapshot should never return the FlowField or any of its components directly.
    public HexSnapshot(HexCoord coord, TreasureMap.Landmark landmark, bool isLand)
    {
        Coord = coord;
        Landmark = landmark;
        IsLand = isLand;
    }
}

