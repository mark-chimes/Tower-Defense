using UnityEngine;
using UnityEngine.InputSystem;


public class CellController : MonoBehaviour
{
    [SerializeField] private CellView cellPrefab;
    [SerializeField] private int width = 9;
    [SerializeField] private int height = 9;
    [SerializeField] private float cellSizeMeters = 10f;

    [SerializeField] private Transform wallsParent;
    [SerializeField] private Wall wallPrefab;

    // whether highlight stays on last hovered grid cell or gets cleared 
    // when mouse cursor moves away
    [SerializeField] private bool isHighlightSticky; 

    private GridCell[,] cells;
    private CellView[,] views;
    private Wall[,] wallObjects;

    private Camera cam;
    private const float MAX_RAY_DISTANCE = 500f;

    private CellView hovered;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateGrid();
    }
    
    void GenerateGrid()
    {
        cells = new GridCell[width, height];
        views = new CellView[width, height];
        wallObjects = new Wall[width, height];

        /* Just to test, remove */
        CellCoord[] wallsCoords = new CellCoord[3];
        wallsCoords[0] = new CellCoord(4,5);
        wallsCoords[1] = new CellCoord(6,6);
        wallsCoords[2] = new CellCoord(3,3);
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
                        cell.HasWall = true;
                    }
                } 
                /* ^^^ */

                if (cell.HasWall)
                {
                    Wall wall = Instantiate(wallPrefab, wallsParent); 
                    wall.transform.localPosition = pos;
                    wall.name = $"Wall_{x}_{z}";
                    wallObjects[x,z] = wall;
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

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandleMouse();
    }

    private void HandleMouse()
    {
        if (Mouse.current == null) return;

        HighlightHoveredCell();
        if (Mouse.current.leftButton.wasPressedThisFrame) PlaceWallAtHovered();
    }

    private void HighlightHoveredCell()
    {

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);
        CellView view = null;

        if (Physics.Raycast(ray, out RaycastHit hit, MAX_RAY_DISTANCE))
        {
            view = hit.collider.GetComponentInParent<CellView>();
        } else if (isHighlightSticky)
        {
            return;
        }
        
        if (view == hovered) return;

        if (hovered != null) hovered.Unhighlight();
        if (view != null) view.Highlight();
        hovered = view;
    
    }

    private void PlaceWallAtHovered()
    {
        if (hovered == null) return;
        CellCoord coord = hovered.Coord;
        if (wallObjects[coord.X, coord.Z] != null) return;

        GridCell cell = cells[coord.X, coord.Z];
        cell.HasWall = true;

        Wall wall = Instantiate(wallPrefab, wallsParent); 
        wall.transform.localPosition = CoordsCellToWorld(coord);
        wall.name = $"Wall_{coord.X}_{coord.Z}";
        wallObjects[coord.X,coord.Z] = wall;

    }
}
