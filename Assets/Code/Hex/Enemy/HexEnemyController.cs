using UnityEngine;


// TODO rename this to better suit its behavior
// TODO split out concerns: 
// - Spawn enemies on map
// - Control where they go
public class HexEnemyController : MonoBehaviour
{
    [SerializeField] private HexBoat boatPrefab;

    private HexTreasureMap treasureMap;
    private HexBoat enemy = null;


    private bool isInitialized;

    public void Initialize(HexTreasureMap treasureMap)
    {
        Debug.Assert(!isInitialized);
        isInitialized = true;

        this.treasureMap = treasureMap;
    }

    public void ClearData()
    {
        Debug.Assert(isInitialized);
        isInitialized = false;

        foreach (Transform child in transform) Destroy(child.gameObject);
        treasureMap = null;
        enemy = null;
    }


    public void Reinitialize(HexTreasureMap treasureMap)
    {
        ClearData();
        Initialize(treasureMap);
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
        enemy = Instantiate(boatPrefab, transform);
        Vector3 pos = HexLayout.CoordsToWorld(treasureMap.SpawnPos);
        enemy.transform.localPosition = pos;
        enemy.name = $"Boat";
        enemy.Initialize(treasureMap.SpawnPos, treasureMap.GoalPos);
        // TODO save enemies in a list 
    }

    public void PathfindingUpdate()
    {
        if (enemy == null) return;

        enemy.RecalculatePathing(treasureMap);

    }

    public void PathfindingClear()
    {
        if (enemy == null) return;

        enemy.ClearPathing();

    }


}
