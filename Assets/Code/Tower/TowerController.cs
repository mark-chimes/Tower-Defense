using UnityEngine;

// Later, this might split or become a view / control class
public class TowerController : MonoBehaviour
{
    [SerializeField] private GameObject towerPrefab;

    // TODO make these a specific object type later
    // Also this data might split out later, especially when towers affect pathfinding
    private Lattice<GameObject> towers;

    private TreasureMap treasureMap;

    private bool isInitialized;

    public void Initialize(TreasureMap treasureMap)
    {
        Debug.Assert(!isInitialized);
        isInitialized = true;

        this.treasureMap = treasureMap;
        towers = new Lattice<GameObject>(treasureMap.NumRings);
    }

    public void ClearData()
    {
        Debug.Assert(isInitialized);
        isInitialized = false;

        RemoveAllTowers();
        towers = null;
        foreach (Transform child in transform) Destroy(child.gameObject);
        treasureMap = null;
    }

    public bool HasTower(HexCoord coord)
    {
        return towers.At(coord) != null;
    }

    public bool CanPlaceTower(HexCoord coord)
    {
        // remember "wall" is actually land
        return treasureMap.HasWall(coord) && !HasTower(coord);
    }

    public void PlaceTower(HexCoord coord)
    {
        if (!CanPlaceTower(coord)) return;

        GameObject tower = Instantiate(towerPrefab, transform);
        Vector3 pos = HexProjection.CoordsToLandSurface(coord);
        tower.transform.localPosition = pos;
        tower.name = $"Tower_{coord}";
        // Might need tower.Initialize later if this becomes its own object
        towers.SetAt(coord, tower);
    }

    public void RemoveTower(HexCoord coord)
    {
        if (!HasTower(coord)) return;
        Destroy(towers.At(coord));
        towers.SetAt(coord, null);
    }

    public void RemoveAllTowers()
    {
        foreach (HexCoord c in towers.AllCoords())
        {
            RemoveTower(c);
        }
    }

    public TowerSaveData GetSaveData()
    {
        Lattice<bool> towerMap = new Lattice<bool>(towers.NumRings);
        foreach (HexCoord c in towers.AllCoords())
        {
            towerMap.SetAt(c, HasTower(c));
        }
        return new TowerSaveData(towerMap);
    }


    public void LoadFromSaveData(TowerSaveData data)
    {
        Debug.Assert(isInitialized);
        Lattice<bool> towersLoaded = data.GetTowerMap(treasureMap.NumRings);
        foreach (HexCoord c in towersLoaded.AllCoords())
        {
            bool isTower = towersLoaded.At(c);
            if (!isTower) continue;

            if (!CanPlaceTower(c))
            {
                Debug.LogWarning($"Loaded tower cannot be placed at {c}");
                continue;
            }

            PlaceTower(c);
        }
    }

}
