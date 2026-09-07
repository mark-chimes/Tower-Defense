public class CellData
{
    public readonly CellCoord Coord;

    public int DistanceToGoal;

    public CellKind Kind;
    public bool HasWall;

    public CellData(CellCoord coord) {
        this.Coord = coord;
        this.DistanceToGoal = -1;
    }
    
}
