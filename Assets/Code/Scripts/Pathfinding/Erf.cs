public class Erf
{
    public readonly Coord Coord;

    public int DistanceToGoal; // TODO move this out of this class

    public ErfKind Kind;
    public bool HasWall;

    public Erf(Coord coord) {
        this.Coord = coord;
        this.DistanceToGoal = -1;
    }
    
}
