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
    [SerializeField] private float erfSizeMeters = 10f;

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

    private TreasureMap treasureMap;

    private Signpost[,] signposts;
    private Wall[,] walls;

    private Camera cam;
    private const float MaxRayDistance = 500f;

    private Signpost hoveredErf;
    private IHighlightable highlighted;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        // TODO out-of-bounds check.
        Coord spawnPos = new Coord(spawnPosXZ.x, spawnPosXZ.y);
        Coord goalPos = new Coord(goalPosXZ.x, goalPosXZ.y);
       
        treasureMap = new TreasureMap(width, height, spawnPos, goalPos);

        signposts = new Signpost[width, height];
        walls = new Wall[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Coord coord = new Coord(x,z);

                Signpost signpost = Instantiate(signpostPrefab, transform);
                Vector3 pos = CoordsToWorld(coord);
                signpost.transform.localPosition = pos;
                signpost.name = $"Signpost_{x}_{z}";
                signpost.Initialize(coord);
                signposts[x, z] = signpost;

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
        obj.transform.localPosition = CoordsToWorld(coord);
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
                ErfSnapshot erf = treasureMap.At(x,z);
                signpost.UpdateDistance(erf.DistanceToGoal);
                signpost.UpdateArrow(erf.DirToGoal, erf.OnCriticalPath);
            }
        }
    }


    void OnDrawGizmos()
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Color originalColor = Gizmos.color;

        Gizmos.matrix = transform.localToWorldMatrix;
        Vector3 size = new Vector3(erfSizeMeters, 1f, erfSizeMeters);

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
                    Gizmos.DrawCube(CoordsToWorld(c), size);
                }
                else if (c == goalPos)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawCube(CoordsToWorld(c), size);
                }
                else
                {
                    Gizmos.DrawWireCube(CoordsToWorld(c), size);
                }


            }
        }
        Gizmos.matrix = originalMatrix;
        Gizmos.color = originalColor;
    }

    private Vector3 CoordsToWorld(Coord coord)
    {
        float worldX = (coord.X - (width - 1) / 2f) * erfSizeMeters;
        float worldZ = (coord.Z - (height - 1) / 2f) * erfSizeMeters;
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

        HighlightAtHoveredErf();
        if (Mouse.current.leftButton.wasPressedThisFrame) PlaceWallAtHovered();
        if (Mouse.current.rightButton.wasPressedThisFrame) DestroyWallAtHovered();
    }

    private void HighlightAtHoveredErf()
    {
        hoveredErf = RaycastForErf();
        IHighlightable target = null;
        Color highlightColor = Color.magenta; // something went wrong if this is the highlight color
        if (hoveredErf != null)
        {
            Coord c = hoveredErf.Coord;
            ErfSnapshot erf = treasureMap.At(c);
            if (erf.Kind != ErfKind.Floor)
                highlightColor = blockedColor;
            else if (erf.HasWall)
                highlightColor = existingWallColor;
            else
                highlightColor = placeableColor;


            Wall wall = walls[c.X, c.Z];
            target = (wall != null) ? wall : hoveredErf;
        }
        highlighted?.Unhighlight();
        target?.Highlight(highlightColor);
        highlighted = target;
    }


    /// Currently assumes Walls have colliders off. Revisit if colliders turned on.
    private Signpost RaycastForErf()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (!Physics.Raycast(ray, out RaycastHit hit, MaxRayDistance)) return null;
        return hit.collider.GetComponentInParent<Signpost>();
    }

    private void PlaceWallAtHovered()
    {
        if (hoveredErf == null) return;
        if (!treasureMap.CanPlaceWall(hoveredErf.Coord)) return; // TODO: red ghost
        SpawnWall(hoveredErf.Coord);

    }

    private void DestroyWallAtHovered()
    {
        if (hoveredErf == null) return;
        DespawnWall(hoveredErf.Coord);

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
        wall.transform.localPosition = CoordsToWorld(c);
        wall.name = $"Wall_{c.X}_{c.Z}";
        walls[c.X, c.Z] = wall;
        treasureMap.SetWall(c, true);
        UpdateDistances();
    }

    private void DespawnWall(Coord c)
    {
        Wall wall = walls[c.X, c.Z];
        if (wall == null) return;

        ErfSnapshot erf = treasureMap.At(c);

        if (erf.Kind != ErfKind.Floor)
        {
            Debug.LogError($"DespawnWall: {c} Kind was {erf.Kind}", wall);
            return;
        }
        walls[c.X, c.Z] = null;
        if (ReferenceEquals(highlighted, wall)) highlighted = null;
        Destroy(wall.gameObject);
        treasureMap.SetWall(c, false);
        UpdateDistances();
    }
}
