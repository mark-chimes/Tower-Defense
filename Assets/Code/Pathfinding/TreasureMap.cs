using System;
using System.Collections.Generic;

public class TreasureMap
{

    private Lattice<bool> wallMap;
    public int NumRings => wallMap.NumRings;

    public readonly HexCoord SpawnPos;
    public readonly HexCoord GoalPos;

    private readonly Action onPathfindingUpdate;
    private readonly Action onPathfindingClear;



    private Wayfinder wayfinder;

    public TreasureMap(Lattice<bool> wallMap, HexCoord spawnPos, HexCoord goalPos, bool isStopOnPathFound,
    Action onPathfindingUpdate, Action onPathfindingClear)
    {
        this.wallMap = wallMap;
        SpawnPos = spawnPos;
        GoalPos = goalPos;
        RecreateWayfinder(isStopOnPathFound);
        this.onPathfindingUpdate = onPathfindingUpdate;
        this.onPathfindingClear = onPathfindingClear;
    }

    /// <summary>
    /// Make a wayfinder from scratch to completely re-do the pathfinding
    /// </summary>
    /// <param name="isStopOnPathFound"></param> stop pathfinding once shortest path found or continue completing the flow-field
    public void RecreateWayfinder(bool isStopOnPathFound)
    {
        wayfinder = new Wayfinder(wallMap, SpawnPos, GoalPos, HexSearch.Dir.FromEnd, isStopOnPathFound);
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

    public HexSnapshot At(HexCoord coord)
    {
        return new HexSnapshot(coord, LandmarkAt(coord), wallMap.At(coord));
    }

    public FlowSample FlowAt(HexCoord coord)
    {
        return wayfinder.FlowAt(coord);
    }

    public IReadOnlyCollection<FlowSample> Flows()
    {
        return wayfinder.Flows();
    }

    public void SetWall(HexCoord c, bool hasWall) => wallMap.SetAt(c, hasWall);

    public bool HasWall(HexCoord c) => wallMap.At(c);


    public bool CanPlaceWall(HexCoord c) => LandmarkAt(c) == Landmark.None && !HasWall(c);

    private Landmark LandmarkAt(HexCoord coord)
    {
        if (coord == SpawnPos) return Landmark.Spawn;
        else if (coord == GoalPos) return Landmark.Goal;
        return Landmark.None;
    }

    public LevelSaveData ToSaveData()
    {
        return new LevelSaveData(wallMap);
    }

    public enum Landmark
    {
        None,
        Spawn,
        Goal,
    }
}


