using System.Collections.Generic;

public class FlowField
{


    private readonly Lattice<Sample> tiles;

    public FlowField(int numRings)
    {
        tiles = new Lattice<Sample>(numRings);
        ClearTiles();
    }

    public IReadOnlyCollection<FlowSample> Signposts()
    {
        var list = new List<FlowSample>();
        foreach (HexCoord c in tiles.AllCoords())
            list.Add(new FlowSample(c, tiles.At(c)));
        return list;
    }

    public Sample TileAt(HexCoord c) => tiles.At(c);

    public void SetTile(HexCoord c, Sample t) => tiles.SetAt(c, t);

    public int DistanceAt(HexCoord c) => TileAt(c).Distance;
    public HexCompass DirectionAt(HexCoord c) => TileAt(c).DirToGoal;
    public bool OnCriticalPath(HexCoord c) => TileAt(c).OnCriticalPath;
    public bool Reachable(HexCoord c) => TileAt(c).Distance >= 0;


    public struct Sample

    {
        public readonly int Distance;
        public readonly HexCompass DirToGoal;
        public readonly bool OnCriticalPath;

        public Sample(int distance, HexCompass dirToGoal, bool onCriticalPath)
        {
            Distance = distance;
            DirToGoal = dirToGoal;
            OnCriticalPath = onCriticalPath;
        }
    }

    public static readonly Sample UnreachedTile = new Sample(-1, HexCompass.NONE, false);

    private void ClearTiles() => tiles.Fill(UnreachedTile);

}
