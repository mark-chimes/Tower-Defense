using System.Collections.Generic;
using UnityEngine;
using static Signpost;

// TODO split out owning the floor (flagstones) and the markers
public class TerrainWithOverlay : MonoBehaviour
{
    [SerializeField] private GameObject floorTilePrefab; // TODO rename to water tile or something after code port
    [SerializeField] private GameObject wallTilePrefab; // TODO rename to land tile or something after code port
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

    // TODO reconsider if this should handle "floors" and "walls" together or not? 
    public void Initialize(Lattice<bool> walls, HexCoord spawnCoord, HexCoord goalCoord,
    // HexCoord spawnPos, HexCoord goalPos, 
        VisualizationSettings visualizationSettings) // Later
    {
        Debug.Assert(!isInitialized);
        isInitialized = true;

        flagstones = new Lattice<Flagstone>(walls.NumRings);
        signposts = new Lattice<Signpost>(walls.NumRings);

        foreach (HexCoord c in flagstones.AllCoords())
        {
            GameObject signpostObj = InstantiateAtCoord(signpostPrefab, c);
            Signpost signpost = signpostObj.GetComponent<Signpost>();
            signpost.Initialize(c);
            signpost.SetIsOnPathableTerrain(!walls.At(c));
            signpost.SetPathingVisible(visualizationSettings.ShowPathfinding);
            signpost.SetDistanceVisible(visualizationSettings.ShowDistance);

            MakeFlagstoneAt(c, walls.At(c));

            signpost.name = $"Signpost_{c}"; // TODO rename this
            signposts.SetAt(c, signpost);

            // TODO spawn and goal pos
        }

        spawnObj = InstantiateAtCoord(spawnPrefab, spawnCoord);
        goalObj = InstantiateAtCoord(goalPrefab, goalCoord);

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


    public void Reinitialize(Lattice<bool> walls, HexCoord spawnCoord, HexCoord goalCoord,
        VisualizationSettings visualizationSettings)
    {
        ClearData();
        Initialize(walls, spawnCoord, goalCoord, visualizationSettings);
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
            signpost.ExhibitAccent(ArrowAccent.Normal);
        }
    }

    public void ExhibitSignposts(IReadOnlyCollection<FlowSample> flows)
    {
        Debug.Assert(isInitialized);

        Debug.Log("RefreshDistanceLabels");


        foreach (FlowSample flow in flows)
        {
            HexCoord c = flow.Coord;
            Signpost signpost = signposts.At(c);
            signpost.ExhibitSignpost(flow);
            signpost.ExhibitAccent(flow.OnCriticalPath ? ArrowAccent.Path : ArrowAccent.Normal);
        }
    }

    public void SetTerrainAt(HexCoord c, bool isWall)
    {
        Destroy(flagstones.At(c).gameObject);
        MakeFlagstoneAt(c, isWall);
        signposts.At(c).SetIsOnPathableTerrain(!isWall);
    }

    private Flagstone MakeFlagstoneAt(HexCoord c, bool isWall)
    {
        GameObject prefab = isWall ? wallTilePrefab : floorTilePrefab;
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
            signpost.ExhibitSignpost(flow);
        }

        if (shouldHighlight)
        {
            foreach (HexCoord c in frontier)
            {
                Signpost signpost = signposts.At(c);
                signpost.ExhibitAccent(ArrowAccent.Frontier);
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
            signpost.ExhibitAccent(ArrowAccent.Path);
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
