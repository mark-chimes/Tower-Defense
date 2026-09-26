using UnityEngine;

public static class HexGizmo
{

    public enum ColorMode
    {
        NONE,
        COLOR_ZEROES,
        COLOR_RGB,
        COLOR_DIAGONALS,
        COLOR_RGB_AND_DIAGONALS,
    }


    public static void Draw(HexAuthor hexAuthor, Mesh hexMesh, Transform transform,
        bool isWire, ColorMode colorMode)
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Color originalColor = Gizmos.color;
        Gizmos.matrix = transform.localToWorldMatrix;

        foreach (HexCoord coord in HexCoord.AllWithinRings(hexAuthor.NumRings))
        {
            Vector3 pos = HexLayout.CoordsToWorld(coord);
            Gizmos.color = ColorFor(coord, hexAuthor.NumRings, colorMode);

            if (isWire)
                Gizmos.DrawWireMesh(hexMesh, pos);
            else
                Gizmos.DrawMesh(hexMesh, pos);
        }

        Gizmos.matrix = originalMatrix;
        Gizmos.color = originalColor;
    }
    private static Color ColorFor(HexCoord coord, int numRings, ColorMode mode) => mode switch
    {
        ColorMode.COLOR_ZEROES => ColorOnZeroes(coord),
        ColorMode.COLOR_RGB => ColorRgb(coord, numRings),
        ColorMode.COLOR_DIAGONALS => ColorDiagonals(coord, numRings) ?? Color.grey,
        ColorMode.COLOR_RGB_AND_DIAGONALS => ColorDiagonals(coord, numRings) ?? ColorRgb(coord, numRings),
        _ => Color.grey,
    };

    private static Color ColorOnZeroes(HexCoord c)
    {
        // Center
        if (c.Q == 0 && c.R == 0)
            return Color.yellowNice;

        if (c.Q == 0)
            return Color.green;
        if (c.R == 0)
            return Color.blue;
        if (c.S == 0)
            return Color.magenta;

        return Color.grey;
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

    // Note the optional `Color?`
    private static Color? ColorDiagonals(HexCoord c, int N)
    {
        // Center
        if (c.Q == 0 && c.R == 0)
            return Color.yellowNice;

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
}
