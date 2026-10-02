// TODO Restructure things to get rid of the HexCoord stored here, and get rid of FlowField.Tile as a concept.
// Basically this does the job that FlowField.Tile now does.

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

    public override string ToString() => $"FlowSample at {Coord} points {DirToGoal} and is {DistanceToGoal} steps from goal.";

}
