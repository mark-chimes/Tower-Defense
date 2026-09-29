using System.Collections.Generic;

public class FlowField
{


    private readonly Lattice<Tile> tiles;

    public FlowField(int numRings)
    {
        tiles = new Lattice<Tile>(numRings);
        ClearTiles();
    }

    public IReadOnlyCollection<FlowSample> FlowSamples()
    {
        var flows = new List<FlowSample>();
        foreach (HexCoord c in tiles.AllCoords())
            flows.Add(new FlowSample(c, tiles.At(c)));
        return flows;
    }

    public Tile TileAt(HexCoord c) => tiles.At(c);

    public void SetTile(HexCoord c, Tile t) => tiles.SetAt(c, t);

    public int DistanceAt(HexCoord c) => TileAt(c).Distance;
    public HexCompass DirectionAt(HexCoord c) => TileAt(c).DirToGoal;
    public bool OnCriticalPath(HexCoord c) => TileAt(c).OnCriticalPath;
    public bool Reachable(HexCoord c) => TileAt(c).Distance >= 0;


    public struct Tile

    {
        public readonly int Distance;
        public readonly HexCompass DirToGoal;
        public readonly bool OnCriticalPath;

        public Tile(int distance, HexCompass dirToGoal, bool onCriticalPath)
        {
            Distance = distance;
            DirToGoal = dirToGoal;
            OnCriticalPath = onCriticalPath;
        }
    }

    public static readonly Tile UnreachedTile = new Tile(-1, HexCompass.NONE, false);

    private void ClearTiles() => tiles.Fill(UnreachedTile);

}
