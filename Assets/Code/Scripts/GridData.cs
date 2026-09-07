public class GridData 
{

    private GridCell[,] Cells;
    private CellCoord goalPos;


    public GridData(GridCell[,] cells, CellCoord goalPos) { 
        Cells = cells;
    }

    public GridCell At(CellCoord coord) => At(coord.X, coord.Z);
    public GridCell At(int x, int z) => Cells[x, z];

    public bool CanPlaceWall(CellCoord c) => Cells[c.X, c.Z].Kind == CellKind.Floor
        && !Cells[c.X, c.Z].HasWall;
    public void RecomputeDistances()
    {   
        int width = Cells.GetLength(0);
        int height = Cells.GetLength(1);

        DistanceCompute.RecomputeDistances(Cells, width, height, goalPos);
    }
}


