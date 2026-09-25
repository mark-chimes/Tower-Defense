using UnityEngine;

// TODO find better name for this class
public class HexGodClass : MonoBehaviour
{
    [SerializeField] private HexAuthor gridAuthor;
    [SerializeField] private HexGridView gridView;


    [SerializeField] private Mesh hexMesh;

    [SerializeField] private bool isWire;


    CameraControl camControl;

    void Awake()
    {
        camControl = new CameraControl();
        camControl.Initialize();
    }

    void Start()
    {
        CreateMapFromNothing();
    }

    void Update()
    {
        camControl.ControlCamera();
    }

    void OnDrawGizmos()
    {
        HexGizmo.Draw(gridAuthor, hexMesh, transform,isWire);
    }

    
    void CreateMapFromNothing()
    {
        HexMap<bool> wallMap = new HexMap<bool>(gridAuthor.NumRings);
        // Coord spawnPos = gridAuthor.SpawnPos;
        // Coord goalPos = gridAuthor.GoalPos;

        // treasureMap = new TreasureMap(wallMap, spawnPos, goalPos, StartingIsStopOnPathFound,
        //     enemyController.PathfindingUpdate, enemyController.PathfindingClear);

        // CreateMapFromTreasureMap(treasureMap);
        CreateMapFromWallMap(wallMap);
    }

    void CreateMapFromWallMap(HexMap<bool> wallMap)
    {
        gridView.Initialize(wallMap);
    }
}