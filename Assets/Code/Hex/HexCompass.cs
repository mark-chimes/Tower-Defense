using System;

public enum HexCompass
{
    R,
    UR,
    UL,
    L,
    DL,
    DR,
    NONE,
}

public static class HexCompassExtension
{
    private static readonly HexCompass[] allDirs =
    {
        HexCompass.R,
        HexCompass.UR,
        HexCompass.UL,
        HexCompass.L,
        HexCompass.DL,
        HexCompass.DR,
    };

    
    public static ReadOnlySpan<HexCompass> AllDirs => allDirs;
    
    public static HexCoord Offset(this HexCompass dir) => dir switch
    {
        HexCompass.R => new HexCoord(+1, 0),
        HexCompass.UR => new HexCoord(0, +1),
        HexCompass.UL => new HexCoord(-1, +1),
        HexCompass.L => new HexCoord(-1, 0),
        HexCompass.DL => new HexCoord(0, -1),
        HexCompass.DR => new HexCoord(+1, -1),
        _ => throw new ArgumentOutOfRangeException(nameof(dir)),
    };

    public static HexCompass Opposite(this HexCompass dir) => dir switch
    {
        HexCompass.R => HexCompass.L,
        HexCompass.UR => HexCompass.DL,
        HexCompass.UL => HexCompass.DR,
        HexCompass.L => HexCompass.R,
        HexCompass.DL => HexCompass.UR,
        HexCompass.DR => HexCompass.UL,
        _ => throw new ArgumentOutOfRangeException(nameof(dir)),
    };

}