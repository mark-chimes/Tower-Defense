using UnityEngine;


// TODO split out concerns: 
// - Spawn enemies on map
// - Control where they go
public class EnemyController : MonoBehaviour
{
    [SerializeField] private Boat boatPrefab;

    private TreasureMap treasureMap;
    private Fleet fleet;
    private Boat[] boats = null;

    public bool BoatsFollowPathOnSpawn { get; set; } = true;



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
        // TODO should this be on fleet.Count?
        for (int i = 0; i < boats.Length; i++)
        {
            if (!fleet.IsAlive(i)) continue;
            boats[i].transform.localPosition = Position3DOf(i);
            boats[i].transform.localRotation = HexProjection.DegreesToQuaternion(fleet.HeadingOf(i));
            // TODO set visibility here? 
        }
    }

    private Vector3 Position3DOf(int slot) => HexProjection.Float2ToWorld(fleet.PositionOf(slot));

    public void OnSpawnBoatPressed()
    {
        SpawnEnemy();
    }

    public void OnDeleteBoatsPressed()
    {
        // TODO implement or remove
    }

    public void OnBoatsFollowExistingPathPressed()
    {
        fleet.StartAll();
    }

    public void OnBoatsStopPressed()
    {
        fleet.StopAll();
    }

    public void SpawnEnemy()
    {
        if (!fleet.CanSpawn())
        {
            Debug.LogWarning("Fleet size reached. Unable to spawn new enemy.");
            return;
        }
        // Should the spawn button handler and the GameMaster startup call pass treasureMap.SpawnCoord?
        int slot = fleet.Spawn(treasureMap.SpawnCoord, BoatsFollowPathOnSpawn);
        Boat boat = Instantiate(boatPrefab, transform);
        Vector3 pos = Position3DOf(slot);
        boat.transform.localPosition = pos;
        boat.name = $"Boat";
        boats[slot] = boat;
    }

    public void PathfindingUpdate()
    {
        // TODO: implement or move
    }

    public void PathfindingClear()
    {
        // TODO: implement or move
    }
}
