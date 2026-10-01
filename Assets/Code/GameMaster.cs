using UnityEngine;

public class GameMaster : MonoBehaviour
{
    [SerializeField] private LayoutSpecs layoutSpecs;
    [SerializeField] private TerrainWithOverlay gridView;

    [SerializeField] DebugGUI gui;
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private TowerController towerController;

    [SerializeField] TerrainWithOverlay.VisualizationSettings StartingVisualization;
    [SerializeField] bool StartingIsStopOnPathFound;


    [SerializeField] private Mesh hexMesh;

    [SerializeField] private bool isWire = false;
    [SerializeField] private bool showSpawnAndGoal = true;

    [SerializeField] private Gizmo.DiagonalColorMode diagonalColorMode = Gizmo.DiagonalColorMode.Positive;
    [SerializeField] private bool colorZeros = true;
    [SerializeField] private bool colorRGB = false;

    CameraRig camRig;
    BoardInput boardInput;

    FlowBridge flowBridge;

    HexLevelSaveLoadSystem saveLoadSystem;

    void Awake()
    {
        camRig = new CameraRig();
        camRig.Initialize();

        flowBridge = new FlowBridge();
        saveLoadSystem = new HexLevelSaveLoadSystem();
    }

    void Start()
    {
        CreateTestMap();
    }

    void Update()
    {
        camRig.ControlCamera();
        boardInput.HandleMouse();
        flowBridge.ContinuallySingleStep();
    }

    void OnDrawGizmos()
    {
        Gizmo.Draw(layoutSpecs, hexMesh, transform, showSpawnAndGoal, isWire, diagonalColorMode, colorZeros, colorRGB);
    }

    public void OnSave()
    {
        saveLoadSystem.Save(treasureMap);
    }

    public void OnLoad()
    {
        LevelSaveData loaded = saveLoadSystem.Load();

        gridView.ClearData();
        enemyController.ClearData();
        towerController.ClearData();

        CreateMapFromData(loaded);
    }

    private TreasureMap treasureMap;

    void CreateTestMap()
    {
        Lattice<bool> wallMap = new Lattice<bool>(layoutSpecs.NumRings);

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

        HexCoord spawnPos = layoutSpecs.SpawnCoord;
        HexCoord goalPos = layoutSpecs.GoalCoord;

        treasureMap = new TreasureMap(wallMap, spawnPos, goalPos,
                StartingIsStopOnPathFound,
                enemyController.PathfindingUpdate,
                enemyController.PathfindingClear,
                flowBridge.OnWallChange
            );

        CreateMapFromTreasureMap(treasureMap, StartingVisualization);
    }

    void CreateMapFromData(LevelSaveData loaded)
    {
        Lattice<bool> loadedWallMap = loaded.ToWallMap();

        treasureMap = new TreasureMap(
            loadedWallMap,
            treasureMap.SpawnCoord,
            treasureMap.GoalCoord,
            StartingIsStopOnPathFound,
            enemyController.PathfindingUpdate,
            enemyController.PathfindingClear,
            flowBridge.OnWallChange
            );
        CreateMapFromTreasureMap(treasureMap, StartingVisualization);
    }

    void CreateMapFromTreasureMap(TreasureMap treasureMap,
        TerrainWithOverlay.VisualizationSettings visualizationSettings)
    {
        gridView.Initialize(treasureMap, visualizationSettings);
        flowBridge.Initialize(treasureMap, gridView);
        enemyController.Initialize(treasureMap);
        towerController.Initialize(treasureMap);

        enemyController.SpawnEnemy();
        flowBridge.RefreshIfAutoRefresh();

        boardInput = new BoardInput(treasureMap, Camera.main, towerController);
        gui.Initialize(StartingVisualization, StartingIsStopOnPathFound,
            boardInput, flowBridge, enemyController, towerController, this);
    }
}