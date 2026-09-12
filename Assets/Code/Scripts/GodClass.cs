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
    [SerializeField] private GridView gridView;

    [SerializeField] private Transform wallsParent;
    [SerializeField] private Wall wallPrefab;

    [SerializeField] private GridAuthor gridAuthor;
    private GridLayout layout;

    [SerializeField] private float visualizeFPS = 60f;

    [SerializeField] private bool isStopOnPathFound = true; // TODO this should be via debug buttons in-game


    // TODO this is an ugly way of doing this - temp debug only
    [SerializeField] private bool shouldHighlight = false;

    private float visualizeTime;


    private TreasureMap treasureMap;

    private Wall[,] walls;

    private bool isAutoRefreshMode = false;
    private bool isVisualizeMode = false;

    GridMouseHighlightIO gridIO;

    void Start()
    {
        visualizeTime = 1f / visualizeFPS;
        GenerateGrid();

        GridMouseHighlightIO.IWallHandler wallHandler = new WallHandler(this);
        // TODO don't forget to update camera method if main camera can change
        gridIO = new GridMouseHighlightIO(wallHandler, treasureMap, Camera.main);
    }

    void Update()
    {
        gridIO.HandleMouse();
        ContinuallySingleStep();
    }

    // TODO temporary implementation class during refactoring
    private class WallHandler : GridMouseHighlightIO.IWallHandler
    {
        private readonly GodClass godClass;
        public WallHandler(GodClass godClass) { this.godClass = godClass; }

        void GridMouseHighlightIO.IWallHandler.DespawnWall(Coord c)
        {
            godClass.DespawnWall(c);
        }

        IHighlightable GridMouseHighlightIO.IWallHandler.MaybeWall(Coord c)
        {
            return godClass.walls[c.X, c.Z];
        }

        void GridMouseHighlightIO.IWallHandler.SpawnWall(Coord c)
        {
            godClass.SpawnWall(c);
        }
    }



    void GenerateGrid()
    {
        layout = gridAuthor.Layout;
        int width = layout.Width;
        int height = layout.Height;
        Coord spawnPos = gridAuthor.SpawnPos;
        Coord goalPos = gridAuthor.GoalPos;

        treasureMap = new TreasureMap(width, height, spawnPos, goalPos, isStopOnPathFound);
        walls = new Wall[width, height];

        gridView.GenerateGridView(layout, spawnPos, goalPos);
        ClearField();
    }

    /** Slow pathfinding and refresh code **/

    public void SetAutoRefreshMode(bool isEnabled)
    {
        isAutoRefreshMode = isEnabled;
        if (isAutoRefreshMode)
        {
            isVisualizeMode = false;
            treasureMap.Recompute();
            gridView.RefreshDistanceLabels(treasureMap.Signposts());
        }
    }

    public void OnFromStartModePressed()
    {
        isVisualizeMode = false;
        treasureMap.SetModeAndClear(Search.Dir.FromStart);
        gridView.RefreshDistanceLabels(treasureMap.Signposts());
    }

    public void OnFromEndModePressed()
    {
        isVisualizeMode = false;
        treasureMap.SetModeAndClear(Search.Dir.FromEnd);
        gridView.RefreshDistanceLabels(treasureMap.Signposts());
    }

    public void OnRefreshPressed()
    {
        treasureMap.Recompute();
        gridView.RefreshDistanceLabels(treasureMap.Signposts());
    }

    public void OnClearFieldPressed()
    {
        isVisualizeMode = false;
        ClearField();
    }

    private void ClearField()
    {
        treasureMap.ClearField();
        gridView.RefreshDistanceLabels(treasureMap.Signposts());
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
        gridView.RefreshDistanceLabels(treasureMap.Signposts());
    }


    void RefreshFromDeltaHighlightFrontier(Search.Delta delta)
    {
        Debug.Log("Refresh from delta");


        switch (delta.Phase)
        {
            case Search.Phase.ExpandFrontier:
                {
                    gridView.HighlightChangedOrFrontier(shouldHighlight, delta.Changed, treasureMap.CurrentFrontier());
                    break;
                }
            case Search.Phase.TracePath:
                {
                    gridView.HighlightPath(delta.Changed);
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

    void OnDrawGizmos()
    {
        GridGizmo.Draw(gridAuthor, transform);
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
        gridIO.ClearHighlightIfMatching(wall);
        Destroy(wall.gameObject);
        treasureMap.SetWall(c, false);
        UpdateDistances();
    }
}
