using UnityEngine;

[System.Serializable]
public class HexAuthor
{
    [SerializeField] private int width = 9;
    [SerializeField] private int height = 9;
    [SerializeField] private Vector2Int spawnPosInitXZ = new(0, 0);
    [SerializeField] private Vector2Int goalPosInitXZ = new(1, 1);
    // Note it is possible to specify the above as out-of-bounds,
    // or as the same square. 

    public HexLayout Layout => new HexLayout(width, height);
    public HexCoord SpawnPos => new HexCoord(spawnPosInitXZ.x, spawnPosInitXZ.y);
    public HexCoord GoalPos => new HexCoord(goalPosInitXZ.x, goalPosInitXZ.y);
}