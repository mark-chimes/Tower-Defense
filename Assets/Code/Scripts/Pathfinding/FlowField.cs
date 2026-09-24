using System.Collections.Generic;

public class FlowField
{

    public readonly int Width;
    public readonly int Height;

    private readonly Tile[,] tiles;

    public FlowField(
        int width, int height)
    {
        Width = width;
        Height = height;

        tiles = new Tile[Width, Height];
        ClearTiles();
    }

    public IReadOnlyCollection<Signpost> Signposts()
    {
        var list = new List<Signpost>();

        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                list.Add(new Signpost(new Coord(x, z), this));
            }
        }
        return list;
    }

    public Tile TileAt(Coord c) => tiles[c.X, c.Z];

    public void SetTile(Coord c, Tile t) => tiles[c.X, c.Z] = t;

    public int DistanceAt(Coord c) => TileAt(c).Distance;
    public Compass DirectionAt(Coord c) => TileAt(c).DirToGoal;
    public bool OnCriticalPath(Coord c) => TileAt(c).OnCriticalPath;
    public bool Reachable(Coord c) => TileAt(c).Distance >= 0;


    public struct Tile

    {
        public readonly int Distance;
        public readonly Compass DirToGoal;
        public readonly bool OnCriticalPath;

        public Tile(int distance, Compass dirToGoal, bool onCriticalPath)
        {
            Distance = distance;
            DirToGoal = dirToGoal;
            OnCriticalPath = onCriticalPath;
        }
    }

    public static readonly Tile UnreachedTile = new Tile(-1, Compass.None, false);

    private void ClearTiles()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                tiles[x,z] = UnreachedTile;
            }
        }
    }

}