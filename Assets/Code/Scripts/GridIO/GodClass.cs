using UnityEngine;
using UnityEngine.InputSystem;
using static DirectionMarker;


// TODO find better name for this class
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
        SpawnEnemy(layout, treasureMap.SpawnPos, treasureMap.GoalPos);

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


    // TODO is this the best way to implement / group these methods? Feel like there must be a better way
    public void OnRefreshPressed() => pathfindingIOManager.OnRefreshPressed();
    public void OnClearFieldPressed() => pathfindingIOManager.OnClearFieldPressed();
    public void OnSingleStepPressed() => pathfindingIOManager.OnSingleStepPressed();
    public void OnVisualizePressed() => pathfindingIOManager.OnVisualizePressed();
    public void OnFromStartModePressed() => pathfindingIOManager.OnFromStartModePressed();
    public void OnFromEndModePressed() => pathfindingIOManager.OnFromEndModePressed();
    public void SetNumbersVisible(bool isEnabled) => pathfindingIOManager.OnSetNumbersVisible(isEnabled);
    public void SetVisualizationVisible(bool isEnabled) => pathfindingIOManager.OnSetVisualizationVisible(isEnabled);
    public void SetAutoRefreshMode(bool isEnabled) => pathfindingIOManager.SetAutoRefreshMode(isEnabled);

    // TODO enemy spawn logic below to get it going. Move out once a home is found. 
    // Maybe some of it will form part of this

    [SerializeField] private Boat boatPrefab;
    [SerializeField] private EnemyController enemyController; // TODO move code herea and use this

    // spawn a single enemy, just to test it out.
    // passing in parameters in prep for moving this function out
    void SpawnEnemy(GridLayout layout, Coord spawnPos, Coord goalPos)
    {
        Boat enemy = Instantiate(boatPrefab, transform);
        Vector3 pos = layout.CoordsToWorld(spawnPos);
        enemy.transform.localPosition = pos;
        enemy.name = $"Boat";
        enemy.Initialize(spawnPos, goalPos);
        // TODO save enemies in a list
    }

}
