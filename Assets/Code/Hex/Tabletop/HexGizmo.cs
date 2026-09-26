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

        int N = hexAuthor.NumRings;

        foreach (HexCoord coord in HexCoord.AllWithinRings(N))
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

    private static Color ColorOnZeroes(HexCoord coord)
    {
        // Center
        if (coord.Q == 0 && coord.R == 0)
            return Color.yellowNice;

        if (coord.Q == 0)
            return Color.green;
        if (coord.R == 0)
            return Color.blue;
        if (coord.S == 0)
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
    private static Color ColorRgb(HexCoord coord, int NumRings)
    {
        int N = NumRings;
        float tQ = Mathf.InverseLerp(-N, N, coord.Q);
        float tS = Mathf.InverseLerp(-N, N, coord.S);
        float tR = Mathf.InverseLerp(-N, N, coord.R);

        return new Color(tS, tQ, tR);
    }

    // Note the optional `Color?`
    private static Color? ColorDiagonals(HexCoord coord, int NumRings)
    {
        // Center
        if (coord.Q == 0 && coord.R == 0)
            return Color.yellowNice;

        int N = NumRings;

        float r = 0.5f;
        float g = 0.5f;
        float b = 0.5f;

        if (coord.Q == coord.R)
        {
            float t = Mathf.InverseLerp(N, -N, coord.S) / 2f;
            // diagonal goes from pure red at max S to pink
            r = 1.0f;
            g = t;
            b = t;
            return new Color(r, g, b);
        }
        if (coord.R == coord.S)
        {
            float t = Mathf.InverseLerp(N, -N, coord.Q) / 2f;
            // diagonal goes from pure green at max Q to light green
            r = t;
            g = 1.0f;
            b = t;
            return new Color(r, g, b);

        }
        if (coord.Q == coord.S)
        {
            float t = Mathf.InverseLerp(N, -N, coord.R) / 2f;
            // diagonal goes from pure blue at max R to light blue
            r = t;
            g = t;
            b = 1.0f;
            return new Color(r, g, b);
        }
        return null;
    }
}
