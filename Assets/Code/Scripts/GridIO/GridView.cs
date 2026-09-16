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


    public void GenerateGridView(GridLayout layout, Coord spawnPos, Coord goalPos,
        VisualizationSettings visualizationSettings)
    {
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
                    InstantiateMarker(spawnPrefab, coord);
                }
                else if (coord == goalPos)
                {
                    InstantiateMarker(goalPrefab, coord);
                }
            }
        }
    }

    private void InstantiateMarker(GameObject prefab, Coord coord)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.transform.localPosition = layout.CoordsToWorld(coord);
    }


    private void UnhighlightAllArrows()
    {
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
        Debug.Log("RefreshDistanceLabels");

        foreach (Signpost sign in signposts)
        {
            Coord c = sign.Coord;
            DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
            directionMarker.ExhibitSignpost(sign);
            directionMarker.ExhibitAccent(sign.OnCriticalPath ? ArrowAccent.Path : ArrowAccent.Normal);
        }
    }

    // TODO the methods below probably don't cut at the right seams

    public void HighlightChangedOrFrontier(
        bool shouldHighlight,
        IReadOnlyCollection<Signpost> changed,
        IReadOnlyCollection<Coord> frontier)
    {
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
        foreach (Signpost sign in changed)
        {
            Coord c = sign.Coord;
            DirectionMarker directionMarker = directionMarkers[c.X, c.Z];
            directionMarker.ExhibitAccent(ArrowAccent.Path);
        }
    }

    public void SetVisualizationVisible(bool isVisible)
    {
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
        for (int x = 0; x < layout.Width; x++)
        {
            for (int z = 0; z < layout.Height; z++)
            {
                directionMarkers[x, z].SetDistanceVisible(isVisible);
            }
        }
    }
}
