using System.Collections.Generic;
using UnityEngine;
using static DirectionMarker;


public class GridView : MonoBehaviour
{
    [SerializeField] private DirectionMarker directionMarkerPrefab;
    [SerializeField] private GameObject spawnPrefab;
    [SerializeField] private GameObject goalPrefab;

    [System.Serializable]
    public class VisualizationSettings
    {
        [SerializeField] public bool ShowDistance = false;
        [SerializeField] public bool ShowPathfinding = true;
    }

    private GridLayout layout;
    private DirectionMarker[,] directionMarkers;

    private GameObject spawnObj;

    private bool isInitialized;

    public void Initialize(GridLayout layout, Coord spawnPos, Coord goalPos,
        VisualizationSettings visualizationSettings)
    {
        Debug.Assert(!isInitialized);
        isInitialized = true;

        this.layout = layout;
        directionMarkers = new DirectionMarker[layout.Width, layout.Height];

        for (int x = 0; x < layout.Width; x++)
        {
            for (int z = 0; z < layout.Height; z++)
            {
                Coord coord = new Coord(x, z);

                DirectionMarker directionMarker = Instantiate(directionMarkerPrefab, transform);
                Vector3 pos = layout.CoordsToWorld(coord);
                directionMarker.transform.localPosition = pos;
                directionMarker.name = $"DirectionMarker_{x}_{z}";
                directionMarker.Initialize(coord);
                directionMarker.SetPathingVisible(visualizationSettings.ShowPathfinding);
                directionMarker.SetDistanceVisible(visualizationSettings.ShowDistance);
                directionMarkers[x, z] = directionMarker;

                if (coord == spawnPos)
                {
                    spawnObj = InstantiateMarker(spawnPrefab, coord);
                }
                else if (coord == goalPos)
                {
                    InstantiateMarker(goalPrefab, coord);
                }
            }
        }
    }

    public void ClearData()
    {
        Debug.Assert(isInitialized);
        isInitialized = false;

        foreach (Transform child in transform) Destroy(child.gameObject);
        layout = null;
        directionMarkers = null;
    }


    public void Reinitialize(GridLayout layout, Coord spawnPos, Coord goalPos,
        VisualizationSettings visualizationSettings)
    {
        ClearData();
        Initialize(layout, spawnPos, goalPos, visualizationSettings);
    }


    private GameObject InstantiateMarker(GameObject prefab, Coord coord)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.transform.localPosition = layout.CoordsToWorld(coord);
        return obj;
    }


    private void UnhighlightAllArrows()
    {
        Debug.Assert(isInitialized);

        for (int x = 0; x < layout.Width; x++)
        {
            for (int z = 0; z < layout.Height; z++)
            {
                DirectionMarker directionMarker = directionMarkers[x, z];
                directionMarker.ExhibitAccent(ArrowAccent.Normal);
            }
        }

    }

    public void RefreshDistanceLabels(IReadOnlyCollection<Signpost> signposts)
    {
        Debug.Assert(isInitialized);

        Debug.Log("RefreshDistanceLabels");


        foreach (Signpost sign in signposts)
        {
            Coord c = sign.Coord;
            DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
            directionMarker.ExhibitSignpost(sign);
            directionMarker.ExhibitAccent(sign.OnCriticalPath ? ArrowAccent.Path : ArrowAccent.Normal);
        }
    }

    // TODO the methods below maybe don't cut at the right seams

    public void HighlightChangedOrFrontier(
        bool shouldHighlight,
        IReadOnlyCollection<Signpost> changed,
        IReadOnlyCollection<Coord> frontier)
    {
        Debug.Assert(isInitialized);

        UnhighlightAllArrows();
        foreach (Signpost sign in changed)
        {
            Coord c = sign.Coord;
            DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
            directionMarker.ExhibitSignpost(sign);
        }

        if (shouldHighlight)
        {
            foreach (Coord c in frontier)
            {
                DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
                directionMarker.ExhibitAccent(ArrowAccent.Frontier);
            }
        }
    }

    public void HighlightPath(IReadOnlyCollection<Signpost> changed)
    {
        Debug.Assert(isInitialized);

        foreach (Signpost sign in changed)
        {
            Coord c = sign.Coord;
            DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
            directionMarker.ExhibitAccent(ArrowAccent.Path);
        }
    }

    public void SetVisualizationVisible(bool isVisible)
    {
        Debug.Assert(isInitialized);

        spawnObj.SetActive(isVisible);

        for (int x = 0; x < layout.Width; x++)
        {
            for (int z = 0; z < layout.Height; z++)
            {
                directionMarkers[x, z].SetPathingVisible(isVisible);
            }
        }
    }

    public void SetDistanceVisible(bool isVisible)
    {
        Debug.Assert(isInitialized);

        for (int x = 0; x < layout.Width; x++)
        {
            for (int z = 0; z < layout.Height; z++)
            {
                directionMarkers[x, z].SetDistanceVisible(isVisible);
            }
        }
    }
}
