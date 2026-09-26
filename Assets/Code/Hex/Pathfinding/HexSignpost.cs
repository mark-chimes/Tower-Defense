using UnityEngine;

public class HexSignpost
{
    public readonly HexCoord Coord;
    public readonly HexFlowField.Tile Tile;

    public int DistanceToGoal => Tile.Distance;

    public HexCompass DirToGoal => Tile.DirToGoal;

    public bool OnCriticalPath => Tile.OnCriticalPath;

    public HexSignpost(HexCoord coord, HexFlowField.Tile tile)
    {
        Coord = coord;
        Tile = tile;
    }

    public override string ToString() => $"Signpost at {Coord} points {DirToGoal} and is {DistanceToGoal} steps from goal.";

}
