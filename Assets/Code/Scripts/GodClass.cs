using UnityEngine;
using UnityEngine.InputSystem;

// TODO find better name for this class
public class GodClass : MonoBehaviour
{
    [SerializeField] private DebugGUI gui;
    [SerializeField] private EnemyController enemyController;


    [SerializeField] private GridView gridView;


    [SerializeField] private GridAuthor gridAuthor;
    private GridLayout layout;

    [SerializeField] private GridWalls gridWalls;

    // These just exist so we can have serialized editor settings on this class
    [SerializeField] public GridView.VisualizationSettings StartingVisualization;
    [SerializeField] public bool StartingIsStopOnPathFound = true;

    private TreasureMap treasureMap;


    GridMouseHighlightIO gridIO;
    GridPathfindingIOManager pathfindingIOManager;

    LevelSaveLoadSystem saveLoadSystem;

    void Start()
    {
        pathfindingIOManager = new GridPathfindingIOManager();
        saveLoadSystem = new LevelSaveLoadSystem();

        GenerateGrid();

        // TODO don't forget to update camera method if main camera can change
        gridIO = new GridMouseHighlightIO(gridWalls, treasureMap, Camera.main);

        enemyController.Initialize(layout, treasureMap);
        enemyController.SpawnEnemy();

        gui.Initialize(StartingVisualization, StartingIsStopOnPathFound, 
            pathfindingIOManager, enemyController, saveLoadSystem);

    }

    void Update()
    {
        gridIO.HandleMouse();
        pathfindingIOManager.ContinuallySingleStep();
        ControlCamera();
    }


    // TODO Camera controls should go to their own file eventually
    private Vector3 cameraStartPosition;
    private Quaternion cameraStartRotation;

    private const float cameraZoomSpeedBase = 300f;
    private const float cameraPanSpeedBase = 200f;

    private const float minCameraY = 10f;

    private float maxCameraY;

    void Awake()
    {
        cameraStartPosition = Camera.main.transform.position;
        cameraStartRotation = Camera.main.transform.rotation;
        maxCameraY = cameraStartPosition.y * 2;
    }

    void ControlCamera()
    {

        float cameraZoomSpeed = cameraZoomSpeedBase;
        float cameraPanSpeed = cameraPanSpeedBase;


        if (Keyboard.current == null) return; // TODO log error?

        var cameraTransform = Camera.main.transform;

        float cameraY = cameraTransform.position.y;
        float cameraZoomLevel = Mathf.InverseLerp(minCameraY, maxCameraY, cameraY);
        float speedFactor = Mathf.Max(cameraZoomLevel, 0.15f);
        cameraZoomSpeed *= speedFactor;
        cameraPanSpeed *= speedFactor;

        if (Keyboard.current.shiftKey.isPressed)
        {
            cameraZoomSpeed = cameraZoomSpeed * 4;
            cameraPanSpeed = cameraPanSpeed * 4;
        }


        if (cameraY > minCameraY && Keyboard.current.eKey.isPressed)
        {
            cameraTransform.position += cameraTransform.forward * cameraZoomSpeed * Time.deltaTime;
        }
        if (cameraY < maxCameraY && Keyboard.current.qKey.isPressed)
        {
            cameraTransform.position -= cameraTransform.forward * cameraZoomSpeed * Time.deltaTime;
        }


        if (Keyboard.current.aKey.isPressed)
        {
            cameraTransform.position -= cameraTransform.right * cameraPanSpeed * Time.deltaTime;
        }
        if (Keyboard.current.dKey.isPressed)
        {
            cameraTransform.position += cameraTransform.right * cameraPanSpeed * Time.deltaTime;
        }

        Vector3 groundForward = new Vector3(cameraTransform.forward.x, 0f, cameraTransform.forward.z).normalized;
        if (Keyboard.current.wKey.isPressed)
        {
            cameraTransform.position += groundForward * cameraPanSpeed * Time.deltaTime;
        }
        if (Keyboard.current.sKey.isPressed)
        {
            cameraTransform.position -= groundForward * cameraPanSpeed * Time.deltaTime;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log($"Camera position WAS {cameraTransform.position}");
            cameraTransform.position = cameraStartPosition;
        }

    }

    void OnDrawGizmos()
    {
        GridGizmo.Draw(gridAuthor, transform);
    }



    void GenerateGrid()
    {
        layout = gridAuthor.Layout;
        int width = layout.Width;
        int height = layout.Height;
        Coord spawnPos = gridAuthor.SpawnPos;
        Coord goalPos = gridAuthor.GoalPos;

        treasureMap = new TreasureMap(width, height, spawnPos, goalPos, StartingIsStopOnPathFound,
            enemyController.PathfindingUpdate, enemyController.PathfindingClear);

        gridView.GenerateGridView(layout, spawnPos, goalPos, StartingVisualization);
        pathfindingIOManager.Initialize(treasureMap, gridView);
        pathfindingIOManager.ClearField();
        gridWalls.Initialize(treasureMap, layout, pathfindingIOManager.UpdateDistances);
    }

    // TODO move this out? 
    public void OnSave() => saveLoadSystem.OnSave();
    public void OnLoad() => saveLoadSystem.OnLoad();


}
