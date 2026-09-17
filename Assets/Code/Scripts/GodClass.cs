using UnityEngine;
using UnityEngine.InputSystem;

// TODO find better name for this class
public class GodClass : MonoBehaviour
{
    [SerializeField] private DebugGUI gui;
    [SerializeField] private EnemyController enemyController;


    [SerializeField] private GridView gridView;


    [SerializeField] private GridAuthor gridAuthor;
    private GridLayout layout;

    [SerializeField] private GridWalls gridWalls;

    // These just exist so we can have serialized editor settings on this class
    [SerializeField] public GridView.VisualizationSettings StartingVisualization;
    [SerializeField] public bool StartingIsStopOnPathFound = true;

    private TreasureMap treasureMap;


    GridMouseHighlightIO gridIO;
    GridPathfindingIOManager pathfindingIOManager;

    LevelSaveLoadSystem saveLoadSystem;

    CameraControl camControl;

    void Awake()
    {
        camControl = new CameraControl();
        pathfindingIOManager = new GridPathfindingIOManager();
        saveLoadSystem = new LevelSaveLoadSystem();
    }

    void Start()
    {
        GenerateGrid();

        // TODO should gridIO and camControl be coupled? 
        gridIO = new GridMouseHighlightIO(gridWalls, treasureMap, Camera.main);

        enemyController.Initialize(layout, treasureMap);
        enemyController.SpawnEnemy();

        gui.Initialize(StartingVisualization, StartingIsStopOnPathFound,
            pathfindingIOManager, enemyController, saveLoadSystem);

        camControl.Initialize();

    }

    void Update()
    {
        gridIO.HandleMouse();
        pathfindingIOManager.ContinuallySingleStep();
        camControl.ControlCamera();
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

        treasureMap = new TreasureMap(width, height, spawnPos, goalPos, StartingIsStopOnPathFound,
            enemyController.PathfindingUpdate, enemyController.PathfindingClear);

        gridView.GenerateGridView(layout, spawnPos, goalPos, StartingVisualization);
        pathfindingIOManager.Initialize(treasureMap, gridView);
        pathfindingIOManager.ClearField();
        gridWalls.Initialize(treasureMap, layout, pathfindingIOManager.UpdateDistances);
    }

    // TODO move this out? 
    public void OnSave() => saveLoadSystem.OnSave();
    public void OnLoad() => saveLoadSystem.OnLoad();


}
