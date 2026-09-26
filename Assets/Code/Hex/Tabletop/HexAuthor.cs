using UnityEngine;

[System.Serializable]
public class HexAuthor
{
    [SerializeField] private int numRings = 3;

    [SerializeField] private Vector2Int spawnPosInitQR = new(0, 0); // S calculated automatically
    [SerializeField] private Vector2Int goalPosInitQR = new(1, 1); // S calculated automatically
    // Note it is possible to specify the above as out-of-bounds, or as the same tile, or as a. 

    public int NumRings => numRings;
    public HexCoord SpawnCoord => new HexCoord(spawnPosInitQR.x, spawnPosInitQR.y);
    public HexCoord GoalCoord => new HexCoord(goalPosInitQR.x, goalPosInitQR.y);
}