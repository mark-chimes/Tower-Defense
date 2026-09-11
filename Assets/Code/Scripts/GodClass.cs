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
    [SerializeField] private DirectionMarker directionMarkerPrefab;
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

    [SerializeField] private float visualizeFPS = 60f;

    [SerializeField] private bool isStopOnPathFound = true; // TODO this should be via debug buttons in-game


    // TODO this is an ugly way of doing this - temp debug only
    [SerializeField] private bool shouldHighlight = false;
    [SerializeField] private bool isHighlightPulse = false;

    private float visualizeTime;


    private TreasureMap treasureMap;

    private DirectionMarker[,] directionMarkers;
    private Wall[,] walls;

    private Camera cam;
    private const float MaxRayDistance = 500f;

    private DirectionMarker hoveredErf;
    private IHighlightable highlighted;

    private bool isAutoRefreshMode = false;
    private bool isVisualizeMode = false;

    void Start()
    {
        visualizeTime = 1f / visualizeFPS;
        GenerateGrid();
    }

    void Update()
    {
        HandleMouse();
        ContinuallySingleStep();
    }



    void GenerateGrid()
    {
        // TODO out-of-bounds check.
        Coord spawnPos = new Coord(spawnPosXZ.x, spawnPosXZ.y);
        Coord goalPos = new Coord(goalPosXZ.x, goalPosXZ.y);

        treasureMap = new TreasureMap(width, height, spawnPos, goalPos, isStopOnPathFound);

        directionMarkers = new DirectionMarker[width, height];
        walls = new Wall[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Coord coord = new Coord(x, z);

                DirectionMarker directionMarker = Instantiate(directionMarkerPrefab, transform);
                Vector3 pos = CoordsToWorld(coord);
                directionMarker.transform.localPosition = pos;
                directionMarker.name = $"DirectionMarker_{x}_{z}";
                directionMarker.Initialize(coord);
                directionMarkers[x, z] = directionMarker;

                ErfSnapshot erf = treasureMap.At(coord);

                switch (erf.Kind)
                {
                    case ErfKind.Spawn: InstantiateMarker(spawnPrefab, coord); break;
                    case ErfKind.Goal: InstantiateMarker(goalPrefab, coord); break;
                }
            }
        }

        previousSigns = System.Array.Empty<Signpost>();
        treasureMap.ClearField();
        UpdateDistances();
    }

    void InstantiateMarker(GameObject prefab, Coord coord)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.transform.localPosition = CoordsToWorld(coord);
    }

    /** Slow pathfinding and refresh code **/

    public void SetAutoRefreshMode(bool isEnabled)
    {
        isAutoRefreshMode = isEnabled;
        if (isAutoRefreshMode)
        {
            isVisualizeMode = false;
            treasureMap.Recompute();
            RefreshDistanceLabels();
        }
    }

    public void OnFromStartModePressed()
    {
        isVisualizeMode = false;
        treasureMap.SetModeAndClear(Search.Dir.FromStart);
        RefreshDistanceLabels();
    }

    public void OnFromEndModePressed()
    {
        isVisualizeMode = false;
        treasureMap.SetModeAndClear(Search.Dir.FromEnd);
        RefreshDistanceLabels();
    }

    public void OnRefreshPressed()
    {
        treasureMap.Recompute();
        RefreshDistanceLabels();
    }

    public void OnClearFieldPressed()
    {
        previousSigns = System.Array.Empty<Signpost>();
        isVisualizeMode = false;
        treasureMap.ClearField();
        RefreshDistanceLabels();
    }

    public void OnVisualizePressed()
    {
        isVisualizeMode = true;
    }

    public void OnSingleStepPressed()
    {
        SingleStep();
    }

    float tempTime;


    void ContinuallySingleStep()
    {
        if (isAutoRefreshMode) return;
        if (!isVisualizeMode) return;

        tempTime += Time.deltaTime;
        if (tempTime > visualizeTime)
        {
            tempTime = 0;
            SingleStep();
        }

    }

    private void SingleStep()
    {
        Search.Delta delta = treasureMap.SingleStep();

        // TODO Ugly way to handle this - temporary debug only.
        if (isHighlightPulse)
        {
            RefreshFromDeltaHighlightPulse(delta);
        }
        else
        {
            RefreshFromDeltaHighlightFrontier(delta);
        }
    }

    void UpdateDistances()
    {
        if (!isAutoRefreshMode)
        {
            Debug.Log("Auto refresh mode disabled, not updating distances");
        }
        else
        {
            treasureMap.Recompute();
        }
        RefreshDistanceLabels();
    }


    void RefreshFromDeltaHighlightFrontier(Search.Delta delta)
    {
        Debug.Log("Refresh from delta");


        switch (delta.Phase)
        {
            case Search.Phase.ExpandFrontier:
                {
                    UnhighlightAllArrows(); 
                    foreach (Signpost sign in delta.Changed)
                    {
                        Coord c = sign.Coord;
                        DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
                        directionMarker.UpdateDistance(sign.DistanceToGoal);
                        directionMarker.PointTo(sign.DirToGoal);
                    }

                    foreach (Coord c in treasureMap.CurrentFrontier())
                    {
                        DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
                        directionMarker.HighlightFrontierArrow();
                    }

                    break;
                }
            case Search.Phase.TracePath:
                {
                    foreach (Signpost sign in delta.Changed)
                    {
                        Coord c = sign.Coord;
                        DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
                        directionMarker.HighlightArrowOnPath();
                    }
                    break;
                }
            case Search.Phase.Done:
                {
                    return;
                }
        }
    }

    private void UnhighlightAllArrows()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                DirectionMarker directionMarker = directionMarkers[x, z];
                directionMarker.UnhighlightArrow();
            }
        }

    }

    private IReadOnlyCollection<Signpost> previousSigns; // = System.Array.Empty<Signpost>();
    void RefreshFromDeltaHighlightPulse(Search.Delta delta)
    {
        Debug.Log("Refresh from delta");

        switch (delta.Phase)
        {
            case Search.Phase.ExpandFrontier:
                {
                    foreach (Signpost sign in previousSigns)
                    {
                        Coord c = sign.Coord;
                        DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
                        directionMarker.UnhighlightArrow();
                    }
                    previousSigns = delta.Changed;
                    foreach (Signpost sign in delta.Changed)
                    {
                        Coord c = sign.Coord;
                        DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
                        directionMarker.UpdateDistance(sign.DistanceToGoal);
                        directionMarker.TurnAndHighlightArrowFrontier(sign.DirToGoal, shouldHighlight);
                    }
                    break;
                }
            case Search.Phase.TracePath:
                {
                    UnhighlightAllPreviousSigns();

                    foreach (Signpost sign in delta.Changed)
                    {
                        Coord c = sign.Coord;
                        DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
                        directionMarker.HighlightArrowOnPath();
                    }
                    break;
                }
            case Search.Phase.Done:
                {
                    UnhighlightAllPreviousSigns();

                    return;
                }
        }
    }

    private void UnhighlightAllPreviousSigns()
    {
        foreach (Signpost sign in previousSigns)
        {
            Coord c = sign.Coord;
            DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
            directionMarker.UnhighlightArrow();
        }
        previousSigns = System.Array.Empty<Signpost>();
    }

    void RefreshDistanceLabels()
    {
        Debug.Log("RefreshDistanceLabels");

        for (int x = 0; x < treasureMap.Width(); x++)
        {
            for (int z = 0; z < treasureMap.Height(); z++)
            {
                DirectionMarker directionMarker = directionMarkers[x, z];
                Signpost sign = treasureMap.SignpostAt(x, z);
                directionMarker.UpdateDistance(sign.DistanceToGoal);
                directionMarker.UpdateArrow(sign.DirToGoal, sign.OnCriticalPath);
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
            target = (wall != null) ? wall : hoveredErf.HighlightableTile();
        }
        highlighted?.Unhighlight();
        target?.Highlight(highlightColor);
        highlighted = target;
    }


    /// Currently assumes Walls have colliders off. Revisit if colliders turned on.
    private DirectionMarker RaycastForErf()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (!Physics.Raycast(ray, out RaycastHit hit, MaxRayDistance)) return null;
        return hit.collider.GetComponentInParent<DirectionMarker>();
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
