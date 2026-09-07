using System.Collections.Generic;

public class DistanceCompute {

    public static void RecomputeDistances(int width, int height, CellCoord goalPos, GridCell[,] cells)
    {   
        bool[,] visited = new bool[width, height];
        Queue<CellCoord> cellQueue = new Queue<CellCoord>();

        CellCoord g = goalPos;
        visited[g.X, g.Z] = true;
        GridCell goalCell = cells[g.X, g.Z];
        goalCell.DistanceToGoal = 0;
        cellQueue.Enqueue(g);

        while (cellQueue.TryDequeue(out var coord))
        {
            GridCell topCell = cells[coord.X, coord.Z];
            int? dist = topCell.DistanceToGoal;
            CellCoord[] adjacents = Adjacents(coord, 0, 0, width-1, height-1);

            for(int i = 0; i < adjacents.Length; i++)
            {
                CellCoord c = adjacents[i];
                if (visited[c.X, c.Z]) {
                    continue;
                }
                visited[c.X, c.Z] = true;
                GridCell cell = cells[c.X, c.Z];
                if (cell.HasWall) {
                    cell.DistanceToGoal = null;
                    continue;
                }
                
                cell.DistanceToGoal = dist+1;

                cellQueue.Enqueue(c);
            }
        }

        for (int x=0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (!visited[x,z])
                {
                    GridCell cell = cells[x, z];
                    cell.DistanceToGoal = null;
                }
            }
        }
    }

    
    static private CellCoord[] Adjacents(CellCoord c, int minX, int minZ, int maxX, int maxZ)
    { 
        // out of bounds
        if (c.X < minX || c.Z < minZ || c.X > maxX || c.Z > maxZ) {
            return new CellCoord[0];
        }

        int n = 0;
        CellCoord[] adjacents = new CellCoord[4];

        if (c.X != minX) {
            adjacents[n] = new CellCoord(c.X-1, c.Z);
            n++;
        } 

        if (c.Z != minZ) {
            adjacents[n] = new CellCoord(c.X, c.Z-1);
            n++;
        }

        if (c.X != maxX) {
            adjacents[n] = new CellCoord(c.X+1, c.Z);
            n++;
        }

        if (c.Z != maxZ) {
            adjacents[n] = new CellCoord(c.X, c.Z+1);
            n++;
        }

        CellCoord[] adjacentsOnly = new CellCoord[n];
        for (int i = 0; i < n; i++)
        {
            adjacentsOnly[i] = adjacents[i];
        } 

        return adjacentsOnly;
    }


}   