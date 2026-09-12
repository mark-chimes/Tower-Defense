using System.Collections.Generic;

public class TreasureMap
{

    private bool[,] wallMap;
    public readonly int Width;
    public readonly int Height;

    public readonly Coord SpawnPos;
    public readonly Coord GoalPos;


    private Wayfinder wayfinder;

    public TreasureMap(int width, int height, Coord spawnPos, Coord goalPos, bool isStopOnPathFound)
    {
        Width = width;
        Height = height;
        SpawnPos = spawnPos;
        GoalPos = goalPos;
        wallMap = new bool[width, height];
        wayfinder = new Wayfinder(width, height, wallMap, spawnPos, goalPos, Search.Dir.FromEnd, isStopOnPathFound);
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
    }

    // Just used to clear and not do anything - never a required external call
    public void ClearField()
    {
        wayfinder.ClearField();
    }

    public ErfSnapshot At(Coord coord)
    {
        return new ErfSnapshot(coord, spawnGoalKind(coord), wallMap[coord.X, coord.Z]);
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

    public void SetWall(Coord c, bool hasWall) => this.wallMap[c.X, c.Z] = hasWall;

    public bool CanPlaceWall(Coord c) => spawnGoalKind(c) == SpawnGoalKind.Floor
        && !wallMap[c.X, c.Z];

    private SpawnGoalKind spawnGoalKind(Coord coord)
    {
        if (coord == SpawnPos) return SpawnGoalKind.Spawn;
        else if (coord == GoalPos) return SpawnGoalKind.Goal;
        return SpawnGoalKind.Floor;
    }
}


