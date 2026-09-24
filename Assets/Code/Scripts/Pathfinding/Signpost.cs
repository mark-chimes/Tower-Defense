
public readonly struct Signpost
{
    public readonly Coord Coord;
    public readonly FlowField.Tile Tile;

    public int DistanceToGoal => Tile.Distance;

    public Compass DirToGoal => Tile.DirToGoal;

    public bool OnCriticalPath => Tile.OnCriticalPath;




    // Signpost should never return the FlowField or any of its components directly.
    public Signpost(Coord coord, FlowField flow)
    {
        Coord = coord;
        Tile = flow.TileAt(coord);
    }


    public Signpost(Coord coord, FlowField.Tile tile)
    {
        Coord = coord;
        Tile = tile;
    }

}
