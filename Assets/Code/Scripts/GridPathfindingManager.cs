using UnityEngine;

[System.Serializable]
public class GridPathfindingManager: GridWalls.IPathfindingCallback
{
    [SerializeField] private float visualizeFPS = 60f;

    // TODO this is an ugly way of doing this - temp debug only
    [SerializeField] private bool shouldHighlightFrontier = false;

    private float visualizeTime;
    private float tempTime;

    private bool isAutoRefreshMode = false;
    private bool isVisualizeMode = false;
    public GridPathfindingManager()
    {
        visualizeTime = 1f / visualizeFPS;
    }

    private TreasureMap treasureMap;
    private GridView gridView;

    public void Initialize(TreasureMap treasureMap, GridView gridView)
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
            gridView.RefreshDistanceLabels(treasureMap.Signposts());
        }
    }

    public void OnFromStartModePressed()
    {
        isVisualizeMode = false;
        treasureMap.SetModeAndClear(Search.Dir.FromStart);
        gridView.RefreshDistanceLabels(treasureMap.Signposts());
    }

    public void OnFromEndModePressed()
    {
        isVisualizeMode = false;
        treasureMap.SetModeAndClear(Search.Dir.FromEnd);
        gridView.RefreshDistanceLabels(treasureMap.Signposts());
    }

    public void OnRefreshPressed()
    {
        treasureMap.Recompute();
        gridView.RefreshDistanceLabels(treasureMap.Signposts());
    }

    public void OnClearFieldPressed()
    {
        isVisualizeMode = false;
        ClearField();
    }

    public void ClearField()
    {
        treasureMap.ClearField();
        gridView.RefreshDistanceLabels(treasureMap.Signposts());
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
        Search.Delta delta = treasureMap.SingleStep();
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
        gridView.RefreshDistanceLabels(treasureMap.Signposts());
    }


    public void RefreshFromDeltaHighlightFrontier(Search.Delta delta)
    {
        Debug.Log("Refresh from delta");


        switch (delta.Phase)
        {
            case Search.Phase.ExpandFrontier:
                {
                    gridView.HighlightChangedOrFrontier(shouldHighlightFrontier, delta.Changed, treasureMap.CurrentFrontier());
                    break;
                }
            case Search.Phase.TracePath:
                {
                    gridView.HighlightPath(delta.Changed);
                    break;
                }
            case Search.Phase.Done:
                {
                    isVisualizeMode = false;
                    tempTime = 0;
                    return;
                }
        }
    }
}
