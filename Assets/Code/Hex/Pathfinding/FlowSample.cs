public class FlowSample
{
    public readonly HexCoord Coord;
    public readonly FlowField.Tile Tile;

    public int DistanceToGoal => Tile.Distance;

    public HexCompass DirToGoal => Tile.DirToGoal;

    public bool OnCriticalPath => Tile.OnCriticalPath;

    public FlowSample(HexCoord coord, FlowField.Tile tile)
    {
        Coord = coord;
        Tile = tile;
    }

    public override string ToString() => $"Signpost at {Coord} points {DirToGoal} and is {DistanceToGoal} steps from goal.";

}
