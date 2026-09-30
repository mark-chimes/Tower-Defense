using UnityEngine;

public class FlowBridge
{
    private float visualizeFPS = 60f;

    // TODO this is an ugly way of doing this - temp debug only
    private bool shouldHighlightFrontier = false;

    private float visualizeTime;
    private float tempTime;

    public bool IsAutoRefreshMode { get; private set; } = false;
    private bool isVisualizeMode = false;


    public FlowBridge()
    {
        visualizeTime = 1f / visualizeFPS;
    }

    private TreasureMap treasureMap;
    private TerrainWithOverlay gridView;



    public void Initialize(TreasureMap treasureMap, TerrainWithOverlay gridView)
    {
        this.treasureMap = treasureMap;
        this.gridView = gridView;
    }

    public void SetAutoRefreshMode(bool isEnabled)
    {
        IsAutoRefreshMode = isEnabled;
        if (IsAutoRefreshMode)
        {
            isVisualizeMode = false;
            Refresh();
        }
    }

    public void SetSearchDir(HexSearch.Dir dir)
    {
        isVisualizeMode = false;
        treasureMap.SetModeAndClear(dir);
        gridView.ShowFlows(treasureMap.Flows());
        RefreshIfAutoRefresh();
    }

    public void RefreshIfAutoRefresh()
    {
        if (!IsAutoRefreshMode) return;
        Refresh();
    }

    public void Refresh()
    {
        treasureMap.Recompute();
        gridView.ShowFlows(treasureMap.Flows());
    }

    public void ClearField()
    {
        isVisualizeMode = false;
        ClearFieldAndRedraw();
    }

    private void ClearFieldAndRedraw()
    {
        treasureMap.ClearField();
        gridView.ShowFlows(treasureMap.Flows());
    }

    public void StartVisualize()
    {
        ClearFieldAndRedraw();
        IsAutoRefreshMode = false;
        isVisualizeMode = true;
    }

    public void SetVisualizationVisible(bool isEnabled) => gridView.SetVisualizationVisible(isEnabled);
    public void SetNumbersVisible(bool isEnabled) => gridView.SetDistanceVisible(isEnabled);

    // Whether this should continue pathfinding after shortest path found or continue to produce a full flow-field
    // TODO later we should have a bunch of settings like the spawn point and goal point that can be moved around etc.
    public void ResetPathfindingWithEarlyStoppingMode(bool isStopOnPathFound)
    {
        treasureMap.RecreateWayfinder(isStopOnPathFound);
        gridView.ShowFlows(treasureMap.Flows());
        RefreshIfAutoRefresh();
    }


    public void ContinuallySingleStep()
    {
        if (IsAutoRefreshMode) return;
        if (!isVisualizeMode) return;

        tempTime += Time.deltaTime;
        if (tempTime > visualizeTime)
        {
            tempTime = 0;
            SingleStep();
        }
    }

    public void SingleStep()
    {
        HexSearch.Delta delta = treasureMap.AdvanceSearch();
        RefreshFromDeltaHighlightFrontier(delta);
    }

    public void OnWallChange(HexCoord c)
    {
        gridView.SetTerrainAt(c, treasureMap.HasWall(c));

        if (!IsAutoRefreshMode)
        {
            Debug.Log("Auto refresh mode disabled, not updating distances");
        }
        else
        {
            treasureMap.Recompute();
        }
        gridView.ShowFlows(treasureMap.Flows());
    }

    private void RefreshFromDeltaHighlightFrontier(HexSearch.Delta delta)
    {
        Debug.Log("Refresh from delta");


        switch (delta.Phase)
        {
            case HexSearch.Phase.ExpandFrontier:
                {
                    gridView.HighlightChangedOrFrontier(shouldHighlightFrontier, delta.Changed, treasureMap.CurrentFrontier());
                    break;
                }
            case HexSearch.Phase.TracePath:
                {
                    gridView.HighlightPath(delta.Changed);
                    break;
                }
            case HexSearch.Phase.Done:
                {
                    isVisualizeMode = false;
                    tempTime = 0;
                    return;
                }
        }
    }
}
