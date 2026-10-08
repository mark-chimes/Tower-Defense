using UnityEngine;

// Later, this might split or become a view / control class
public class TowerController : MonoBehaviour
{
    [SerializeField] private GameObject towerPrefab1;
    [SerializeField] private GameObject towerPrefab2;


    [SerializeField] private Material ghostMaterial;
    [SerializeField] private Material ghostBlockedMaterial;

    private GameObject ghostPlaceable1;
    private GameObject ghostBlocked1;

    private GameObject ghostPlaceable2;
    private GameObject ghostBlocked2;

    // TODO current tower type logic is horrible and won't scale to more tower types
    // TODO also BoardInput or someone should definitely be the one tracking this variable and passing it in?
    public int currentPlacementTowerType { get; private set; } = 0;

    // TODO make these a specific object type later
    // Also this data might split out later, especially when towers affect pathfinding
    // TODO we also shoul think about the fact we're having to keep towers and towerTypes in-sync, is that good logic?
    private Lattice<GameObject> towers;
    private Lattice<int> towerTypes;

    private TreasureMap treasureMap;

    private bool isInitialized;

    public void Initialize(TreasureMap treasureMap)
    {
        Debug.Assert(!isInitialized);
        isInitialized = true;

        this.treasureMap = treasureMap;
        towers = new Lattice<GameObject>(treasureMap.NumRings);
        towerTypes = new Lattice<int>(treasureMap.NumRings);
        currentPlacementTowerType = 1;

        CreateGhosts();
    }

    public void ClearData()
    {
        Debug.Assert(isInitialized);
        isInitialized = false;

        RemoveAllTowers();
        foreach (Transform child in transform) Destroy(child.gameObject);

        towers = null;
        towerTypes = null;
        treasureMap = null;
        ghostPlaceable1 = null;
        ghostBlocked1 = null;
        ghostPlaceable2 = null;
        ghostBlocked2 = null;
    }

    public bool HasTower(HexCoord coord)
    {
        Debug.Assert(towerTypes.At(coord) == 0 && towers.At(coord) == null ||
            towerTypes.At(coord) != 0 && towers.At(coord) != null,
            $"Tower type is {towerTypes.At(coord)} but towers.At(coord) != null {towers.At(coord)}. Should be null iff tower type is 0");

        return towerTypes.At(coord) != 0;
    }

    public int TowerTypeAt(HexCoord coord)
    {
        return towerTypes.At(coord);
    }

    public bool CanPlaceTower(HexCoord coord)
    {
        return treasureMap.IsLand(coord) && !HasTower(coord);
    }

    // TODO gave this a horrible name so we know to change it later
    public void BoardInputCallsPlaceTower(HexCoord coord)
    {
        PlaceTower(coord, currentPlacementTowerType);
    }

    // TODO gave this a horrible name so we know to get rid of it later
    public void BoardInputSetsTowerType(int towerType)
    {
        currentPlacementTowerType = towerType;
    }

    private void PlaceTower(HexCoord coord, int towerTypeAtC)
    {
        Debug.Assert(towerTypeAtC == 1 || towerTypeAtC == 2, $"Tower type {towerTypeAtC} unrecognized");
        Debug.Assert(CanPlaceTower(coord), $"Cannot place tower at {coord}");

        if (!CanPlaceTower(coord)) return;

        GameObject tower;
        if (towerTypeAtC == 2)
        {
            tower = Instantiate(towerPrefab2, transform);
        }
        else if (towerTypeAtC == 1)
        {
            tower = Instantiate(towerPrefab1, transform);

        }
        else
        {
            return;
        }
        Vector3 pos = HexProjection.CoordsToLandSurface(coord);
        tower.transform.localPosition = pos;
        tower.name = $"Tower_{towerTypeAtC}_{coord}";
        // Might need tower.Initialize later if this becomes its own object
        towers.SetAt(coord, tower);
        towerTypes.SetAt(coord, towerTypeAtC);
    }

    private void CreateGhosts()
    {
        ghostPlaceable2 = CreateGhost(ghostMaterial, towerPrefab2, "TowerPlaceableGhost");
        ghostBlocked2 = CreateGhost(ghostBlockedMaterial, towerPrefab2, "TowerBlockedGhost");
        ghostBlocked2.transform.localScale *= 1.03f;

        ghostPlaceable1 = CreateGhost(ghostMaterial, towerPrefab1, "TowerPlaceableGhost1");
        ghostBlocked1 = CreateGhost(ghostBlockedMaterial, towerPrefab1, "TowerBlockedGhost1");
        ghostBlocked1.transform.localScale *= 1.03f;

    }

    private GameObject CreateGhost(Material material, GameObject objectPrefab, string objName)
    {
        GameObject ghost = Instantiate(objectPrefab, transform);
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
        HideGhosts();
        Vector3 pos = HexProjection.CoordsToLandSurface(coord);
        if (currentPlacementTowerType == 2)
        {
            ghostPlaceable2.transform.localPosition = pos;
            ghostPlaceable2.SetActive(true);
        }
        else if (currentPlacementTowerType == 1)
        {
            ghostPlaceable1.transform.localPosition = pos;
            ghostPlaceable1.SetActive(true);
        }

    }

    public void ShowBlockedGhostAt(HexCoord coord)
    {
        HideGhosts();
        Vector3 pos = HexProjection.CoordsToLandSurface(coord);

        if (currentPlacementTowerType == 2)
        {
            ghostBlocked2.transform.localPosition = pos;
            ghostBlocked2.SetActive(true);
        }
        else if (currentPlacementTowerType == 1)
        {
            ghostBlocked1.transform.localPosition = pos;
            ghostBlocked1.SetActive(true);
        }
    }


    public void HideGhosts()
    {
        ghostPlaceable2.SetActive(false);
        ghostBlocked2.SetActive(false);
        ghostPlaceable1.SetActive(false);
        ghostBlocked1.SetActive(false);
    }


    public void RemoveTower(HexCoord coord)
    {
        Debug.Assert(HasTower(coord), $"No tower at {coord} to remove.");
        if (!HasTower(coord)) return; // TODO optional?
        Destroy(towers.At(coord));
        towers.SetAt(coord, null);
        towerTypes.SetAt(coord, 0);
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
        Lattice<int> towerMap = new Lattice<int>(towers.NumRings);
        foreach (HexCoord c in towers.AllCoords())
        {
            towerMap.SetAt(c, TowerTypeAt(c));
        }
        return new TowerSaveData(towerMap);
    }


    public void LoadFromSaveData(TowerSaveData data)
    {
        Debug.Assert(isInitialized);
        Lattice<int> towersLoaded = data.GetTowerMap(treasureMap.NumRings);
        foreach (HexCoord c in towersLoaded.AllCoords())
        {
            int towerTypeAtC = towersLoaded.At(c);
            if (towerTypeAtC == 0) continue;
            if (!CanPlaceTower(c))
            {
                Debug.LogWarning($"Loaded tower cannot be placed at {c}");
                continue;
            }
            PlaceTower(c, towerTypeAtC);
        }
    }

}
