using UnityEngine;


// TODO split out concerns: 
// - Spawn enemies on map
// - Control where they go
public class EnemyController : MonoBehaviour
{
    [SerializeField] private Boat boatPrefab;

    private TreasureMap treasureMap;
    private Fleet fleet;
    private Boat enemy = null;


    private bool isInitialized;

    public void Initialize(TreasureMap treasureMap, Fleet fleet)
    {
        Debug.Assert(!isInitialized);
        isInitialized = true;

        this.treasureMap = treasureMap;
        this.fleet = fleet;
    }

    public void ClearData()
    {
        Debug.Assert(isInitialized);
        isInitialized = false;

        foreach (Transform child in transform) Destroy(child.gameObject);
        treasureMap = null;
        enemy = null;
    }

    public void OnSpawnBoatPressed()
    {
        OnDeleteBoatsPressed(); // we can only have one boat at the moment.
        SpawnEnemy();
    }
    public void OnDeleteBoatsPressed()
    {
        Debug.Log($"Destroy enemy {enemy}");
        if (enemy != null) Destroy(enemy.gameObject);
        Debug.Log($"Enemy destroyed: {enemy}");
    }

    public void OnBoatsFollowExistingPathPressed()
    {
        PathfindingUpdate();
    }

    public void OnBoatsStopPressed()
    {
        PathfindingClear();
    }



    // spawn a single enemy, just to test it out.
    public void SpawnEnemy()
    {
        if (!fleet.CanSpawn()) 
        {
            Debug.LogWarning("Fleet size reached. Unable to spawn new enemy.");
            return;
        }
        int slot = fleet.Spawn(treasureMap.SpawnCoord);
        enemy = Instantiate(boatPrefab, transform);
        Vector3 pos = HexProjection.Vector2ToWorld(fleet.PositionOf(slot));
        enemy.transform.localPosition = pos;
        enemy.name = $"Boat";
        enemy.Initialize(treasureMap.SpawnCoord, treasureMap.GoalCoord);
        // TODO save enemies in a list 
    }

    public void PathfindingUpdate()
    {
        if (enemy == null) return;
        // TODO: this should change to go via the fleet, soon
        enemy.RecalculatePathing(treasureMap);

    }

    public void PathfindingClear()
    {
        if (enemy == null) return;
        // TODO: this should change to go via the fleet, soon
        enemy.ClearPathing();
    }


}
