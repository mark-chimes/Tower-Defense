using System;

public readonly struct CellCoord : IEquatable<CellCoord>
{
    public int X { get; }
    public int Z { get; }

    public CellCoord(int x, int z)
    {
        X = x;
        Z = z;
    }

    public CellCoord Shifted(int x, int z) => new CellCoord(X + x, Z + z);
    public bool InBounds(int width, int height) => X >= 0 && Z >= 0 && X < width && Z < height;

    public override int GetHashCode() => HashCode.Combine(X, Z);
    public override string ToString() => $"({X}, {Z})";

    public bool Equals(CellCoord other) => X == other.X && Z == other.Z;
    public override bool Equals(object obj) => obj is CellCoord c && Equals(c);

    public static bool operator ==(CellCoord a, CellCoord b) => a.Equals(b);
    public static bool operator !=(CellCoord a, CellCoord b) => !a.Equals(b);


    public string ToStringNumbersOnly() => $"{X} {Z}";

}

