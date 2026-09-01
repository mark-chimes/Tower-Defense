using System;
using UnityEngine;

public class CellController : MonoBehaviour
{
    [SerializeField] private CellView cellPrefab;
    [SerializeField] private int width = 9;
    [SerializeField] private int height = 9;
    [SerializeField] private float cellSizeMeters = 10f;

    [SerializeField] private GameObject walls_parent;
    [SerializeField] private Wall wallPrefab;


    private GridCell[,] cells;
    private CellView[,] views;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateGrid();
    }
    
    void GenerateGrid()
    {
        cells = new GridCell[width, height];
        views = new CellView[width, height];

        /* Just to test, remove */
        CellCoord[] wallsCoords = new CellCoord[3];
        wallsCoords[0] = new CellCoord(5,5);
        wallsCoords[1] = new CellCoord(6,6);
        wallsCoords[2] = new CellCoord(2,3);
        /* ^^^ */

 
        for (int x=0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                CellCoord coord = new CellCoord(x,z);
                GridCell cell = new GridCell(coord);
                cells[x,z] = cell;

                CellView view = Instantiate(cellPrefab, transform); 
                Vector3 pos = CoordsCellToWorld(coord);
                view.transform.localPosition = pos;
                view.name = $"Cell_{x}_{z}";
                view.Initialize(coord);
                views[x,z] = view;

                /* Just to test, remove */
                for (int i=0; i < 3; i++)
                {
                    CellCoord wall_coord = wallsCoords[i];
                    if (wall_coord == coord)
                    {
                        cell.has_wall = true;
                    }
                } 
                /* ^^^ */

                if (cell.has_wall)
                {
                    Wall wall = Instantiate(wallPrefab, transform); 
                    wall.transform.localPosition = pos;
                    wall.name = $"Wall_{x}_{z}";
                    // todo, put wall on walls_parent
                }

            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.grey;
        Vector3 size = new Vector3(cellSizeMeters, 0f, cellSizeMeters);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Gizmos.DrawWireCube(CoordsCellToWorld(new CellCoord(x,z)), size);
            }
        }
    }

    // Translate from cell coordinates to world coordinates
    private Vector3 CoordsCellToWorld(CellCoord coord)
    {
        float worldX = (coord.X - (width-1) / 2f) * cellSizeMeters;
        float worldZ = (coord.Z - (height-1) / 2f) * cellSizeMeters;
        return new Vector3(worldX, 0f, worldZ);
    }
}
