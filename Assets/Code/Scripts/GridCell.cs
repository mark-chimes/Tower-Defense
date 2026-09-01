public class GridCell
{
    public readonly CellCoord Coord;

    public bool has_wall;

    public GridCell(CellCoord coord) {
        this.Coord = coord;
        has_wall = false;
    }
    
}
