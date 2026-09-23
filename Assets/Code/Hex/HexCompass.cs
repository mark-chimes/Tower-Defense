using System;

public enum HexCompass
{
    NONE,
    E,
    NE,
    NW,
    W,
    SW,
    SE,
}

public static class HexCompassExtension
{
    private static readonly HexCompass[] allDirs =
    {
        HexCompass.E,
        HexCompass.NE,
        HexCompass.NW,
        HexCompass.W,
        HexCompass.SW,
        HexCompass.SE,
    };


    public static ReadOnlySpan<HexCompass> AllDirs => allDirs;

    public static HexCoord Offset(this HexCompass dir) => dir switch
    {
        HexCompass.E => new HexCoord(+1, 0),
        HexCompass.NE => new HexCoord(0, +1),
        HexCompass.NW => new HexCoord(-1, +1),
        HexCompass.W => new HexCoord(-1, 0),
        HexCompass.SW => new HexCoord(0, -1),
        HexCompass.SE => new HexCoord(+1, -1),
        _ => throw new ArgumentOutOfRangeException(nameof(dir)),
    };

    public static HexCompass Opposite(this HexCompass dir) => dir switch
    {
        HexCompass.E => HexCompass.W,
        HexCompass.NE => HexCompass.SW,
        HexCompass.NW => HexCompass.SE,
        HexCompass.W => HexCompass.E,
        HexCompass.SW => HexCompass.NE,
        HexCompass.SE => HexCompass.NW,
        _ => throw new ArgumentOutOfRangeException(nameof(dir)),
    };

}