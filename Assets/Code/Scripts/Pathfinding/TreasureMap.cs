using System;
using System.Collections.Generic;

public class TreasureMap
{

    private bool[,] wallMap; // TODO I'm starting to think TreasureMap should not hold the wall map
    public readonly int Width;
    public readonly int Height;

    public readonly Coord SpawnPos;
    public readonly Coord GoalPos;

    private readonly Action onPathfindingUpdate;
    private readonly Action onPathfindingClear;



    private Wayfinder wayfinder;

    public TreasureMap(int width, int height, bool[,] wallMap, Coord spawnPos, Coord goalPos, bool isStopOnPathFound,
    Action onPathfindingUpdate, Action onPathfindingClear)
    {
        Width = width;
        Height = height;
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
        wayfinder = new Wayfinder(Width, Height, wallMap, SpawnPos, GoalPos, Search.Dir.FromEnd, isStopOnPathFound);
    }

    public IReadOnlyCollection<Coord> CurrentFrontier()
    {
        return wayfinder.CurrentFrontier();
    }

    public void SetModeAndClear(Search.Dir searchDir)
    {
        wayfinder = wayfinder.WithNewSearchDir(searchDir);
    }

    public Search.Delta SingleStep()
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
    public ErfSnapshot At(Coord coord)
    {
        return new ErfSnapshot(coord, SpawnGoalKindAt(coord), wallMap[coord.X, coord.Z]);
    }

    public Signpost SignpostAt(Coord coord)
    {
        return wayfinder.SignpostAt(coord);
    }

    public IReadOnlyCollection<Signpost> Signposts()
    {
        return wayfinder.Signposts();
    }

    public Signpost SignpostAt(int x, int z) => SignpostAt(new Coord(x, z));

    public void SetWall(Coord c, bool hasWall) => wallMap[c.X, c.Z] = hasWall;

    public bool HasWall(Coord c) => wallMap[c.X, c.Z];


    public bool CanPlaceWall(Coord c) => SpawnGoalKindAt(c) == SpawnGoalKind.Floor && !HasWall(c);

    private SpawnGoalKind SpawnGoalKindAt(Coord coord)
    {
        if (coord == SpawnPos) return SpawnGoalKind.Spawn;
        else if (coord == GoalPos) return SpawnGoalKind.Goal;
        return SpawnGoalKind.Floor;
    }

    public SaveableLevel AsSaveableData()
    {
        return new SaveableLevel(Width, Height, wallMap);
    }
}


