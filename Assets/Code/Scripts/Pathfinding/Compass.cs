using System;

public enum Compass
{
    North,
    East,
    South,
    West,
    None,
}

public static class CompassExtension
{
    private static readonly Compass[] allDirs =
    {
        Compass.North,
        Compass.East,
        Compass.South,
        Compass.West,
    };

    public static ReadOnlySpan<Compass> AllDirs => allDirs;

    public static Compass Opposite(this Compass dir) {
        return dir switch
        {
            Compass.North => Compass.South,
            Compass.East =>  Compass.West,
            Compass.South => Compass.North,
            Compass.West =>  Compass.East, 
            _ => Compass.None
        };
    }
}