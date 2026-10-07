using UnityEngine;


// TODO split out concerns: 
// - Spawn enemies on map
// - Control where they go
public class EnemyController : MonoBehaviour
{
    [SerializeField] private Boat boatPrefab;
    [SerializeField] private int spawnBatchSize = 10;

    public int SpawnBatchSize => spawnBatchSize;


    private TreasureMap treasureMap;
    private Fleet fleet;
    private Boat[] boats = null;

    public bool BoatsFollowPathOnSpawn { get; set; } = true;

    public bool BoatsDespawnAtGoal { get => fleet.DespawnsAtGoal; set => fleet.DespawnsAtGoal = value; }


    private bool isInitialized;

    public void Initialize(TreasureMap treasureMap, Fleet fleet)
    {
        Debug.Assert(!isInitialized);
        isInitialized = true;

        this.treasureMap = treasureMap;
        this.fleet = fleet;

        boats = new Boat[fleet.Capacity()];

    }

    public void ClearData()
    {
        Debug.Assert(isInitialized);
        isInitialized = false;

        foreach (Transform child in transform) Destroy(child.gameObject);
        treasureMap = null;
        fleet = null;
        boats = null;
    }

    public void SyncBoats()
    {
        for (int i = 0; i < fleet.SlotsUsed; i++)
        {
            bool isAlive = fleet.IsAlive(i);
            if (boats[i].gameObject.activeSelf != isAlive)
                boats[i].gameObject.SetActive(isAlive);
            if (!isAlive) continue;

            boats[i].transform.localPosition = Position3DOf(i);
            boats[i].transform.localRotation = HexProjection.DegreesToQuaternion(fleet.HeadingOf(i));
        }
    }

    private Vector3 Position3DOf(int slot) => HexProjection.Float2ToWorld(fleet.PositionOf(slot));

    public void OnSpawnBoatPressed()
    {
        SpawnEnemy();
    }

    public void OnDeleteBoatsPressed()
    {
        fleet.DespawnAll();
    }

    public void OnBoatsFollowExistingPathPressed()
    {
        fleet.StartAll();
    }

    public void OnBoatsStopPressed()
    {
        fleet.StopAll();
    }

    public void OnBoatsBrakePressed()
    {
        fleet.BrakeAll();
    }


    public void OnSpawnBatchPressed()
    {
        for (int i = 0; i < spawnBatchSize && fleet.CanSpawn(); i++)
        {
            SpawnEnemy();
        }
    }

    public void SpawnEnemy()
    {
        if (!fleet.CanSpawn())
        {
            Debug.LogWarning("Fleet size reached. Unable to spawn new enemy.");
            return;
        }
        int slot = fleet.Spawn(treasureMap.SpawnCoord, BoatsFollowPathOnSpawn);

        if (boats[slot] == null) boats[slot] = Instantiate(boatPrefab, transform);

        Boat boat = boats[slot];
        Vector3 pos = Position3DOf(slot);
        boat.transform.localPosition = pos;
        boat.name = $"Boat_{slot}";
    }
}
