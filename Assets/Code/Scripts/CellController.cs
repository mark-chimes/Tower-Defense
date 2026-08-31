using UnityEngine;

public class CellController : MonoBehaviour
{
    [SerializeField] private CellView cellPrefab;
    [SerializeField] private int width = 9;
    [SerializeField] private int height = 9;
    [SerializeField] private float cellSize_meters = 10f;

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
                float worldX = (x - (width-1) / 2f) * cellSize_meters;
                float worldZ = (z - (height-1) / 2f) * cellSize_meters;
                Vector3 pos = new Vector3(worldX, 0f, worldZ);
                CellView view = Instantiate(cellPrefab, transform); 
                view.Initialize(x, z);
                view.transform.localPosition = pos;
                view.name = $"Cell_{x}_{z}";

            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
