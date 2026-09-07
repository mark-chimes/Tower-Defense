using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


// May as well call it this until I figure out what it does 
// In some way it is literally a God class; it lets you place 
// walls and recomputes the map etc.
// TODO separate concerns (some of these might still bundle / be split differently)
// taking variables in editor
// makes the view / physical unity objects
// handles input from the player
// manages walls
// displays gizmos
public class GodClass : MonoBehaviour
{
    [SerializeField] private Signpost signpostPrefab;
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

    [SerializeField] private Vector2Int spawnPosXZ = new(0, 0);
    [SerializeField] private Vector2Int goalPosXZ = new(1, 1);
    // Note it is possible to specify the above as out-of-bounds,
    // or as the same square. 
    // Improving it to add checks deferred to later
    Coord spawnPos;
    Coord goalPos;
    private TreasureMap treasureMap;

    private Signpost[,] signposts;
    private Wall[,] walls;

    private Camera cam;
    private const float MaxRayDistance = 500f;

    private Signpost hoveredCell;
    private IHighlightable highlighted;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        // TODO out-of-bounds check.
        spawnPos = new Coord(spawnPosXZ.x, spawnPosXZ.y);
        goalPos = new Coord(goalPosXZ.x, goalPosXZ.y);
       
        treasureMap = new TreasureMap(width, height, spawnPos, goalPos);

        signposts = new Signpost[width, height];
        walls = new Wall[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Coord coord = new Coord(x,z);

                Signpost view = Instantiate(signpostPrefab, transform);
                Vector3 pos = CoordsCellToWorld(coord);
                view.transform.localPosition = pos;
                view.name = $"Cell_{x}_{z}";
                view.Initialize(coord);
                signposts[x, z] = view;

                ErfSnapshot snap = treasureMap.At(coord);

                switch (snap.Kind) 
                {
                    case ErfKind.Spawn: InstantiateMarker(spawnPrefab, coord); break;
                    case ErfKind.Goal:  InstantiateMarker(goalPrefab, coord);  break;
                }
            }
        }

        UpdateDistances();
    }
    
    void InstantiateMarker(GameObject prefab, Coord coord)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.transform.localPosition = CoordsCellToWorld(coord);
    }
    
    // Breadth-first search
    // Cannot use when multiple tile-costs are involved
    void UpdateDistances()
    {
        treasureMap.RecomputeDistances();
        RefreshDistanceLabels();
    }

    void RefreshDistanceLabels()
    {
        for (int x = 0; x < treasureMap.Width; x++)
        {
            for (int z = 0; z < treasureMap.Height; z++)
            {
                Signpost signpost = signposts[x, z];
                signpost.UpdateDistance(treasureMap.DistanceToGoal(x,z));
            }
        }
    }


    void OnDrawGizmos()
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Color originalColor = Gizmos.color;

        Gizmos.matrix = transform.localToWorldMatrix;
        Vector3 size = new Vector3(cellSizeMeters, 1f, cellSizeMeters);
        Coord spawnPos = new Coord(spawnPosXZ.x, spawnPosXZ.y);
        Coord goalPos = new Coord(goalPosXZ.x, goalPosXZ.y);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Gizmos.color = Color.grey;
                Coord c = new Coord(x, z);
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
    private Vector3 CoordsCellToWorld(Coord coord)
    {
        float worldX = (coord.X - (width - 1) / 2f) * cellSizeMeters;
        float worldZ = (coord.Z - (height - 1) / 2f) * cellSizeMeters;
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
            Coord c = hoveredCell.Coord;
            ErfSnapshot erf = treasureMap.At(c);
            if (erf.Kind != ErfKind.Floor)
                highlightColor = blockedColor;
            else if (erf.HasWall)
                highlightColor = existingWallColor;
            else
                highlightColor = placeableColor;


            Wall wall = walls[c.X, c.Z];
            target = (wall != null) ? wall : hoveredCell;
        }
        highlighted?.Unhighlight();
        target?.Highlight(highlightColor);
        highlighted = target;
    }


    /// Currently assumes Walls have colliders off. Revisit if colliders turned on.
    private Signpost RaycastForCell()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (!Physics.Raycast(ray, out RaycastHit hit, MaxRayDistance)) return null;
        return hit.collider.GetComponentInParent<Signpost>();
    }

    private void PlaceWallAtHovered()
    {
        if (hoveredCell == null) return;
        if (!treasureMap.CanPlaceWall(hoveredCell.Coord)) return; // TODO: red ghost
        SpawnWall(hoveredCell.Coord);

    }

    private void DestroyWallAtHovered()
    {
        if (hoveredCell == null) return;
        DespawnWall(hoveredCell.Coord);

    }

    private void SpawnWall(Coord c)
    {
        if (walls[c.X, c.Z] != null) return;

        ErfSnapshot erf = treasureMap.At(c);

        if (erf.Kind != ErfKind.Floor)
        {
            Debug.LogError($"SpawnWall: {c} Kind was {erf.Kind}");
            return;
        }

        Wall wall = Instantiate(wallPrefab, wallsParent);
        wall.transform.localPosition = CoordsCellToWorld(c);
        wall.name = $"Wall_{c.X}_{c.Z}";
        walls[c.X, c.Z] = wall;
        treasureMap.SetWall(c, true);
        UpdateDistances();
    }

    private void DespawnWall(Coord c)
    {
        Wall wall = walls[c.X, c.Z];
        if (wall == null) return;

        ErfSnapshot cell = treasureMap.At(c);

        if (cell.Kind != ErfKind.Floor)
        {
            Debug.LogError($"DespawnWall: {c} Kind was {cell.Kind}", wall);
            return;
        }
        walls[c.X, c.Z] = null;
        if (ReferenceEquals(highlighted, wall)) highlighted = null;
        Destroy(wall.gameObject);
        treasureMap.SetWall(c, false);
        UpdateDistances();
    }
}
