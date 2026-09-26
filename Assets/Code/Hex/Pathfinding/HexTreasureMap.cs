using System;
using System.Collections.Generic;

public class HexTreasureMap
{

    private HexMap<bool> wallMap;
    public int NumRings => wallMap.NumRings;

    public readonly HexCoord SpawnPos;
    public readonly HexCoord GoalPos;

    private readonly Action onPathfindingUpdate;
    private readonly Action onPathfindingClear;



    private HexWayfinder wayfinder;

    public HexTreasureMap(HexMap<bool> wallMap, HexCoord spawnPos, HexCoord goalPos, bool isStopOnPathFound,
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
        wayfinder = new HexWayfinder(wallMap, SpawnPos, GoalPos, HexSearch.Dir.FromEnd, isStopOnPathFound);
    }

    public IReadOnlyCollection<HexCoord> CurrentFrontier()
    {
        return wayfinder.CurrentFrontier();
    }

    public void SetModeAndClear(HexSearch.Dir searchDir)
    {
        wayfinder = wayfinder.WithNewSearchDir(searchDir);
    }

    public HexSearch.Delta SingleStep()
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

    // TODO check this works if pathfinding is not set
    public HexSnapshot At(HexCoord coord)
    {
        return new HexSnapshot(coord, TileMarkerAt(coord), wallMap.At(coord));
    }

    public HexSignpost SignpostAt(HexCoord coord)
    {
        return wayfinder.SignpostAt(coord);
    }

    public IReadOnlyCollection<HexSignpost> Signposts()
    {
        return wayfinder.Signposts();
    }

    // public HexSignpost SignpostAt(int q, int r) => SignpostAt(new HexCoord(q, r));

    public void SetWall(HexCoord c, bool hasWall) => wallMap.SetAt(c, hasWall);

    public bool HasWall(HexCoord c) => wallMap.At(c);


    public bool CanPlaceWall(HexCoord c) => TileMarkerAt(c) == TileMarker.None && !HasWall(c);

    private TileMarker TileMarkerAt(HexCoord coord)
    {
        if (coord == SpawnPos) return TileMarker.Spawn;
        else if (coord == GoalPos) return TileMarker.Goal;
        return TileMarker.None;
    }

    // public SaveableLevel AsSaveableData()
    // {
    //     return new SaveableLevel(Width, Height, wallMap);
    // }

    public enum TileMarker
    {
        None,
        Spawn,
        Goal,
    }
}


