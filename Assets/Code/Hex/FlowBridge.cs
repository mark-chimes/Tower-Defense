using UnityEngine;

public class FlowBridge
{
    private float visualizeFPS = 60f;

    // TODO this is an ugly way of doing this - temp debug only
    private bool shouldHighlightFrontier = false;

    private float visualizeTime;
    private float tempTime;

    private bool isAutoRefreshMode = false;
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
        isAutoRefreshMode = isEnabled;
        if (isAutoRefreshMode)
        {
            isVisualizeMode = false;
            treasureMap.Recompute();
            gridView.ExhibitSignposts(treasureMap.Signposts());
        }
    }

    public void OnFromStartModePressed()
    {
        isVisualizeMode = false;
        treasureMap.SetModeAndClear(HexSearch.Dir.FromStart);
        gridView.ExhibitSignposts(treasureMap.Signposts());
    }

    public void OnFromEndModePressed()
    {
        isVisualizeMode = false;
        treasureMap.SetModeAndClear(HexSearch.Dir.FromEnd);
        gridView.ExhibitSignposts(treasureMap.Signposts());
    }

    public void OnRefreshPressed()
    {
        treasureMap.Recompute();
        gridView.ExhibitSignposts(treasureMap.Signposts());
    }

    public void OnClearFieldPressed()
    {
        isVisualizeMode = false;
        ClearField();
    }

    public void ClearField()
    {
        treasureMap.ClearField();
        gridView.ExhibitSignposts(treasureMap.Signposts());
    }
    // TODO if isAutoRefreshMode is on, this clears the field but never redraws it
    // autorefresh should retrigger after the clear.
    public void OnVisualizePressed()
    {
        ClearField();
        isVisualizeMode = true;
    }

    public void OnSingleStepPressed()
    {
        SingleStep();
    }

    public void OnSetVisualizationVisible(bool isEnabled)
    {
        gridView.SetVisualizationVisible(isEnabled);
    }

    public void OnSetNumbersVisible(bool isEnabled)
    {
        gridView.SetDistanceVisible(isEnabled);
    }

    // Whether this should continue pathfinding after shortest path found or continue to produce a full flow-field
    // TODO later we should have a bunch of settings like the spawn point and goal point that can be moved around etc.
    public void ResetPathfindingWithEarlyStoppingMode(bool isStopOnPathFound)
    {
        treasureMap.RecreateWayfinder(isStopOnPathFound);
        gridView.ExhibitSignposts(treasureMap.Signposts());
    }


    public void ContinuallySingleStep()
    {
        if (isAutoRefreshMode) return;
        if (!isVisualizeMode) return;

        tempTime += Time.deltaTime;
        if (tempTime > visualizeTime)
        {
            tempTime = 0;
            SingleStep();
        }
    }

    private void SingleStep()
    {
        HexSearch.Delta delta = treasureMap.SingleStep();
        RefreshFromDeltaHighlightFrontier(delta);

    }

    public void UpdateDistances()
    {
        if (!isAutoRefreshMode)
        {
            Debug.Log("Auto refresh mode disabled, not updating distances");
        }
        else
        {
            treasureMap.Recompute();
        }
        gridView.ExhibitSignposts(treasureMap.Signposts());
    }


    public void RefreshFromDeltaHighlightFrontier(HexSearch.Delta delta)
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
