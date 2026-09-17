using UnityEngine;


// TODO rename this to better suit its behavior
// TODO split out concerns: 
// - Spawn enemies on map
// - Control where they go
public class EnemyController : MonoBehaviour
{
    [SerializeField] private Boat boatPrefab;

    private GridLayout layout;
    private TreasureMap treasureMap;

    public void Initialize(GridLayout layout, TreasureMap treasureMap)
    {
        this.layout = layout;
        this.treasureMap = treasureMap;
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


    Boat enemy = null;

    // spawn a single enemy, just to test it out.
    public void SpawnEnemy()
    {
        enemy = Instantiate(boatPrefab, transform);
        Vector3 pos = layout.CoordsToWorld(treasureMap.SpawnPos);
        enemy.transform.localPosition = pos;
        enemy.name = $"Boat";
        enemy.Initialize(treasureMap.SpawnPos, treasureMap.GoalPos, layout);
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
