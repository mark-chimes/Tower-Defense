using UnityEngine;

// TODO find better name for this class
public class GodClass : MonoBehaviour
{
    [SerializeField] private DebugGUI gui;
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private GridView gridView;
    [SerializeField] private GridWalls gridWalls;

    [SerializeField] private GridAuthor gridAuthor;

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
        camControl.Initialize();

        pathfindingIOManager = new GridPathfindingIOManager();
        saveLoadSystem = new LevelSaveLoadSystem();
    }

    void Start()
    {
        CreateMapFromNothing();
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

    public void OnSave()
    {
        saveLoadSystem.SaveMap(treasureMap);
    }

    public void OnLoad()
    {
        SaveableLevel loaded = saveLoadSystem.OnLoad();

        gridView.ClearData();
        gridWalls.ClearData();
        enemyController.ClearData();

        CreateMapFromData(loaded);
    }

    void CreateMapFromNothing()
    {
        int width = gridAuthor.Layout.Width;
        int height = gridAuthor.Layout.Height;
        WallMap wallMap = new WallMap(width, height);
        Coord spawnPos = gridAuthor.SpawnPos;
        Coord goalPos = gridAuthor.GoalPos;

        treasureMap = new TreasureMap(width, height, wallMap, spawnPos, goalPos, StartingIsStopOnPathFound,
            enemyController.PathfindingUpdate, enemyController.PathfindingClear);

        CreateMapFromTreasureMap(treasureMap);
    }

    void CreateMapFromData(SaveableLevel loaded)
    {
        WallMap loadedMap = loaded.LoadMap();

        treasureMap = new TreasureMap(loaded.Width, loaded.Height, loadedMap, 
            treasureMap.SpawnPos, 
            treasureMap.GoalPos,
            StartingIsStopOnPathFound,
            enemyController.PathfindingUpdate,
            enemyController.PathfindingClear);
        CreateMapFromTreasureMap(treasureMap);
    }

    void CreateMapFromTreasureMap(TreasureMap treasureMap)
    {
        GridLayout layout = new GridLayout(treasureMap.Width, treasureMap.Height);
        gridView.Initialize(layout, treasureMap.SpawnPos, treasureMap.GoalPos, StartingVisualization);
        pathfindingIOManager.Initialize(treasureMap, gridView);
        pathfindingIOManager.ClearField();
        gridWalls.Initialize(treasureMap, layout, pathfindingIOManager.UpdateDistances);

        gridIO = new GridMouseHighlightIO(gridWalls, treasureMap, Camera.main);

        enemyController.Initialize(layout, treasureMap);
        enemyController.SpawnEnemy();

        gui.Initialize(StartingVisualization, StartingIsStopOnPathFound,
            pathfindingIOManager, enemyController, this, gridWalls); // TODO cross-dependency code-smell
    }


}
