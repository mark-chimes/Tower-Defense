using System.Collections.Generic;

public class TreasureMap
{

    private Erf[,] map;


    private Wayfinder wayfinder;

    public TreasureMap(int width, int height, Coord spawnPos, Coord goalPos, bool isStopOnPathFound)
    {
        map = new Erf[width, height];
        wayfinder = new Wayfinder(width, height, spawnPos, goalPos, Search.Dir.FromEnd, isStopOnPathFound);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Coord coord = new Coord(x, z);
                Erf erf = new Erf(coord);

                if (coord == spawnPos)
                {
                    erf.Kind = ErfKind.Spawn;
                }
                else if (coord == goalPos)
                {
                    erf.Kind = ErfKind.Goal;
                }
                map[x, z] = erf;
            }
        }

        Recompute();
    }

    public IReadOnlyCollection<Coord> CurrentFrontier() { 
        return wayfinder.CurrentFrontier();
    }

    public void SetModeAndClear(Search.Dir searchDir)
    {
        wayfinder = wayfinder.WithNewSearchDir(searchDir);
        ClearField(); // code smell: feels weird I have to clear field after spawning new one
    }

    public Search.Delta SingleStep()
    {
        return wayfinder.ComputeSingleStep(map);
    }

    public void Recompute()
    {
        ClearField();
        wayfinder.ComputeFlow(map);
    }

    // TODO code smell
    public void ClearField()
    {
        wayfinder.ClearField();
    }

    public int Width()
    {
        return wayfinder.Width;
    }

    public int Height()
    {
        return wayfinder.Height;
    }


    public ErfSnapshot At(Coord coord)
    {
        Erf data = map[coord.X, coord.Z];
        return new ErfSnapshot(coord, data.Kind, data.HasWall);
    }

    // TODO Deprecate this? 
    public ErfSnapshot At(int x, int z) => At(new Coord(x, z));

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

    public bool CanPlaceWall(Coord c) => map[c.X, c.Z].Kind == ErfKind.Floor
        && !map[c.X, c.Z].HasWall;

}


