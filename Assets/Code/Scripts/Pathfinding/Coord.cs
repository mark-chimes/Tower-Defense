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

    public Cardinal DirTo(Coord c)
    {
        int x_diff = c.X - X;
        int z_diff = c.Z - Z;

        if (x_diff == 0 && z_diff == 1)
        {
            return Cardinal.North;
        }
        else if (x_diff == 1 && z_diff == 0)
        {
            return Cardinal.East;
        }
        else if (x_diff == 0 && z_diff == -1)
        {
            return Cardinal.South;
        }
        else if (x_diff == -1 && z_diff == 0)
        {
            return Cardinal.West;
        }
        else
        {
            throw new ArgumentException($"{this} and {c} are not cardinally adjacent");
        }
    }

    public Coord InDirection(Cardinal dir)
    {
        int x_diff = 0;
        int z_diff = 0;

        switch (dir) 
        {
            case Cardinal.North: z_diff = 1; break;
            case Cardinal.East: x_diff = 1;  break;
            case Cardinal.South: z_diff = -1; break;
            case Cardinal.West: x_diff = -1;  break;
            case Cardinal.None: break;  
        }

        return Shifted(x_diff, z_diff);
    }

    public Coord? InDirectionInBoundsNonSelf(Cardinal dir, int width, int height)
    {
        if (dir == Cardinal.None) {
            return null;
        }

        Coord shifted = InDirection(dir);
        if (!shifted.InBounds(width, height))
        {
            return null;
        }
        return shifted;
    }

    // public Cardinal DirFrom(Coord c)
    // {
    //     int x_diff = X - c.X;
    //     int z_diff = Z - c.Z;

    //     if (x_diff == 0 && z_diff == 1)
    //     {
    //         return Cardinal.North;
    //     }
    //     else if (x_diff == 1 && z_diff == 0)
    //     {
    //         return Cardinal.East;
    //     }
    //     else if (x_diff == 0 && z_diff == -1)
    //     {
    //         return Cardinal.South;
    //     }
    //     else if (x_diff == -1 && z_diff == 0)
    //     {
    //         return Cardinal.West;
    //     }
    //     else
    //     {
    //         System.Diagnostics.Debug.Fail($"direction from {this} to {c} was ({x_diff}, {z_diff}");
    //         return Cardinal.None;
    //     }
    // }
}

