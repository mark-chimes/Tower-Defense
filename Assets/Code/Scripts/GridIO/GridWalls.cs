using System;
using UnityEngine;

public class GridWalls : MonoBehaviour
{

    [SerializeField] private Wall wallPrefab;

    private RectMap<Wall> walls;
    private TreasureMap treasureMap;
    private GridLayout layout;

    private Action onWallChange;

    private bool isInitialized;

    public void Initialize(TreasureMap treasureMap, GridLayout layout, Action onWallChange)
    {
        Debug.Assert(!isInitialized);
        isInitialized = true;

        this.treasureMap = treasureMap;
        this.layout = layout;
        this.onWallChange = onWallChange;
        walls = new RectMap<Wall>(treasureMap.Width, treasureMap.Height);

        for (int x = 0; x < treasureMap.Width; x++)
        {
            for (int z = 0; z < treasureMap.Height; z++)
            {
                Coord c = new Coord(x, z);
                if (treasureMap.HasWall(c))
                {
                    MakeWallAt(c);
                }
            }

        }
    }

    public void ClearData()
    {
        Debug.Assert(isInitialized);
        isInitialized = false;

        this.treasureMap = null;
        this.layout = null;
        this.onWallChange = null;
        foreach (Transform child in transform) Destroy(child.gameObject);
        this.walls = null;
    }

    public void Reinitialize(TreasureMap treasureMap, GridLayout layout, Action onWallChange)
    {
        ClearData();
        Initialize(treasureMap, layout, onWallChange);
    }

    public Highlightable MaybeWall(Coord c)
    {
        return walls.At(c);
    }


    public void SpawnWall(Coord c)
    {
        Debug.Assert(treasureMap != null, "GridWalls.Initialize was never called");
        if (walls.At(c) != null) return;

        ErfSnapshot erf = treasureMap.At(c);

        if (erf.Kind != SpawnGoalKind.Floor)
        {
            Debug.LogError($"SpawnWall: {c} Kind was {erf.Kind}");
            return;
        }

        treasureMap.SetWall(c, true);
        MakeWallAt(c);
        onWallChange.Invoke();
    }

    private void MakeWallAt(Coord c)
    {
        Wall wall = Instantiate(wallPrefab, transform);
        wall.transform.localPosition = layout.CoordsToWorld(c);
        wall.name = $"Wall_{c.X}_{c.Z}";
        walls.SetAt(c, wall);
    }

    public void DespawnWall(Coord c)
    {
        Debug.Assert(treasureMap != null, "GridWalls.Initialize was never called");

        Wall wall = walls.At(c);
        if (wall == null) return;

        ErfSnapshot erf = treasureMap.At(c);

        if (erf.Kind != SpawnGoalKind.Floor)
        {
            Debug.LogError($"DespawnWall: {c} Kind was {erf.Kind}", wall);
            return;
        }
        walls.SetAt(c, null);
        Destroy(wall.gameObject);
        treasureMap.SetWall(c, false);
        onWallChange.Invoke();
    }

    // The below are temporary and should be reconsidered with the proper feature 
    public void OnBuildWallsMode()
    {
        // TODO switch mode

    }

    public void OnBuildTowersMode()
    {

        // TODO switch mode

    }


}