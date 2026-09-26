using System.Collections.Generic;
using UnityEngine;

public class HexFlowField
{


    private readonly HexMap<Tile> tiles;

    public HexFlowField(int numRings)
    {
        tiles = new HexMap<Tile>(numRings);
        ClearTiles();
    }

    public IReadOnlyCollection<HexSignpost> Signposts()
    {
        var list = new List<HexSignpost>();
        foreach (HexCoord c in tiles.AllCoords())
            list.Add(new HexSignpost(c, tiles.At(c)));
        return list;
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
