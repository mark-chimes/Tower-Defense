using System.Collections.Generic;
using UnityEngine;
using static Signpost;

// TODO split out owning the terrain (flagstones) and the markers
public class TerrainWithOverlay : MonoBehaviour
{
    [SerializeField] private GameObject seaTilePrefab;
    [SerializeField] private GameObject landTilePrefab;
    [SerializeField] private GameObject signpostPrefab;

    [SerializeField] private GameObject spawnPrefab;
    [SerializeField] private GameObject goalPrefab;

    [System.Serializable]
    public class VisualizationSettings
    {
        [SerializeField] public bool ShowDistance = false;
        [SerializeField] public bool ShowPathfinding = true;
    }

    private Lattice<Flagstone> flagstones;
    private Lattice<Signpost> signposts;

    private GameObject spawnObj;
    private GameObject goalObj;

    private bool isInitialized;

    public void Initialize(TreasureMap treasureMap, VisualizationSettings visualizationSettings)
    {
        Debug.Assert(!isInitialized);
        isInitialized = true;

        flagstones = new Lattice<Flagstone>(treasureMap.NumRings);
        signposts = new Lattice<Signpost>(treasureMap.NumRings);

        foreach (HexCoord c in flagstones.AllCoords())
        {
            GameObject signpostObj = InstantiateAtCoord(signpostPrefab, c);
            Signpost signpost = signpostObj.GetComponent<Signpost>();
            signpost.Initialize(c);
            signpost.SetIsOnPathableTerrain(!treasureMap.IsLand(c));
            signpost.SetPathingVisible(visualizationSettings.ShowPathfinding);
            signpost.SetDistanceVisible(visualizationSettings.ShowDistance);

            MakeFlagstoneAt(c, treasureMap.IsLand(c));

            signpost.name = $"Signpost_{c}";
            signposts.SetAt(c, signpost);
        }

        spawnObj = InstantiateAtCoord(spawnPrefab, treasureMap.SpawnCoord);
        goalObj = InstantiateAtCoord(goalPrefab, treasureMap.GoalCoord);
    }

    public void ClearData()
    {
        Debug.Assert(isInitialized);
        isInitialized = false;

        foreach (Transform child in transform) Destroy(child.gameObject);
        spawnObj = null;
        goalObj = null;
        flagstones = null;
    }

    public void Reinitialize(TreasureMap treasureMap, VisualizationSettings visualizationSettings)
    {
        ClearData();
        Initialize(treasureMap, visualizationSettings);
    }

    private GameObject InstantiateAtCoord(GameObject prefab, HexCoord coord)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.transform.localPosition = HexProjection.CoordsToWorld(coord);
        return obj;
    }

    private void UnhighlightAllArrows()
    {
        Debug.Assert(isInitialized);
        foreach (Signpost signpost in signposts.All())
        {
            signpost.SetAccent(ArrowAccent.Normal);
        }
    }

    public void ShowFlows(IReadOnlyCollection<FlowSample> flows)
    {
        Debug.Assert(isInitialized);

        foreach (FlowSample flow in flows)
        {
            HexCoord c = flow.Coord;
            Signpost signpost = signposts.At(c);
            signpost.ShowFlow(flow);
            signpost.SetAccent(flow.OnCriticalPath ? ArrowAccent.Path : ArrowAccent.Normal);
        }
    }

    public void SetTerrainAt(HexCoord c, bool isLand)
    {
        Destroy(flagstones.At(c).gameObject);
        MakeFlagstoneAt(c, isLand);
        signposts.At(c).SetIsOnPathableTerrain(!isLand);
    }

    private Flagstone MakeFlagstoneAt(HexCoord c, bool isLand)
    {
        GameObject prefab = isLand ? landTilePrefab : seaTilePrefab;
        GameObject flagstoneObj = InstantiateAtCoord(prefab, c);
        flagstoneObj.name = $"Flagstone_{c}";
        Flagstone flagstone = flagstoneObj.GetComponent<Flagstone>();
        flagstone.Initialize(c);
        flagstones.SetAt(c, flagstone);
        return flagstone;
    }

    // TODO the methods below maybe don't cut at the right seams
    // TODO we're getting changed as sigpost, frontier as coords. Could we just get everything as coords? 
    public void HighlightChangedOrFrontier(
        bool shouldHighlight,
        IReadOnlyCollection<FlowSample> changed,
        IReadOnlyCollection<HexCoord> frontier)
    {
        Debug.Assert(isInitialized);

        UnhighlightAllArrows();
        foreach (FlowSample flow in changed)
        {
            HexCoord c = flow.Coord;
            Signpost signpost = signposts.At(c);
            signpost.ShowFlow(flow);
        }

        if (shouldHighlight)
        {
            foreach (HexCoord c in frontier)
            {
                Signpost signpost = signposts.At(c);
                signpost.SetAccent(ArrowAccent.Frontier);
            }
        }
    }

    public void HighlightPath(IReadOnlyCollection<FlowSample> changed)
    {
        Debug.Assert(isInitialized);

        foreach (FlowSample flow in changed)
        {
            HexCoord c = flow.Coord;
            Signpost signpost = signposts.At(c);
            signpost.SetAccent(ArrowAccent.Path);
        }
    }

    public void SetVisualizationVisible(bool isVisible)
    {
        Debug.Assert(isInitialized);

        spawnObj.SetActive(isVisible);

        foreach (Signpost signpost in signposts.All())
        {
            signpost.SetPathingVisible(isVisible);
        }
    }

    public void SetDistanceVisible(bool isVisible)
    {
        Debug.Assert(isInitialized);

        foreach (Signpost signpost in signposts.All())
        {
            signpost.SetDistanceVisible(isVisible);
        }
    }
}
