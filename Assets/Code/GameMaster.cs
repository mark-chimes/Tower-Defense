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

    Fleet fleet;


    [SerializeField] private float stepSize = 0.02f;

    [SerializeField] private int maxSteps = 5;

    private bool isSimRunning = true;
    private float accumulator = 0;

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
        UpdateEnemies();
    }


    void UpdateEnemies()
    {
        if (fleet == null) return;

        if (isSimRunning)
        {
            accumulator += Time.deltaTime;
            int numSteps = 0;
            while (accumulator >= stepSize && numSteps < maxSteps)
            {
                fleet.StepDt(stepSize);
                accumulator -= stepSize;
                numSteps++;
            }
            if (numSteps == maxSteps) accumulator = 0;
        }
        enemyController.SyncBoats();
    }

    void OnDrawGizmos()
    {
        Gizmo.Draw(layoutSpecs, hexMesh, transform, showSpawnAndGoal, isWire, diagonalColorMode, colorZeros, colorRGB);
    }

    private void Save()
    {
        TerrainSaveData terrain = treasureMap.GetSaveData();
        TowerSaveData towers = towerController.GetSaveData();
        LevelSaveData saveData = new LevelSaveData(treasureMap.NumRings, terrain, towers);
        saveLoadSystem.Save(saveData);
    }

    private void Load()
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

        flowBridge.RefreshIfAutoRefresh();

        // >>> TODO For testing <<<
        int capacity = 100;
        fleet = new Fleet(treasureMap, capacity);
        // >>> ^^^ <<<

        enemyController.Initialize(treasureMap, fleet);
        towerController.Initialize(treasureMap);
        enemyController.SpawnEnemy();

        boardInput = new BoardInput(treasureMap, Camera.main, towerController);
        gui.Initialize(StartingVisualization, StartingIsStopOnPathFound,
            boardInput, flowBridge, enemyController, towerController, Save, Load, () => isSimRunning, b => isSimRunning=b);
    }
}