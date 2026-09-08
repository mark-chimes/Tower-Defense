public class FlowField
{
    private readonly int[,] distanceToGoal;
    private readonly Compass[,] dirToGoal;

    private readonly bool[,] onCriticalPath;

    public FlowField(
        int[,] distanceToGoal,
        Compass[,] dirToGoal,
        bool[,] onCriticalPath)
    {
        this.distanceToGoal = distanceToGoal;
        this.dirToGoal = dirToGoal;
        this.onCriticalPath = onCriticalPath;
    }

    public int DistanceAt(Coord c) => distanceToGoal[c.X, c.Z];
    public Compass DirectionAt(Coord c) => dirToGoal[c.X, c.Z];
    public bool OnCriticalPath(Coord c) => onCriticalPath[c.X, c.Z];
    public bool Reachable(Coord c) => distanceToGoal[c.X, c.Z] >= 0;

}