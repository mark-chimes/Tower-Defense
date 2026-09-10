using System.Collections.Generic;
using System.Linq;

public class FlowField
{

    public readonly int Width;
    public readonly int Height;

    public readonly int[,] distance;
    public readonly Compass[,] dirToGoal;
    public readonly bool[,] onCriticalPath;
    
    public FlowField(
        int[,] distanceToGoal,
        Compass[,] dirToGoal,
        bool[,] onCriticalPath)
    {
        this.distance = distanceToGoal;
        this.dirToGoal = dirToGoal;
        this.onCriticalPath = onCriticalPath;
    }

    public FlowField(
        int width, int height)
    {
        Width = width;
        Height = height;

        distance = new int[Width, Height];
        dirToGoal = new Compass[Width, Height];
        onCriticalPath = new bool[Width, Height];

        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                distance[x, z] = -1;
                dirToGoal[x, z] = Compass.None;
                onCriticalPath[x, z] = false;
            }
        }
    }

    public IReadOnlyCollection<Signpost> Signposts() { 
        var list = new List<Signpost>();

        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                if(onCriticalPath[x, z]) {
                    list.Append(new Signpost(new Coord(x,z), this));
                }
            }
        }
        return list;
    }

    public int DistanceAt(Coord c) => distance[c.X, c.Z];
    public Compass DirectionAt(Coord c) => dirToGoal[c.X, c.Z];
    public bool OnCriticalPath(Coord c) => onCriticalPath[c.X, c.Z];
    public bool Reachable(Coord c) => distance[c.X, c.Z] >= 0;

}