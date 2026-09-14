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


    [SerializeField] private GridAuthor gridAuthor;
    private GridLayout layout;

    [SerializeField] private bool isStopOnPathFound = true; // TODO this should be via debug buttons in-game

    [SerializeField] private GridWalls gridWalls;



    private TreasureMap treasureMap;


    GridMouseHighlightIO gridIO;
    GridPathfindingManager pathfindingManager;


    // TODO this is almost surely not the way to do this
    public GridPathfindingManager PathfindingManager => pathfindingManager;

    void Start()
    {
        pathfindingManager = new GridPathfindingManager();
        GenerateGrid();

        // TODO don't forget to update camera method if main camera can change
        gridIO = new GridMouseHighlightIO(gridWalls, treasureMap, Camera.main);
    }

    void Update()
    {
        gridIO.HandleMouse();
        pathfindingManager.ContinuallySingleStep();
    }

    void OnDrawGizmos()
    {
        GridGizmo.Draw(gridAuthor, transform);
    }

    void GenerateGrid()
    {
        layout = gridAuthor.Layout;
        int width = layout.Width;
        int height = layout.Height;
        Coord spawnPos = gridAuthor.SpawnPos;
        Coord goalPos = gridAuthor.GoalPos;

        treasureMap = new TreasureMap(width, height, spawnPos, goalPos, isStopOnPathFound);

        gridView.GenerateGridView(layout, spawnPos, goalPos);
        pathfindingManager.Initialize(treasureMap, gridView);
        pathfindingManager.ClearField();
        gridWalls.Initialize(treasureMap, layout, pathfindingManager);
    }


}
