using UnityEngine;

// TODO find better name for this class
public class HexGodClass : MonoBehaviour
{
    [SerializeField] private HexAuthor gridAuthor;
    [SerializeField] private HexGridView gridView;


    [SerializeField] private Mesh hexMesh;

    [SerializeField] private bool isWire = false;
    [SerializeField] private bool isDrawingColors = true; 


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
        HexGizmo.Draw(gridAuthor, hexMesh, transform, isWire, isDrawingColors);
    }

    
    void CreateMapFromNothing()
    {
        HexMap<bool> wallMap = new HexMap<bool>(gridAuthor.NumRings);
        // Coord spawnPos = gridAuthor.SpawnPos;
        // Coord goalPos = gridAuthor.GoalPos;

        // treasureMap = new TreasureMap(wallMap, spawnPos, goalPos, StartingIsStopOnPathFound,
        //     enemyController.PathfindingUpdate, enemyController.PathfindingClear);

        // CreateMapFromTreasureMap(treasureMap);

        //** TEST wall positions **//
        HexCoord wallPos1 = new HexCoord(0,1);
        wallMap.SetAt(wallPos1, true);
        HexCoord wallPos2 = new HexCoord(1,0);
        wallMap.SetAt(wallPos2, true);
        //** TEST wall positions **//

        CreateMapFromWallMap(wallMap);
    }

    void CreateMapFromWallMap(HexMap<bool> wallMap)
    {
        gridView.Initialize(wallMap);
    }
}