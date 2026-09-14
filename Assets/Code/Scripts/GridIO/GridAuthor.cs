using UnityEngine;

[System.Serializable]
public class GridAuthor
{
    [SerializeField] private int width = 9;
    [SerializeField] private int height = 9;
    [SerializeField] private Vector2Int spawnPosInitXZ = new(0, 0);
    [SerializeField] private Vector2Int goalPosInitXZ = new(1, 1);
    // Note it is possible to specify the above as out-of-bounds,
    // or as the same square. 
    // Improving it to add checks deferred to later
    public GridLayout Layout => new GridLayout(width, height);
    public Coord SpawnPos => new Coord(spawnPosInitXZ.x, spawnPosInitXZ.y);
    public Coord GoalPos => new Coord(goalPosInitXZ.x, goalPosInitXZ.y);
}