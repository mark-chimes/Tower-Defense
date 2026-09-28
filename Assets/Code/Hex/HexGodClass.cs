using UnityEngine;

// TODO find better name for this class
public class HexGodClass : MonoBehaviour
{
    [SerializeField] private HexAuthor gridAuthor;
    [SerializeField] private HexGridView gridView;

    [SerializeField] HexGUI gui;
    [SerializeField] private HexEnemyController enemyController;

    [SerializeField] HexGridView.VisualizationSettings StartingVisualization;
    [SerializeField] bool StartingIsStopOnPathFound;


    [SerializeField] private Mesh hexMesh;

    [SerializeField] private bool isWire = false;
    [SerializeField] private bool showSpawnAndGoal = true;

    [SerializeField] private HexGizmo.DiagonalColorMode diagonalColorMode = HexGizmo.DiagonalColorMode.POSITIVE;
    [SerializeField] private bool colorZeros = true;
    [SerializeField] private bool colorRGB = false;

    CameraControl camControl;
    HexMouseIO mouseIO;

    HexPathfindingIOManager pathfindingIOManager;

    HexLevelSaveLoadSystem saveLoadSystem;

    void Awake()
    {
        camControl = new CameraControl();
        camControl.Initialize();

        pathfindingIOManager = new HexPathfindingIOManager();
        saveLoadSystem = new HexLevelSaveLoadSystem();
    }

    void Start()
    {
        CreateMapFromNothing();
    }

    void Update()
    {
        camControl.ControlCamera();
        mouseIO.HandleMouse();
        pathfindingIOManager.ContinuallySingleStep();
    }

    void OnDrawGizmos()
    {
        HexGizmo.Draw(gridAuthor, hexMesh, transform, showSpawnAndGoal, isWire, diagonalColorMode, colorZeros, colorRGB);
    }

    public void OnSave()
    {
        saveLoadSystem.SaveMap(treasureMap);
    }

    public void OnLoad()
    {
        HexSaveableLevel loaded = saveLoadSystem.OnLoad();

        gridView.ClearData();
        enemyController.ClearData();

        CreateMapFromData(loaded);
    }

    // void CreateMapFromWallMap(HexMap<bool> wallMap, HexCoord spawnCoord, HexCoord goalCoord)
    // {
    //     gridView.Initialize(wallMap, spawnCoord, goalCoord);
    // }


    private HexTreasureMap treasureMap;

    void CreateMapFromNothing()
    {
        HexMap<bool> wallMap = new HexMap<bool>(gridAuthor.NumRings);

        //** TEST wall positions **//
        HexCoord wallPos0 = new HexCoord(-0, 0);
        wallMap.SetAt(wallPos0, true);
        HexCoord wallPos1 = new HexCoord(-1, 1);
        wallMap.SetAt(wallPos1, true);
        HexCoord wallPos2 = new HexCoord(-2, 2);
        wallMap.SetAt(wallPos2, true);
        HexCoord wallPos3 = new HexCoord(-3, 3);
        wallMap.SetAt(wallPos3, true);
        //** TEST wall positions **//

        HexCoord spawnPos = gridAuthor.SpawnCoord;
        HexCoord goalPos = gridAuthor.GoalCoord;

        treasureMap = new HexTreasureMap(wallMap, spawnPos, goalPos,
                StartingIsStopOnPathFound,
                enemyController.PathfindingUpdate,
                enemyController.PathfindingClear
            );

        CreateMapFromTreasureMap(treasureMap, wallMap, StartingVisualization);
    }

    void CreateMapFromData(HexSaveableLevel loaded)
    {
        HexMap<bool> loadedMap = loaded.LoadMap();

        treasureMap = new HexTreasureMap(loadedMap,
            treasureMap.SpawnPos,
            treasureMap.GoalPos,
            StartingIsStopOnPathFound,
            enemyController.PathfindingUpdate,
            enemyController.PathfindingClear
            );
        CreateMapFromTreasureMap(treasureMap, loadedMap, StartingVisualization);
    }

    void CreateMapFromTreasureMap(HexTreasureMap treasureMap,
        HexMap<bool> wallMap,
        HexGridView.VisualizationSettings visualizationSettings)
    {
        gridView.Initialize(wallMap, treasureMap.SpawnPos, treasureMap.GoalPos, visualizationSettings);
        pathfindingIOManager.Initialize(treasureMap, gridView);

        // TODO

        // gridWalls.Initialize(treasureMap, layout, pathfindingIOManager.UpdateDistances);
        // gridIO = new GridMouseHighlightIO(gridWalls, treasureMap, Camera.main);

        enemyController.Initialize(treasureMap);
        enemyController.SpawnEnemy();

        gui.Initialize(StartingVisualization, StartingIsStopOnPathFound, pathfindingIOManager, enemyController, this);
        //     , enemyController, this, gridWalls); // TODO cross-dependency code-smell
        mouseIO = new HexMouseIO(gridView, treasureMap, Camera.main, pathfindingIOManager.UpdateDistances);
    }
}