
public readonly struct Signpost
{
    public readonly Coord Coord;
    public readonly FlowField.Tile Tile;

    public int DistanceToGoal => Tile.Distance;

    public Compass DirToGoal => Tile.DirToGoal;

    public bool OnCriticalPath => Tile.OnCriticalPath;

    public Signpost(Coord coord, FlowField.Tile tile)
    {
        Coord = coord;
        Tile = tile;
    }

}
