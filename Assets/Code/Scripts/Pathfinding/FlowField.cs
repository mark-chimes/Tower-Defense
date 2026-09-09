public class FlowField
{

    public readonly int Width;
    public readonly int Height;

    public readonly int[,] distanceToGoal;
    public readonly Compass[,] dirToGoal;
    public readonly bool[,] onCriticalPath;
    
    public FlowField(
        int[,] distanceToGoal,
        Compass[,] dirToGoal,
        bool[,] onCriticalPath)
    {
        this.distanceToGoal = distanceToGoal;
        this.dirToGoal = dirToGoal;
        this.onCriticalPath = onCriticalPath;
    }

    public FlowField(
        int width, int height)
    {
        Width = width;
        Height = height;

        distanceToGoal = new int[Width, Height];
        dirToGoal = new Compass[Width, Height];
        onCriticalPath = new bool[Width, Height];

        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                distanceToGoal[x, z] = -1;
                dirToGoal[x, z] = Compass.None;
                onCriticalPath[x, z] = false;
            }
        }
    }

    public int DistanceAt(Coord c) => distanceToGoal[c.X, c.Z];
    public Compass DirectionAt(Coord c) => dirToGoal[c.X, c.Z];
    public bool OnCriticalPath(Coord c) => onCriticalPath[c.X, c.Z];
    public bool Reachable(Coord c) => distanceToGoal[c.X, c.Z] >= 0;

}