public class GridCell
{
    public readonly CellCoord Coord;

    public int? distanceToGoal;

    public CellKind Kind;
    public bool HasWall;

    public GridCell(CellCoord coord) {
        this.Coord = coord;
    }
    
}
