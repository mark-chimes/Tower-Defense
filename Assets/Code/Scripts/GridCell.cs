public class GridCell
{
    public readonly CellCoord Coord;

    public int? DistanceToGoal;

    public CellKind Kind;
    public bool HasWall;

    public GridCell(CellCoord coord) {
        this.Coord = coord;
    }
    
}
