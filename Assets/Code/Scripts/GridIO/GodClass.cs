using UnityEngine;
using UnityEngine.InputSystem;

// TODO find better name for this class
public class GodClass : MonoBehaviour
{
    [SerializeField] private GridView gridView;


    [SerializeField] private GridAuthor gridAuthor;
    private GridLayout layout;

    [SerializeField] private bool isStopOnPathFound = true; // TODO this should be via debug buttons in-game

    [SerializeField] private GridWalls gridWalls;



    private TreasureMap treasureMap;


    GridMouseHighlightIO gridIO;
    GridPathfindingIOManager pathfindingIOManager;

    void Start()
    {
        pathfindingIOManager = new GridPathfindingIOManager();
        GenerateGrid();
        SpawnEnemy(layout, treasureMap.SpawnPos, treasureMap.GoalPos);

        // TODO don't forget to update camera method if main camera can change
        gridIO = new GridMouseHighlightIO(gridWalls, treasureMap, Camera.main);
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

    private const float cameraZoomSpeedBase = 100f;
    private const float cameraPanSpeedBase = 100f;

    private const float minCameraY = 10f;   

    private float maxCameraY;

    void Awake()
    {
        cameraStartPosition = Camera.main.transform.position;
        cameraStartRotation = Camera.main.transform.rotation;
        maxCameraY = cameraStartPosition.y;
    }

    void ControlCamera()
    {

        float cameraZoomSpeed = cameraZoomSpeedBase;
        float cameraPanSpeed = cameraPanSpeedBase;
        // TODO adjust cameraZoomSpeed and cameraPanSpeed based off of zoom level.
        // TODO adjust cameraZoomLevel based on how far camera is from max zoom somehow.


        if (Keyboard.current == null) return; // TODO log error?
        
        var cameraTransform = Camera.main.transform;

        float cameraY = cameraTransform.position.y;
        float cameraZoomLevel = Mathf.InverseLerp(minCameraY, maxCameraY, cameraY);
        float speedFactor = Mathf.Max(cameraZoomLevel, 0.15f);
        cameraZoomSpeed *= speedFactor;
        cameraPanSpeed *= speedFactor;
        
        if (Keyboard.current.shiftKey.isPressed)
        {
            cameraZoomSpeed = cameraZoomSpeed * 2;
            cameraPanSpeed = cameraPanSpeed * 2;
        }

        if (Keyboard.current.eKey.isPressed)
        {
            cameraTransform.position += cameraTransform.forward * cameraZoomSpeed * Time.deltaTime;
        }
        if (Keyboard.current.qKey.isPressed)
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
            // cameraTransform.rotation = cameraStartRotation; // revisit if anything affects rotation
            // cameraZoomLevel = 1f;
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

        treasureMap = new TreasureMap(width, height, spawnPos, goalPos, isStopOnPathFound, PathfindingUpdate, PathfindingClear);

        gridView.GenerateGridView(layout, spawnPos, goalPos);
        pathfindingIOManager.Initialize(treasureMap, gridView);
        pathfindingIOManager.ClearField();
        gridWalls.Initialize(treasureMap, layout, pathfindingIOManager.UpdateDistances);
    }


    // TODO is this the best way to implement / group these methods? Feel like there must be a better way
    public void OnRefreshPressed() => pathfindingIOManager.OnRefreshPressed();
    public void OnClearFieldPressed() => pathfindingIOManager.OnClearFieldPressed();
    public void OnSingleStepPressed() => pathfindingIOManager.OnSingleStepPressed();
    public void OnVisualizePressed() => pathfindingIOManager.OnVisualizePressed();
    public void OnFromStartModePressed() => pathfindingIOManager.OnFromStartModePressed();
    public void OnFromEndModePressed() => pathfindingIOManager.OnFromEndModePressed();
    public void SetNumbersVisible(bool isEnabled) => pathfindingIOManager.OnSetNumbersVisible(isEnabled);
    public void SetVisualizationVisible(bool isEnabled) => pathfindingIOManager.OnSetVisualizationVisible(isEnabled);
    public void SetAutoRefreshMode(bool isEnabled) => pathfindingIOManager.SetAutoRefreshMode(isEnabled);

    // TODO enemy spawn logic below to get it going. Move out once a home is found. 
    // Maybe some of it will form part of this

    [SerializeField] private Boat boatPrefab;
    [SerializeField] private EnemyController enemyController; // TODO move code herea and use this

    Boat enemy = null;

    // spawn a single enemy, just to test it out.
    // passing in parameters in prep for moving this function out
    void SpawnEnemy(GridLayout layout, Coord spawnPos, Coord goalPos)
    {
        enemy = Instantiate(boatPrefab, transform);
        Vector3 pos = layout.CoordsToWorld(spawnPos);
        enemy.transform.localPosition = pos;
        enemy.name = $"Boat";
        enemy.Initialize(spawnPos, goalPos, layout);
        // TODO save enemies in a list
    }

    void PathfindingUpdate()
    {
        if (enemy == null) return;

        enemy.RecalculatePathing(treasureMap);

    }

    void PathfindingClear()
    {
        if (enemy == null) return;

        enemy.ClearPathing();

    }

}
