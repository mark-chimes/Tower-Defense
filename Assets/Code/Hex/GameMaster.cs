using UnityEngine;

public class GameMaster : MonoBehaviour
{
    [SerializeField] private LayoutSpecs gridAuthor;
    [SerializeField] private TerrainWithOverlay gridView;

    [SerializeField] PathfindingGUI gui;
    [SerializeField] private EnemyController enemyController;

    [SerializeField] TerrainWithOverlay.VisualizationSettings StartingVisualization;
    [SerializeField] bool StartingIsStopOnPathFound;


    [SerializeField] private Mesh hexMesh;

    [SerializeField] private bool isWire = false;
    [SerializeField] private bool showSpawnAndGoal = true;

    [SerializeField] private Gizmo.DiagonalColorMode diagonalColorMode = Gizmo.DiagonalColorMode.POSITIVE;
    [SerializeField] private bool colorZeros = true;
    [SerializeField] private bool colorRGB = false;

    CameraRig camControl;
    BoardInput mouseIO;

    FlowBridge pathfindingIOManager;

    HexLevelSaveLoadSystem saveLoadSystem;

    void Awake()
    {
        camControl = new CameraRig();
        camControl.Initialize();

        pathfindingIOManager = new FlowBridge();
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
        Gizmo.Draw(gridAuthor, hexMesh, transform, showSpawnAndGoal, isWire, diagonalColorMode, colorZeros, colorRGB);
    }

    public void OnSave()
    {
        saveLoadSystem.SaveMap(treasureMap);
    }

    public void OnLoad()
    {
        LevelSaveData loaded = saveLoadSystem.OnLoad();

        gridView.ClearData();
        enemyController.ClearData();

        CreateMapFromData(loaded);
    }

    private TreasureMap treasureMap;

    // TODO rename this
    void CreateMapFromNothing()
    {
        Lattice<bool> wallMap = new Lattice<bool>(gridAuthor.NumRings);

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

        treasureMap = new TreasureMap(wallMap, spawnPos, goalPos,
                StartingIsStopOnPathFound,
                enemyController.PathfindingUpdate,
                enemyController.PathfindingClear
            );

        CreateMapFromTreasureMap(treasureMap, wallMap, StartingVisualization);
    }

    void CreateMapFromData(LevelSaveData loaded)
    {
        Lattice<bool> loadedMap = loaded.LoadMap();

        treasureMap = new TreasureMap(loadedMap,
            treasureMap.SpawnPos,
            treasureMap.GoalPos,
            StartingIsStopOnPathFound,
            enemyController.PathfindingUpdate,
            enemyController.PathfindingClear
            );
        CreateMapFromTreasureMap(treasureMap, loadedMap, StartingVisualization);
    }

    void CreateMapFromTreasureMap(TreasureMap treasureMap,
        Lattice<bool> wallMap,
        TerrainWithOverlay.VisualizationSettings visualizationSettings)
    {
        gridView.Initialize(wallMap, treasureMap.SpawnPos, treasureMap.GoalPos, visualizationSettings);
        pathfindingIOManager.Initialize(treasureMap, gridView);

        enemyController.Initialize(treasureMap);
        enemyController.SpawnEnemy();

        gui.Initialize(StartingVisualization, StartingIsStopOnPathFound, pathfindingIOManager, enemyController, this);
        mouseIO = new BoardInput(gridView, treasureMap, Camera.main, pathfindingIOManager.UpdateDistances);
    }
}