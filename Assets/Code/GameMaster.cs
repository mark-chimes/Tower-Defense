using System.Collections.Generic;
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
        TerrainSaveData terrain = treasureMap.GetSaveData();
        TowerSaveData towers = towerController.GetSaveData();
        LevelSaveData saveData = new LevelSaveData(treasureMap.NumRings, terrain, towers);
        saveLoadSystem.Save(saveData);
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
        Lattice<bool> landMap = new Lattice<bool>(layoutSpecs.NumRings);

        //** TEST terrain **//
        List<HexCoord> landCoords = new List<HexCoord>() {
            new HexCoord(-0, 0),
            new HexCoord(-1, 1),
            new HexCoord(-2, 2),
            new HexCoord(-3, 3),
        };

        foreach (HexCoord c in landCoords)
        {
            landMap.SetAt(c, true);
        }
        //** TEST terrain **//

        HexCoord spawnPos = layoutSpecs.SpawnCoord;
        HexCoord goalPos = layoutSpecs.GoalCoord;

        treasureMap = new TreasureMap(landMap, spawnPos, goalPos,
                StartingIsStopOnPathFound,
                enemyController.PathfindingUpdate,
                enemyController.PathfindingClear,
                flowBridge.OnTerrainChange
            );

        CreateMapFromTreasureMap(treasureMap, StartingVisualization);
    }

    void CreateMapFromData(LevelSaveData loaded)
    {
        Lattice<bool> loadedLand = loaded.Terrain.GetLandMap(loaded.NumRings);

        treasureMap = new TreasureMap(
            loadedLand,
            treasureMap.SpawnCoord,
            treasureMap.GoalCoord,
            StartingIsStopOnPathFound,
            enemyController.PathfindingUpdate,
            enemyController.PathfindingClear,
            flowBridge.OnTerrainChange
            );
        CreateMapFromTreasureMap(treasureMap, StartingVisualization);

        towerController.LoadFromSaveData(loaded.Towers);
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