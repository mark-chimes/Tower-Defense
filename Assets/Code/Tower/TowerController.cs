using UnityEngine;

// Later, this might split or become a view / control class
public class TowerController : MonoBehaviour
{
    [SerializeField] private GameObject towerPrefab;

    [SerializeField] private Material ghostMaterial;
    [SerializeField] private Material ghostBlockedMaterial;

    private GameObject ghostPlaceable;
    private GameObject ghostBlocked;

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

        CreateGhosts();
    }

    public void ClearData()
    {
        Debug.Assert(isInitialized);
        isInitialized = false;

        RemoveAllTowers();
        foreach (Transform child in transform) Destroy(child.gameObject);

        towers = null;
        treasureMap = null;
        ghostPlaceable = null;
        ghostBlocked = null;
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

    // Only call this if the tower is 
    public void PlaceTower(HexCoord coord)
    {
        Debug.Assert(CanPlaceTower(coord), $"Cannot place tower at {coord}");
        if (!CanPlaceTower(coord)) return;

        GameObject tower = Instantiate(towerPrefab, transform);
        Vector3 pos = HexProjection.CoordsToLandSurface(coord);
        tower.transform.localPosition = pos;
        tower.name = $"Tower_{coord}";
        // Might need tower.Initialize later if this becomes its own object
        towers.SetAt(coord, tower);
    }

    private void CreateGhosts()
    {
        ghostPlaceable = CreateGhost(ghostMaterial, "TowerPlaceableGhost");
        ghostBlocked = CreateGhost(ghostBlockedMaterial, "TowerBlockedGhost");
        ghostBlocked.transform.localScale *= 1.03f;
    }

    private GameObject CreateGhost(Material material, string objName)
    {
        GameObject ghost = Instantiate(towerPrefab, transform);
        ghost.name = objName;
        foreach (Renderer renderer in ghost.GetComponentsInChildren<Renderer>())
        {
            Material[] mats = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = material;
            }
            renderer.sharedMaterials = mats;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        ghost.SetActive(false);
        return ghost;
    }

    public void ShowPlaceableGhostAt(HexCoord coord)
    {
        Vector3 pos = HexProjection.CoordsToLandSurface(coord);
        ghostPlaceable.transform.localPosition = pos;
        ghostPlaceable.SetActive(true);
        ghostBlocked.SetActive(false);
    }

    public void ShowBlockedGhostAt(HexCoord coord)
    {
        Vector3 pos = HexProjection.CoordsToLandSurface(coord);
        ghostBlocked.transform.localPosition = pos;
        ghostPlaceable.SetActive(false);
        ghostBlocked.SetActive(true);
    }


    public void HideGhosts()
    {
        ghostPlaceable.SetActive(false);
        ghostBlocked.SetActive(false);
    }


    public void RemoveTower(HexCoord coord)
    {
        Debug.Assert(HasTower(coord), $"No tower at {coord} to remove.");
        if (!HasTower(coord)) return; // TODO optional?
        Destroy(towers.At(coord));
        towers.SetAt(coord, null);
    }

    public void RemoveTowerIfPresent(HexCoord coord)
    {
        if (HasTower(coord)) RemoveTower(coord);
    }

    public void RemoveAllTowers()
    {
        foreach (HexCoord c in towers.AllCoords())
        {
            RemoveTowerIfPresent(c);
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
