using UnityEngine;
using UnityEngine.InputSystem;
using static DirectionMarker;


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

    [SerializeField] private Transform wallsParent;
    [SerializeField] private Wall wallPrefab;
    [SerializeField] private GameObject spawnPrefab;

    [SerializeField] private GameObject goalPrefab;
    [SerializeField] private GridAuthor gridAuthor;
    private GridLayout layout;

    [SerializeField] private Color placeableColor = Color.green;
    [SerializeField] private Color blockedColor = Color.red;
    [SerializeField] private Color existingWallColor = Color.yellow;


    [SerializeField] private float visualizeFPS = 60f;

    [SerializeField] private bool isStopOnPathFound = true; // TODO this should be via debug buttons in-game


    // TODO this is an ugly way of doing this - temp debug only
    [SerializeField] private bool shouldHighlight = false;

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
        layout = gridAuthor.Layout;
        int width = layout.Width;
        int height = layout.Height; 
        Coord spawnPos = gridAuthor.SpawnPos;
        Coord goalPos = gridAuthor.GoalPos;

        treasureMap = new TreasureMap(width, height, spawnPos, goalPos, isStopOnPathFound);

        directionMarkers = new DirectionMarker[width, height];
        walls = new Wall[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Coord coord = new Coord(x, z);

                DirectionMarker directionMarker = Instantiate(directionMarkerPrefab, transform);
                Vector3 pos = layout.CoordsToWorld(coord);
                directionMarker.transform.localPosition = pos;
                directionMarker.name = $"DirectionMarker_{x}_{z}";
                directionMarker.Initialize(coord);
                directionMarkers[x, z] = directionMarker;

                ErfSnapshot erf = treasureMap.At(coord);

                switch (erf.Kind)
                {
                    case SpawnGoalKind.Spawn: InstantiateMarker(spawnPrefab, coord); break;
                    case SpawnGoalKind.Goal: InstantiateMarker(goalPrefab, coord); break;
                }
            }
        }

        ClearField();
    }

    void InstantiateMarker(GameObject prefab, Coord coord)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.transform.localPosition = layout.CoordsToWorld(coord);
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
        isVisualizeMode = false;
        ClearField();
    }

    private void ClearField()
    {
        treasureMap.ClearField();
        RefreshDistanceLabels();
    }

    public void OnVisualizePressed()
    {
        ClearField();
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
        RefreshFromDeltaHighlightFrontier(delta);

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
                        directionMarker.ExhibitSignpost(sign);
                    }

                    if (shouldHighlight)
                    {
                        foreach (Coord c in treasureMap.CurrentFrontier())
                        {
                            DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
                            directionMarker.ExhibitAccent(ArrowAccent.Frontier);
                        }
                    }

                    break;
                }
            case Search.Phase.TracePath:
                {
                    foreach (Signpost sign in delta.Changed)
                    {
                        Coord c = sign.Coord;
                        DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
                        directionMarker.ExhibitAccent(ArrowAccent.Path);
                    }
                    break;
                }
            case Search.Phase.Done:
                {
                    isVisualizeMode = false;
                    tempTime = 0;
                    return;
                }
        }
    }

    private void UnhighlightAllArrows()
    {
        for (int x = 0; x < layout.Width; x++)
        {
            for (int z = 0; z < layout.Height; z++)
            {
                DirectionMarker directionMarker = directionMarkers[x, z];
                directionMarker.ExhibitAccent(ArrowAccent.Normal);
            }
        }

    }

    void RefreshDistanceLabels()
    {
        Debug.Log("RefreshDistanceLabels");

        foreach (Signpost sign in treasureMap.Signposts())
        {
            Coord c = sign.Coord;
            DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
            directionMarker.ExhibitSignpost(sign);
            directionMarker.ExhibitAccent(sign.OnCriticalPath ? ArrowAccent.Path : ArrowAccent.Normal);
        }
    }


    void OnDrawGizmos()
    {
        GridGizmo.Draw(gridAuthor, transform);
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
            if (erf.Kind != SpawnGoalKind.Floor)
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

        if (erf.Kind != SpawnGoalKind.Floor)
        {
            Debug.LogError($"SpawnWall: {c} Kind was {erf.Kind}");
            return;
        }

        Wall wall = Instantiate(wallPrefab, wallsParent);
        wall.transform.localPosition = layout.CoordsToWorld(c);
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

        if (erf.Kind != SpawnGoalKind.Floor)
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
