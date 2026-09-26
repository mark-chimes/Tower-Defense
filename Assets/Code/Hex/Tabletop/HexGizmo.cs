using UnityEngine;

public static class HexGizmo
{
    public enum DiagonalColorMode
    {
        NONE,
        POSITIVE,
        ALL
    }

    private static readonly Color centerColor = Color.darkGray;

    private static readonly Color spawnColor = Color.white;
    private static readonly Color goalColor = Color.gold;


    public static void Draw(HexAuthor hexAuthor, Mesh hexMesh, Transform transform,
         bool showSpawnGoal, bool isWire, DiagonalColorMode diagonalMode, bool colorZeros, bool colorRGB)
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Color originalColor = Gizmos.color;
        Gizmos.matrix = transform.localToWorldMatrix;

        HexCoord spawnCoord = hexAuthor.SpawnCoord;
        HexCoord goalCoord = hexAuthor.GoalCoord;

        foreach (HexCoord coord in HexCoord.AllWithinRings(hexAuthor.NumRings))
        {
            Vector3 pos = HexLayout.CoordsToWorld(coord);
            Gizmos.color = ColorFor(coord, hexAuthor.NumRings, diagonalMode, colorZeros, colorRGB);
            if (showSpawnGoal)
            {
                if (coord == spawnCoord) Gizmos.color = spawnColor;
                if (coord == goalCoord) Gizmos.color = goalColor;
            }

            if (isWire)
                Gizmos.DrawWireMesh(hexMesh, pos);
            else
                Gizmos.DrawMesh(hexMesh, pos);
        }

        Gizmos.matrix = originalMatrix;
        Gizmos.color = originalColor;
    }


    private static Color ColorFor(HexCoord coord, int numRings, DiagonalColorMode mode, bool colorZeros, bool colorRGB)
    {
        return ColorDiagonals(coord, numRings, mode)                    // color diagonals
            ?? (colorZeros ? ColorOnZeroes(coord) : null)               // if null, color zeros
            ?? (colorRGB ? ColorRgb(coord, numRings) : (Color?)null)   // if still null, color rgb
            ?? Color.grey;                                              // otherwise grey
    }

    /**
    ** Positive Q is green
    ** Positive R is blue
    ** Positive S is red
    ** 
    ** East is      turquoise
    ** North-East   blue
    ** North        lavender
    ** North-West   purple
    ** West is      magenta
    ** South-West   Orange
    ** South        Yellow
    ** South-East   Tennis-ball green
    **/
    private static Color ColorRgb(HexCoord c, int N)
    {
        float tQ = Mathf.InverseLerp(-N, N, c.Q);
        float tS = Mathf.InverseLerp(-N, N, c.S);
        float tR = Mathf.InverseLerp(-N, N, c.R);

        return new Color(tS, tQ, tR);
    }

    private static Color? ColorOnZeroes(HexCoord c)
    {
        // Center
        if (c.Q == 0 && c.R == 0)
            return centerColor;

        if (c.Q == 0)
            return Color.mediumSeaGreen;
        if (c.R == 0)
            return Color.steelBlue;
        if (c.S == 0)
            return Color.darkSalmon;

        return null;
    }

    private static Color? ColorDiagonals(HexCoord c, int N, HexGizmo.DiagonalColorMode mode)
    {
        switch (mode)
        {
            case DiagonalColorMode.POSITIVE: return ColorDiagonalsPositive(c, N);
            case DiagonalColorMode.ALL: return ColorDiagonalsAll(c, N);
            default: return null;
        }
    }

    private static Color? ColorDiagonalsAll(HexCoord c, int N)
    {
        // Center
        if (c.Q == 0 && c.R == 0)
            return centerColor;

        if (c.Q == c.R)
        {
            float t = Mathf.InverseLerp(N, -N, c.S) / 2f;
            // diagonal goes from pure red at max S to pink
            return new Color(1.0f, t, t);
        }
        if (c.R == c.S)
        {
            float t = Mathf.InverseLerp(N, -N, c.Q) / 2f;
            // diagonal goes from pure green at max Q to light green
            return new Color(t, 1.0f, t);
        }
        if (c.Q == c.S)
        {
            float t = Mathf.InverseLerp(N, -N, c.R) / 2f;
            // diagonal goes from pure blue at max R to light blue
            return new Color(t, t, 1.0f);
        }
        return null;
    }

    private static Color? ColorDiagonalsPositive(HexCoord c, int N)
    {
        // Center
        if (c.Q == 0 && c.R == 0)
            return centerColor;

        if (c.Q == c.R && c.S > 0)
        {
            float t = Mathf.InverseLerp(N, 0, c.S) / 2f;
            // diagonal goes from pure red at max S to pink
            return new Color(1.0f, t, t);
        }
        if (c.R == c.S && c.Q > 0)
        {
            float t = Mathf.InverseLerp(N, 0, c.Q) / 2f;
            // diagonal goes from pure green at max Q to light green
            return new Color(t, 1.0f, t);
        }
        if (c.Q == c.S && c.R > 0)
        {
            float t = Mathf.InverseLerp(N, 0, c.R) / 2f;
            // diagonal goes from pure blue at max R to light blue
            return new Color(t, t, 1.0f);
        }
        return null;
    }
}
