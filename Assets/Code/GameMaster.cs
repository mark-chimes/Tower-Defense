using UnityEngine;

public class GameMaster : MonoBehaviour
{
    [SerializeField] private LayoutSpecs layoutSpecs;
    [SerializeField] private TerrainWithOverlay gridView;

    [SerializeField] PathfindingGUI gui;
    [SerializeField] private EnemyController enemyController;

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

        CreateMapFromData(loaded);
    }

    private TreasureMap treasureMap;

    // TODO rename this
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
                enemyController.PathfindingClear
            );

        CreateMapFromTreasureMap(treasureMap, wallMap, StartingVisualization);
    }

    void CreateMapFromData(LevelSaveData loaded)
    {
        Lattice<bool> loadedWallMap = loaded.ToWallMap();

        treasureMap = new TreasureMap(
            loadedWallMap,
            treasureMap.SpawnPos,
            treasureMap.GoalPos,
            StartingIsStopOnPathFound,
            enemyController.PathfindingUpdate,
            enemyController.PathfindingClear
            );
        CreateMapFromTreasureMap(treasureMap, loadedWallMap, StartingVisualization);
    }

    void CreateMapFromTreasureMap(TreasureMap treasureMap,
        Lattice<bool> wallMap,
        TerrainWithOverlay.VisualizationSettings visualizationSettings)
    {
        gridView.Initialize(wallMap, treasureMap.SpawnPos, treasureMap.GoalPos, visualizationSettings);
        flowBridge.Initialize(treasureMap, gridView);

        enemyController.Initialize(treasureMap);
        enemyController.SpawnEnemy();

        gui.Initialize(StartingVisualization, StartingIsStopOnPathFound, flowBridge, enemyController, this);
        boardInput = new BoardInput(gridView, treasureMap, Camera.main, flowBridge.OnWallsChanged);
    }
}