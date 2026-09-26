public readonly struct HexSnapshot
{
    public HexCoord Coord { get; }
    public HexTreasureMap.TileMarker TileMarker { get; }
    public bool HasWall { get; }

    // Snapshot should never return the FlowField or any of its components directly.
    public HexSnapshot(HexCoord coord, HexTreasureMap.TileMarker tileMarker, bool hasWall)
    {
        Coord = coord;
        TileMarker = tileMarker;
        HasWall = hasWall;
    }
}

