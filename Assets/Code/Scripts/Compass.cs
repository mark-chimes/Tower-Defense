public enum Cardinal
{
    North,
    East,
    South,
    West,
    None,
}

public static class Compass
{
    public static readonly Cardinal[] AllDirs =
    {
        Cardinal.North,
        Cardinal.East,
        Cardinal.South,
        Cardinal.West,
    };

    public static Cardinal Opposite(this Cardinal dir) {
        return dir switch
        {
            Cardinal.North => Cardinal.South,
            Cardinal.East =>  Cardinal.West,
            Cardinal.South => Cardinal.North,
            Cardinal.West =>  Cardinal.East, 
            _ => Cardinal.None
        };
    }
}