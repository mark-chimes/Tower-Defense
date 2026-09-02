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
    [SerializeField] private GameObject spawnPrefab;

    [SerializeField] private GameObject goalPrefab;


    [SerializeField] private Color placeableColor = Color.green; 
    [SerializeField] private Color blockedColor = Color.red; 
    [SerializeField] private Color existingWallColor = Color.yellow; 

    [SerializeField] private Vector2Int spawnPosXZ = new (0,0);
    [SerializeField] private Vector2Int goalPosXZ = new (1,1);
    // Note it is possible to specify the above as out-of-bounds,
    // or as the same square. 
    // Improving it to add checks deferred to later


    private GridCell[,] cells;
    private CellView[,] views;
    private Wall[,] wallObjects;

    private Camera cam;
    private const float MaxRayDistance = 500f;

    private CellView hoveredCell;
    private IHighlightable highlighted;



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

        CellCoord spawnPos = new CellCoord(spawnPosXZ.x, spawnPosXZ.y);
        CellCoord goalPos = new CellCoord(goalPosXZ.x, goalPosXZ.y);

        for (int x=0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                CellCoord coord = new CellCoord(x,z);
                GridCell cell = new GridCell(coord);
                if (coord == spawnPos)
                {
                    cell.Kind = CellKind.Spawn;
                    GameObject spawnObj = Instantiate(spawnPrefab, transform); 
                    spawnObj.transform.localPosition = CoordsCellToWorld(coord);
                } else if (coord == goalPos) {
                    cell.Kind = CellKind.Goal;
                    GameObject goalObj = Instantiate(goalPrefab, transform); 
                    goalObj.transform.localPosition = CoordsCellToWorld(coord);
                }
                cells[x,z] = cell;

                CellView view = Instantiate(cellPrefab, transform); 
                Vector3 pos = CoordsCellToWorld(coord);
                view.transform.localPosition = pos;
                view.name = $"Cell_{x}_{z}";
                view.Initialize(coord);
                views[x,z] = view;
            }
        }
    }

    void OnDrawGizmos()
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Color originalColor = Gizmos.color;

        Gizmos.matrix = transform.localToWorldMatrix;
        Vector3 size = new Vector3(cellSizeMeters, 1f, cellSizeMeters);
        CellCoord spawnPos = new CellCoord(spawnPosXZ.x, spawnPosXZ.y);
        CellCoord goalPos = new CellCoord(goalPosXZ.x, goalPosXZ.y);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Gizmos.color = Color.grey;
                CellCoord c = new CellCoord(x,z);
                if (c == spawnPos)
                {
                    Gizmos.color = Color.lightBlue;        
                    Gizmos.DrawCube(CoordsCellToWorld(c), size);  
                } 
                else if (c == goalPos)  
                {
                    Gizmos.color = Color.yellow;        
                    Gizmos.DrawCube(CoordsCellToWorld(c), size); 
                }
                else
                {
                    Gizmos.DrawWireCube(CoordsCellToWorld(c), size);
                }
                

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

        HighlightAtHoveredCell();
        if (Mouse.current.leftButton.wasPressedThisFrame) PlaceWallAtHovered();
        if (Mouse.current.rightButton.wasPressedThisFrame) DestroyWallAtHovered();
    }

    private void HighlightAtHoveredCell()
    {
        hoveredCell = RaycastForCell();
        IHighlightable target = null;
        Color highlightColor = Color.magenta; // something went wrong if this is the highlight color
        if (hoveredCell != null)
        {
            CellCoord c = hoveredCell.Coord;
            GridCell cell = cells[c.X, c.Z];
            if (cell.Kind != CellKind.Floor)
                highlightColor = blockedColor;  
            else if (cell.HasWall)
                highlightColor = existingWallColor;  
            else
                highlightColor = placeableColor; 
            

            Wall wall = wallObjects[c.X, c.Z];
            target = (wall != null) ? wall : hoveredCell;
        }
        highlighted?.Unhighlight();
        target?.Highlight(highlightColor);
        highlighted = target;
    }


    /// Currently assumes Walls have colliders off. Revisit if colliders turned on.
    private CellView RaycastForCell()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (!Physics.Raycast(ray, out RaycastHit hit, MaxRayDistance)) return null;
        return hit.collider.GetComponentInParent<CellView>();
    }

    private bool CanPlaceWall(CellCoord c) => cells[c.X, c.Z].Kind == CellKind.Floor
        && wallObjects[c.X, c.Z] == null;

    private void PlaceWallAtHovered()
    {
        if (hoveredCell == null) return;
        if (!CanPlaceWall(hoveredCell.Coord)) return; // TODO: red ghost
        SpawnWall(hoveredCell.Coord);
    }

    private void DestroyWallAtHovered()
    {
        if (hoveredCell == null) return;
        DespawnWall(hoveredCell.Coord);
    }

    private void SpawnWall(CellCoord c)
    {
        if (wallObjects[c.X, c.Z] != null) return;

        GridCell cell = cells[c.X, c.Z];
        
        if (cell.Kind != CellKind.Floor)
        {
            Debug.LogError($"SpawnWall: {c} Kind was {cell.Kind}");
            return;
        }

        cell.HasWall = true;
        Wall wall = Instantiate(wallPrefab, wallsParent); 
        wall.transform.localPosition = CoordsCellToWorld(c);
        wall.name = $"Wall_{c.X}_{c.Z}";
        wallObjects[c.X,c.Z] = wall;
    }

    private void DespawnWall(CellCoord c)
    {
        Wall wall = wallObjects[c.X, c.Z];
        if (wall == null) return;

        GridCell cell = cells[c.X, c.Z];

        if (cell.Kind != CellKind.Floor)
        {
            Debug.LogError($"DespawnWall: {c} Kind was {cell.Kind}", wall);
            return;
        }

        cell.HasWall = false;
        wallObjects[c.X, c.Z] = null;
        if (ReferenceEquals(highlighted, wall)) highlighted = null;
        Destroy(wall.gameObject);
    }
}
