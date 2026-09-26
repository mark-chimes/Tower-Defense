using UnityEngine;

// TODO split out owning the floor (flagstones) and the markers

public class HexGridView : MonoBehaviour
{
    [SerializeField] private GameObject floorTilePrefab; // TODO rename to water tile or something after code port
    [SerializeField] private GameObject wallTilePrefab; // TODO rename to land tile or something after code port


    // [SerializeField] private GameObject spawnPrefab;
    // [SerializeField] private GameObject goalPrefab;

    [System.Serializable]
    public class VisualizationSettings
    {
        [SerializeField] public bool ShowDistance = false;
        [SerializeField] public bool ShowPathfinding = true;
    }

    // private DirectionMarker[,] directionMarkers;
    private HexMap<GameObject> flagstones;

    private GameObject spawnObj;

    private bool isInitialized;

    // TODO reconsider if this should handle "floors" and "walls" together or not? 
    public void Initialize(HexMap<bool> walls)
    // , HexCoord spawnPos, HexCoord goalPos, VisualizationSettings visualizationSettings) // Later
    {
        Debug.Assert(!isInitialized);
        isInitialized = true;

        flagstones = new HexMap<GameObject>(walls.NumRings);
        foreach (HexCoord coord in flagstones.AllCoords()) 
        {
            GameObject flagstone;
            if (walls.At(coord))     
                flagstone = InstantiateAtCoord(wallTilePrefab, coord); 
            else
                flagstone = InstantiateAtCoord(floorTilePrefab, coord); 
            flagstone.name = $"Flagstone_{coord}"; // TODO rename this
            flagstones.SetAt(coord, flagstone);
            // TODO spawn and goal pos
        }
    }

    public void ClearData()
    {
        Debug.Assert(isInitialized);
        isInitialized = false;

        foreach (Transform child in transform) Destroy(child.gameObject);
        flagstones = null;
    }


    public void Reinitialize(HexMap<bool> walls)
    // HexCoord spawnPos, HexCoord goalPos, VisualizationSettings visualizationSettings)
    {
        ClearData();
        Initialize(walls); // spawnPos, goalPos, visualizationSettings);
    }


    private GameObject InstantiateAtCoord(GameObject prefab, HexCoord coord)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.transform.localPosition = HexLayout.CoordsToWorld(coord);
        return obj;
    }


    // private void UnhighlightAllArrows()
    // {
    //     Debug.Assert(isInitialized);

    //     for (int x = 0; x < layout.Width; x++)
    //     {
    //         for (int z = 0; z < layout.Height; z++)
    //         {
    //             DirectionMarker directionMarker = directionMarkers[x, z];
    //             directionMarker.ExhibitAccent(ArrowAccent.Normal);
    //         }
    //     }

    // }

    // public void RefreshDistanceLabels(IReadOnlyCollection<Signpost> signposts)
    // {
    //     Debug.Assert(isInitialized);

    //     Debug.Log("RefreshDistanceLabels");


    //     foreach (Signpost sign in signposts)
    //     {
    //         Coord c = sign.Coord;
    //         DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
    //         directionMarker.ExhibitSignpost(sign);
    //         directionMarker.ExhibitAccent(sign.OnCriticalPath ? ArrowAccent.Path : ArrowAccent.Normal);
    //     }
    // }

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
