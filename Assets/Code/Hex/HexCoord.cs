using System;
using System.Collections.Generic;

// Check https://www.redblobgames.com/grids/hexagons for more info
// We use an axial coordinate system which provides a cubic interface

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

    public override int GetHashCode() => HashCode.Combine(Q, R);
    public override string ToString() => $"({Q}, {R}, {S})";

    public bool Equals(HexCoord other) => Q == other.Q && R == other.R;
    public override bool Equals(object obj) => obj is HexCoord c && Equals(c);

    public static bool operator ==(HexCoord a, HexCoord b) => a.Equals(b);
    public static bool operator !=(HexCoord a, HexCoord b) => !a.Equals(b);

    public int Length()
    {
        return Math.Max(Math.Abs(Q), Math.Max(Math.Abs(R), Math.Abs(S)));
    }

    public HexCoord InDirection(HexCompass dir) => Shifted(dir.Offset());

    
    // See https://www.redblobgames.com/grids/hexagons/#range
    public static IEnumerable<HexCoord> AllWithinRings(int numRings)
    {
        for (int q = -numRings; q <= numRings; q++)
            for (int r = Math.Max(-numRings, -q - numRings); r <= Math.Min(numRings, -q + numRings); r++)
                yield return new HexCoord(q, r);
    }

    // TODO can use something similar to above method for RANGE later
}

