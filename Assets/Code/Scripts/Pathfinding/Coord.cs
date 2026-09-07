using System;

public readonly struct Coord : IEquatable<Coord>
{
    public int X { get; }
    public int Z { get; }

    public Coord(int x, int z)
    {
        X = x;
        Z = z;
    }

    public Coord Shifted(int x, int z) => new Coord(X + x, Z + z);
    public bool InBounds(int width, int height) => X >= 0 && Z >= 0 && X < width && Z < height;

    public override int GetHashCode() => HashCode.Combine(X, Z);
    public override string ToString() => $"({X}, {Z})";

    public bool Equals(Coord other) => X == other.X && Z == other.Z;
    public override bool Equals(object obj) => obj is Coord c && Equals(c);

    public static bool operator ==(Coord a, Coord b) => a.Equals(b);
    public static bool operator !=(Coord a, Coord b) => !a.Equals(b);

}

