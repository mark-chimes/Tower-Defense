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

    [SerializeField] private bool isStopOnPathFound = true; // TODO this should be via debug buttons in-game




    private TreasureMap treasureMap;

    private Wall[,] walls;



    GridMouseHighlightIO gridIO;
    GridPathfindingManager pathfindingManager;

    // TODO this is almost surely not the way to do this
    public GridPathfindingManager PathfindingManager => pathfindingManager;

    void Start()
    {
        pathfindingManager = new GridPathfindingManager();
        GenerateGrid();

        GridMouseHighlightIO.IWallHandler wallHandler = new WallHandler(this);
        // TODO don't forget to update camera method if main camera can change
        gridIO = new GridMouseHighlightIO(wallHandler, treasureMap, Camera.main);
    }

    void Update()
    {
        gridIO.HandleMouse();
        pathfindingManager.ContinuallySingleStep();
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
        pathfindingManager.Initialize(treasureMap, gridView);
        pathfindingManager.ClearField();
    }

    /** Slow pathfinding and refresh code **/


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
        pathfindingManager.UpdateDistances();
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
        pathfindingManager.UpdateDistances();
    }
}
