using System.Collections.Generic;
using UnityEngine;
using static HexDirectionMarker;

// TODO split out owning the floor (flagstones) and the markers

// TODO maybe this should be called terrain or terrain view or something
public class HexGridView : MonoBehaviour
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

    // private DirectionMarker[,] directionMarkers;
    private HexMap<HexFlagstone> flagstones;
    private HexMap<HexDirectionMarker> directionMarkers;

    private GameObject spawnObj;
    private GameObject goalObj;


    private bool isInitialized;

    // TODO reconsider if this should handle "floors" and "walls" together or not? 
    public void Initialize(HexMap<bool> walls, HexCoord spawnCoord, HexCoord goalCoord)
    // , HexCoord spawnPos, HexCoord goalPos, VisualizationSettings visualizationSettings) // Later
    {
        Debug.Assert(!isInitialized);
        isInitialized = true;

        flagstones = new HexMap<HexFlagstone>(walls.NumRings);
        directionMarkers = new HexMap<HexDirectionMarker>(walls.NumRings);

        foreach (HexCoord coord in flagstones.AllCoords())
        {
            GameObject flagstoneObj;
            GameObject signpostObj = InstantiateAtCoord(signpostPrefab, coord);
            HexDirectionMarker directionMarker = signpostObj.GetComponent<HexDirectionMarker>();
            directionMarker.Initialize(coord);
            directionMarker.SetIsOnPathableTerrain(!walls.At(coord));
            directionMarker.SetPathingVisible(true);
            directionMarker.SetDistanceVisible(true);

            if (walls.At(coord))
                flagstoneObj = InstantiateAtCoord(wallTilePrefab, coord);
            else
                flagstoneObj = InstantiateAtCoord(floorTilePrefab, coord);

            flagstoneObj.name = $"Flagstone_{coord}"; // TODO rename this
            HexFlagstone flagstone = flagstoneObj.GetComponentInChildren<HexFlagstone>();
            flagstone.Initialize(coord);
            flagstones.SetAt(coord, flagstone);

            directionMarker.name = $"Signpost_{coord}"; // TODO rename this
            directionMarkers.SetAt(coord, directionMarker);

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


    public void Reinitialize(HexMap<bool> walls, HexCoord spawnCoord, HexCoord goalCoord)
    // HexCoord spawnPos, HexCoord goalPos, VisualizationSettings visualizationSettings)
    {
        ClearData();
        Initialize(walls, spawnCoord, goalCoord); // visualizationSettings);
    }


    private GameObject InstantiateAtCoord(GameObject prefab, HexCoord coord)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.transform.localPosition = HexLayout.CoordsToWorld(coord);
        return obj;
    }

    private void UnhighlightAllArrows()
    {
        Debug.Assert(isInitialized);
        foreach (HexDirectionMarker marker in directionMarkers.All())
        {
            marker.ExhibitAccent(ArrowAccent.Normal);
        }
    }

    public void ExhibitSignposts(IReadOnlyCollection<HexSignpost> signposts)
    {
        Debug.Assert(isInitialized);

        Debug.Log("RefreshDistanceLabels");


        foreach (HexSignpost sign in signposts)
        {
            HexCoord c = sign.Coord;
            HexDirectionMarker directionMarker = directionMarkers.At(c);
            directionMarker.ExhibitSignpost(sign);
            directionMarker.ExhibitAccent(sign.OnCriticalPath ? ArrowAccent.Path : ArrowAccent.Normal);
        }
    }

    public void SetTerrainAt(HexCoord c, bool isWall)
    {

        Destroy(flagstones.At(c).gameObject);
        GameObject flagstoneObj =
        isWall ?
            InstantiateAtCoord(wallTilePrefab, c)
        :
            InstantiateAtCoord(floorTilePrefab, c);
        flagstoneObj.name = $"Flagstone_{c}"; // TODO rename this?
        HexFlagstone flagstone = flagstoneObj.GetComponentInChildren<HexFlagstone>();
        flagstone.Initialize(c);
        flagstones.SetAt(c, flagstone);
        directionMarkers.At(c).SetIsOnPathableTerrain(!isWall);
    }

    // TODO the methods below maybe don't cut at the right seams

    // public void HighlightChangedOrFrontier(
    //     bool shouldHighlight,
    //     IReadOnlyCollection<Signpost> changed,
    //     IReadOnlyCollection<Coord> frontier)
    // {
    //     Debug.Assert(isInitialized);

    //     UnhighlightAllArrows();
    //     foreach (Signpost sign in changed)
    //     {
    //         Coord c = sign.Coord;
    //         DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
    //         directionMarker.ExhibitSignpost(sign);
    //     }

    //     if (shouldHighlight)
    //     {
    //         foreach (Coord c in frontier)
    //         {
    //             DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
    //             directionMarker.ExhibitAccent(ArrowAccent.Frontier);
    //         }
    //     }
    // }

    // public void HighlightPath(IReadOnlyCollection<Signpost> changed)
    // {
    //     Debug.Assert(isInitialized);

    //     foreach (Signpost sign in changed)
    //     {
    //         Coord c = sign.Coord;
    //         DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
    //         directionMarker.ExhibitAccent(ArrowAccent.Path);
    //     }
    // }

    // public void SetVisualizationVisible(bool isVisible)
    // {
    //     Debug.Assert(isInitialized);

    //     spawnObj.SetActive(isVisible);

    //     for (int x = 0; x < layout.Width; x++)
    //     {
    //         for (int z = 0; z < layout.Height; z++)
    //         {
    //             directionMarkers[x, z].SetPathingVisible(isVisible);
    //         }
    //     }
    // }

    // public void SetDistanceVisible(bool isVisible)
    // {
    //     Debug.Assert(isInitialized);

    //     for (int x = 0; x < layout.Width; x++)
    //     {
    //         for (int z = 0; z < layout.Height; z++)
    //         {
    //             directionMarkers[x, z].SetDistanceVisible(isVisible);
    //         }
    //     }
    // }
}
