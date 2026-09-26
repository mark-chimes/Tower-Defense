using UnityEngine;

[System.Serializable]
public class HexAuthor
{
    [SerializeField] private int numRings = 3;

    [SerializeField] private Vector2Int spawnInitQR = new(0, 0); // S calculated automatically
    [SerializeField] private Vector2Int goalInitQR = new(1, 1); // S calculated automatically
    // Note it is possible to specify the above as out-of-bounds, or as the same tile, or as a. 

    public int NumRings => numRings;
    public HexCoord SpawnCoord => new HexCoord(spawnInitQR.x, spawnInitQR.y);
    public HexCoord GoalCoord => new HexCoord(goalInitQR.x, goalInitQR.y);
}