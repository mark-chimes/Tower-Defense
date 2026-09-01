public class GridCell
{
    public readonly CellCoord Coord;

    public bool HasWall;

    public GridCell(CellCoord coord) {
        this.Coord = coord;
        HasWall = false;
    }
    
}
