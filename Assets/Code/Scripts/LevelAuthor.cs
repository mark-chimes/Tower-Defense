using UnityEngine;

public class LevelAuthor
{
    [SerializeField] private Vector2Int spawnPosInitXZ;
    [SerializeField] private Vector2Int goalPosInitXZ;

    private GridLayout layout;
    public Coord SpawnPos => new Coord(spawnPosInitXZ.x, spawnPosInitXZ.y);
}