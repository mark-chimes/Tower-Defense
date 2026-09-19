using System;

public readonly struct HexCoord : IEquatable<HexCoord>
{
    public readonly int Q;
    public readonly int R;

    public readonly int S;

    public HexCoord(int q, int r)
    {
        Q = q;
        R = r;
        S = -q - r;
    }

    public HexCoord ShiftedAxial(int q, int r) => new HexCoord(Q + q, R + r);
    public HexCoord Shifted(HexCoord shift) => new HexCoord(Q + shift.Q, R + shift.R);

    public HexCoord[] Neighbours()
    {
        HexCoord[] neighbors = new HexCoord[6];
        for (int i = 0; i < 6; i++)
        {
            neighbors[i] = Shifted(HexCompassExtension.AllDirs[i].Offset());
        }
        return neighbors;
    }

    // Mirrored across the origin
    public HexCoord Negative()
    {
        return new HexCoord(-Q, -R);
    }

    // public bool InBounds(int width, int height) => X >= 0 && Z >= 0 && X < width && Z < height;

    public override int GetHashCode() => HashCode.Combine(Q, R);
    public override string ToString() => $"({Q}, {R}, {S})";

    public bool Equals(HexCoord other) => Q == other.Q && R == other.R;
    public override bool Equals(object obj) => obj is HexCoord c && Equals(c);

    public static bool operator ==(HexCoord a, HexCoord b) => a.Equals(b);
    public static bool operator !=(HexCoord a, HexCoord b) => !a.Equals(b);

    // public HexCoord InDirection(HexCompass dir) 
    // {
    //     int x_diff = 0;
    //     int z_diff = 0;

    //     switch (dir) 
    //     {
    //         case HexCompass.North: z_diff = 1; break;
    //         case HexCompass.East: x_diff = 1;  break;
    //         case HexCompass.South: z_diff = -1; break;
    //         case HexCompass.West: x_diff = -1;  break;
    //         case HexCompass.None: break;  
    //     }

    //     return Shifted(x_diff, z_diff);
    // }

    // TODO should this be a Coord? 
    // public static HexCoord ForDirection(HexCompass dir) 
    // {
    //     int x_diff = 0;
    //     int z_diff = 0;

    //     switch (dir) 
    //     {
    //         case HexCompass.North: z_diff = 1; break;
    //         case HexCompass.East: x_diff = 1;  break;
    //         case HexCompass.South: z_diff = -1; break;
    //         case HexCompass.West: x_diff = -1;  break;
    //         case HexCompass.None: break;  
    //     }

    //     return new HexCoord(x_diff, z_diff);
    // }

    // public HexCoord? InDirectionInBoundsNonSelf(HexCompass dir, int width, int height)
    // {
    //     if (dir == HexCompass.None) {
    //         return null;
    //     }

    //     HexCoord shifted = InDirection(dir);
    //     if (!shifted.InBounds(width, height))
    //     {
    //         return null;
    //     }
    //     return shifted;
    // }
}

