using UnityEngine;

public class CellController : MonoBehaviour
{
    [SerializeField] private CellView cellPrefab;
    [SerializeField] private int width = 9;
    [SerializeField] private int height = 9;
    [SerializeField] private float cellSizeMeters = 10f;

    private GridCell[,] cells;

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
                GridCell cell = new GridCell(x,z);
                cells[x,z] = cell;

                Vector3 pos = CoordsCellToWorld(x,z);

                CellView view = Instantiate(cellPrefab, transform); 
                view.transform.localPosition = pos;
                view.name = $"Cell_{x}_{z}";
                view.Initialize(x, z);

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
                Gizmos.DrawWireCube(CoordsCellToWorld(x, z), size);
            }
        }
    }

    // Translate from cell coordinates to world coordinates
    private Vector3 CoordsCellToWorld(int x, int z)
    {
        float worldX = (x - (width-1) / 2f) * cellSizeMeters;
        float worldZ = (z - (height-1) / 2f) * cellSizeMeters;
        return new Vector3(worldX, 0f, worldZ);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
