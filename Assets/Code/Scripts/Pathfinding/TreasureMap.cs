using System.Collections.Generic;

public class TreasureMap
{

    private Erf[,] map;
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
        map = new Erf[width, height];
        wayfinder = new Wayfinder(width, height, spawnPos, goalPos, Search.Dir.FromEnd, isStopOnPathFound);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Coord coord = new Coord(x, z);
                Erf erf = new Erf(coord);

                map[x, z] = erf;
            }
        }

        wayfinder.ComputeFlow(map);
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
        return wayfinder.ComputeSingleStep(map);
    }

    public void Recompute()
    {
        wayfinder.ClearField();
        wayfinder.ComputeFlow(map);
    }

    // Just used to clear and not do anything - never a required external call
    public void ClearField()
    {
        wayfinder.ClearField();
    }

    public ErfSnapshot At(Coord coord)
    {
        Erf data = map[coord.X, coord.Z];
        return new ErfSnapshot(coord, erfKind(coord), data.HasWall);
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

    public void SetWall(Coord c, bool hasWall) => map[c.X, c.Z].HasWall = hasWall;

    public bool CanPlaceWall(Coord c) => erfKind(c) == ErfKind.Floor
        && !map[c.X, c.Z].HasWall;

    private ErfKind erfKind(Coord coord)
    {
        if (coord == SpawnPos) return ErfKind.Spawn;
        else if (coord == GoalPos) return ErfKind.Goal;
        return ErfKind.Floor;
    }
}


