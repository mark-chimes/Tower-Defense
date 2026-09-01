using UnityEngine;

public class CellController : MonoBehaviour
{
    [SerializeField] private CellView cellPrefab;
    [SerializeField] private int width = 9;
    [SerializeField] private int height = 9;
    [SerializeField] private float cellSizeMeters = 10f;

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
                view.name = $"Cell_{coord.X}_{coord.Z}";
                view.Initialize(coord);
                views[x,z] = view;
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
