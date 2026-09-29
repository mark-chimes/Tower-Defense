public class FlowSample
{
    public readonly HexCoord Coord;
    public readonly FlowField.Sample Tile;

    public int DistanceToGoal => Tile.Distance;

    public HexCompass DirToGoal => Tile.DirToGoal;

    public bool OnCriticalPath => Tile.OnCriticalPath;

    public FlowSample(HexCoord coord, FlowField.Sample tile)
    {
        Coord = coord;
        Tile = tile;
    }

    public override string ToString() => $"Signpost at {Coord} points {DirToGoal} and is {DistanceToGoal} steps from goal.";

}
