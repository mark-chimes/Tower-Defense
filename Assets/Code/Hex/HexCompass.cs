using System;

public enum HexCompass
{
    North,
    East,
    South,
    West,
    None,
}

public static class HexCompassExtension
{
    private static readonly HexCompass[] allDirs =
    {
        HexCompass.North,
        HexCompass.East,
        HexCompass.South,
        HexCompass.West,
    };

    public static ReadOnlySpan<HexCompass> AllDirs => allDirs;

    public static HexCompass Opposite(this HexCompass dir) {
        return dir switch
        {
            HexCompass.North => HexCompass.South,
            HexCompass.East =>  HexCompass.West,
            HexCompass.South => HexCompass.North,
            HexCompass.West =>  HexCompass.East, 
            _ => HexCompass.None
        };
    }
}