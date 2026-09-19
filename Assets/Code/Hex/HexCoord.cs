using System;

public readonly struct HexCoord : IEquatable<Coord>
{
    public int X { get; }
    public int Z { get; }

    public HexCoord(int x, int z)
    {
        X = x;
        Z = z;
    }

    public HexCoord Shifted(int x, int z) => new HexCoord(X + x, Z + z);
    public HexCoord Shifted(HexCoord shift) => new HexCoord(X + shift.X, Z + shift.Z);

    public bool InBounds(int width, int height) => X >= 0 && Z >= 0 && X < width && Z < height;

    public override int GetHashCode() => HashCode.Combine(X, Z);
    public override string ToString() => $"({X}, {Z})";

    public bool Equals(Coord other) => X == other.X && Z == other.Z;
    public override bool Equals(object obj) => obj is Coord c && Equals(c);

    public static bool operator ==(HexCoord a, HexCoord b) => a.Equals(b);
    public static bool operator !=(HexCoord a, HexCoord b) => !a.Equals(b);

    public HexCoord InDirection(Compass dir) 
    {
        int x_diff = 0;
        int z_diff = 0;

        switch (dir) 
        {
            case Compass.North: z_diff = 1; break;
            case Compass.East: x_diff = 1;  break;
            case Compass.South: z_diff = -1; break;
            case Compass.West: x_diff = -1;  break;
            case Compass.None: break;  
        }

        return Shifted(x_diff, z_diff);
    }

    // TODO should this be a Coord? 
    public static HexCoord ForDirection(Compass dir) 
    {
        int x_diff = 0;
        int z_diff = 0;

        switch (dir) 
        {
            case Compass.North: z_diff = 1; break;
            case Compass.East: x_diff = 1;  break;
            case Compass.South: z_diff = -1; break;
            case Compass.West: x_diff = -1;  break;
            case Compass.None: break;  
        }

        return new HexCoord(x_diff, z_diff);
    }

    public HexCoord? InDirectionInBoundsNonSelf(Compass dir, int width, int height)
    {
        if (dir == Compass.None) {
            return null;
        }

        HexCoord shifted = InDirection(dir);
        if (!shifted.InBounds(width, height))
        {
            return null;
        }
        return shifted;
    }
}

