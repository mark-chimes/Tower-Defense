using System;
using System.Collections.Generic;

public class TreasureMap
{

    private Lattice<bool> landMap;
    public int NumRings => landMap.NumRings;

    public readonly HexCoord SpawnCoord;
    public readonly HexCoord GoalCoord;

    private readonly Action onPathfindingUpdate;
    private readonly Action onPathfindingClear;
    private readonly Action<HexCoord> onTerrainChange;


    private Wayfinder wayfinder;

    public TreasureMap(Lattice<bool> landMap, HexCoord spawnCoord, HexCoord goalCoord, bool isStopOnPathFound,
    Action onPathfindingUpdate, Action onPathfindingClear, Action<HexCoord> onTerrainChange)
    {
        this.landMap = landMap;
        SpawnCoord = spawnCoord;
        GoalCoord = goalCoord;
        RecreateWayfinder(isStopOnPathFound);
        this.onPathfindingUpdate = onPathfindingUpdate;
        this.onPathfindingClear = onPathfindingClear;
        this.onTerrainChange = onTerrainChange;
    }

    /// <summary>
    /// Make a wayfinder from scratch to completely re-do the pathfinding
    /// </summary>
    /// <param name="isStopOnPathFound"></param> stop pathfinding once shortest path found or continue completing the flow-field
    public void RecreateWayfinder(bool isStopOnPathFound)
    {
        wayfinder = new Wayfinder(landMap, SpawnCoord, GoalCoord, HexSearch.Dir.FromEnd, isStopOnPathFound);
    }

    public IReadOnlyCollection<HexCoord> CurrentFrontier()
    {
        return wayfinder.CurrentFrontier();
    }

    public void SetModeAndClear(HexSearch.Dir searchDir)
    {
        wayfinder = wayfinder.WithNewSearchDir(searchDir);
    }

    public HexSearch.Delta AdvanceSearch()
    {
        return wayfinder.ComputeSingleStep();
    }

    public void Recompute()
    {
        wayfinder.ClearField();
        wayfinder.ComputeFlow();
        onPathfindingUpdate.Invoke();
    }

    // External functions can assume internal functions will call this if they have to
    public void ClearField()
    {
        wayfinder.ClearField();
        onPathfindingClear.Invoke();
    }

    public HexCompass DirectionAt(HexCoord coord) 
    {
        return FlowAt(coord).DirToGoal;
    }

    public FlowSample FlowAt(HexCoord coord)
    {
        return wayfinder.FlowAt(coord);
    }

    public IReadOnlyCollection<FlowSample> Flows()
    {
        return wayfinder.Flows();
    }

    public void SetLand(HexCoord c, bool isLand) 
    {
        landMap.SetAt(c, isLand);
        onTerrainChange.Invoke(c);
    }

    public bool IsLand(HexCoord c) => landMap.At(c);

    public bool Contains(HexCoord c) => landMap.Contains(c);


    public bool CanPlaceLand(HexCoord c) => LandmarkAt(c) == Landmark.None && !IsLand(c);

    private Landmark LandmarkAt(HexCoord coord)
    {
        if (coord == SpawnCoord) return Landmark.Spawn;
        else if (coord == GoalCoord) return Landmark.Goal;
        return Landmark.None;
    }

    public TerrainSaveData GetSaveData()
    {
        return new TerrainSaveData(landMap);
    }

    private enum Landmark
    {
        None,
        Spawn,
        Goal,
    }
}


