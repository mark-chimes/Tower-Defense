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

    void Awake()
    {
        camControl = new CameraControl();
        camControl.Initialize();
    }

    void Start()
    {
        CreateMapWithWallsToTestWayfinder();
    }

    void Update()
    {
        camControl.ControlCamera();
    }

    void OnDrawGizmos()
    {
        HexGizmo.Draw(gridAuthor, hexMesh, transform, showSpawnAndGoal, isWire, diagonalColorMode, colorZeros, colorRGB);
    }


    void CreateMapFromNothing()
    {
        // TODO
    }

    void CreateMapWithWallsToTestWayfinder()
    {
        HexMap<bool> wallMap = new HexMap<bool>(gridAuthor.NumRings);
        HexCoord spawnCoord = gridAuthor.SpawnCoord;
        HexCoord goalCoord = gridAuthor.GoalCoord;

        // treasureMap = new TreasureMap(wallMap, spawnPos, goalPos, StartingIsStopOnPathFound,
        //     enemyController.PathfindingUpdate, enemyController.PathfindingClear);

        // CreateMapFromTreasureMap(treasureMap);

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


        HexWayfinder wayfinder = new HexWayfinder(wallMap, spawnCoord, goalCoord, HexSearch.Dir.FromEnd, true);
        wayfinder.ComputeFlow();
        Debug.Log($"wayfinder at spawn coord: {wayfinder.SignpostAt(spawnCoord)}");
        Debug.Log($"wayfinder away from goal: {wayfinder.SignpostAt(new HexCoord(-3, 0))}");

        Debug.Log($"wayfinder GoalCoord neighbors");
        foreach (HexCoord coord in goalCoord.Neighbours())
        {
            Debug.Log($"wayfinder at {coord} : {wayfinder.SignpostAt(new HexCoord(-3, 0))}");
        }

        CreateMapFromWallMap(wallMap);


    }

    void CreateMapFromWallMap(HexMap<bool> wallMap)
    {
        gridView.Initialize(wallMap);
    }
}