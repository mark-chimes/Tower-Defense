using System;
using UnityEngine;

public class GridWalls : MonoBehaviour
{

    [SerializeField] private Wall wallPrefab;

    private Wall[,] walls;
    private TreasureMap treasureMap;
    private GridLayout layout;

    private Action onWallChange;

    public void Initialize(TreasureMap treasureMap, GridLayout layout, Action onWallChange)
    {
        this.treasureMap = treasureMap;
        this.layout = layout;
        this.onWallChange = onWallChange;
        walls = new Wall[treasureMap.Width, treasureMap.Height];
    }

    public Highlightable MaybeWall(Coord c)
    {
        return walls[c.X, c.Z];
    }


    public void SpawnWall(Coord c)
    {
        Debug.Assert(treasureMap != null, "GridWalls.Initialize was never called");
        if (walls[c.X, c.Z] != null) return;

        ErfSnapshot erf = treasureMap.At(c);

        if (erf.Kind != SpawnGoalKind.Floor)
        {
            Debug.LogError($"SpawnWall: {c} Kind was {erf.Kind}");
            return;
        }

        Wall wall = Instantiate(wallPrefab, transform);
        wall.transform.localPosition = layout.CoordsToWorld(c);
        wall.name = $"Wall_{c.X}_{c.Z}";
        walls[c.X, c.Z] = wall;
        treasureMap.SetWall(c, true);
        onWallChange.Invoke();
    }

    public void DespawnWall(Coord c)
    {
        Debug.Assert(treasureMap != null, "GridWalls.Initialize was never called");

        Wall wall = walls[c.X, c.Z];
        if (wall == null) return;

        ErfSnapshot erf = treasureMap.At(c);

        if (erf.Kind != SpawnGoalKind.Floor)
        {
            Debug.LogError($"DespawnWall: {c} Kind was {erf.Kind}", wall);
            return;
        }
        walls[c.X, c.Z] = null;
        Destroy(wall.gameObject);
        treasureMap.SetWall(c, false);
        onWallChange.Invoke();
    }
}