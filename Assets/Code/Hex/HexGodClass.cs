using UnityEngine;

// TODO find better name for this class
public class HexGodClass : MonoBehaviour
{
    [SerializeField] private HexAuthor gridAuthor;
    [SerializeField] private HexGridView gridView;


    [SerializeField] private Mesh hexMesh;

    [SerializeField] private bool isWire = false;
    [SerializeField] private bool showSpawnAndGoal = true;

    [SerializeField] private HexGizmo.DiagonalColorMode diagonalColorMode = HexGizmo.DiagonalColorMode.POSITIVE;
    [SerializeField] private bool colorZeros = true;
    [SerializeField] private bool colorRGB = false;

    CameraControl camControl;
    HexMouseIO mouseIO;

    HexPathfindingIOManager pathfindingIOManager;

    void Awake()
    {
        camControl = new CameraControl();
        camControl.Initialize();

        pathfindingIOManager = new HexPathfindingIOManager();
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

    void CreateMapFromWallMap(HexMap<bool> wallMap, HexCoord spawnCoord, HexCoord goalCoord)
    {
        gridView.Initialize(wallMap, spawnCoord, goalCoord);
    }


    private HexTreasureMap treasureMap;

    // TODO remove this once it is unused
    void PATHFINDING_UPDATE_PLACEHOLDER()
    {
        //
    }

    void PATHFINDING_CLEAR_PLACEHOLDER()
    {
        //
    }


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
            true, // TODO
                PATHFINDING_UPDATE_PLACEHOLDER, PATHFINDING_CLEAR_PLACEHOLDER// TODO enemyController.PathfindingUpdate, enemyController.PathfindingClear
            );

        CreateMapFromTreasureMap(treasureMap, wallMap);
    }

    // void CreateMapFromData(SaveableLevel loaded)
    // {
    //     RectMap<bool> loadedMap = loaded.LoadMap();

    //     treasureMap = new TreasureMap(loadedMap,
    //         treasureMap.SpawnPos,
    //         treasureMap.GoalPos,
    //         StartingIsStopOnPathFound,
    //         enemyController.PathfindingUpdate,
    //         enemyController.PathfindingClear);
    //     CreateMapFromTreasureMap(treasureMap);
    // }

    void CreateMapFromTreasureMap(HexTreasureMap treasureMap, HexMap<bool> wallMap)
    {
        gridView.Initialize(wallMap, treasureMap.SpawnPos, treasureMap.GoalPos);
        pathfindingIOManager.Initialize(treasureMap, gridView);

        pathfindingIOManager.SetAutoRefreshMode(true); // TODO fix this when debug gui added

        // TODO

        // gridWalls.Initialize(treasureMap, layout, pathfindingIOManager.UpdateDistances);
        // gridIO = new GridMouseHighlightIO(gridWalls, treasureMap, Camera.main);

        // enemyController.Initialize(layout, treasureMap);
        // enemyController.SpawnEnemy();

        // gui.Initialize(StartingVisualization, StartingIsStopOnPathFound,
        //     pathfindingIOManager, enemyController, this, gridWalls); // TODO cross-dependency code-smell
        mouseIO = new HexMouseIO(gridView, treasureMap, Camera.main);
    }
}