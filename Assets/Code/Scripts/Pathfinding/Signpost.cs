
public readonly struct Signpost
{
    public int DistanceToGoal { get; }

    public Compass DirToGoal { get; }

    public bool OnCriticalPath { get; }

    public readonly Coord Coord;


    // Stepshot should never return the FlowField or any of its components directly.
    public Signpost(Coord coord, FlowField flow)
    {
        Coord = coord;
        DistanceToGoal = flow.DistanceAt(coord);
        DirToGoal = flow.DirectionAt(coord);
        OnCriticalPath = flow.OnCriticalPath(coord);
    }

}
