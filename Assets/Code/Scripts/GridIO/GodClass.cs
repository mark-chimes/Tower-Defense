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
    GridPathfindingIOManager pathfindingIOManager;

    void Start()
    {
        pathfindingIOManager = new GridPathfindingIOManager();
        GenerateGrid();

        // TODO don't forget to update camera method if main camera can change
        gridIO = new GridMouseHighlightIO(gridWalls, treasureMap, Camera.main);
    }

    void Update()
    {
        gridIO.HandleMouse();
        pathfindingIOManager.ContinuallySingleStep();
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
        pathfindingIOManager.Initialize(treasureMap, gridView);
        pathfindingIOManager.ClearField();
        gridWalls.Initialize(treasureMap, layout, pathfindingIOManager.UpdateDistances);
    }

    public void OnRefreshPressed() => pathfindingIOManager.OnRefreshPressed();
    public void OnClearFieldPressed() => pathfindingIOManager.OnClearFieldPressed();
    public void OnSingleStepPressed() => pathfindingIOManager.OnSingleStepPressed();
    public void OnVisualizePressed() => pathfindingIOManager.OnVisualizePressed();
    public void OnFromStartModePressed() => pathfindingIOManager.OnFromStartModePressed();
    public void OnFromEndModePressed() => pathfindingIOManager.OnFromEndModePressed();
    public void SetNumbersVisible(bool isEnabled) => pathfindingIOManager.OnSetNumbersVisible(isEnabled);

    public void SetVisualizationVisible(bool isEnabled) => pathfindingIOManager.OnSetVisualizationVisible(isEnabled);

    public void SetAutoRefreshMode(bool isEnabled) => pathfindingIOManager.SetAutoRefreshMode(isEnabled);


}
