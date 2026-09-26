using UnityEngine;

public static class HexGizmo
{

    public enum ColorMode
    {
        NONE,
        COLOR_ON_ZEROES,
        COLOR_RGB,
        COLOR_ALONG_DIR,
        COLOR_RGB_AND_DIR,
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
            Gizmos.color = Color.white;

            if (colorMode == ColorMode.COLOR_ON_ZEROES)
            {
                if (coord.Q == 0)
                    Gizmos.color = Color.green;
                if (coord.R == 0)
                    Gizmos.color = Color.blue;
                if (coord.S == 0)
                    Gizmos.color = Color.magenta;
                // Center
                if (coord.Q == 0 && coord.R == 0)
                    Gizmos.color = Color.yellowNice;
            }
            if (colorMode == ColorMode.COLOR_RGB || colorMode == ColorMode.COLOR_RGB_AND_DIR)
            {
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

                int N = hexAuthor.NumRings;
                float tQ = Mathf.InverseLerp(-N, N, coord.Q);
                float tS = Mathf.InverseLerp(-N, N, coord.S);
                float tR = Mathf.InverseLerp(-N, N, coord.R);

                Gizmos.color = new Color(tS, tQ, tR);
            }
            if (colorMode == ColorMode.COLOR_ALONG_DIR || colorMode == ColorMode.COLOR_RGB_AND_DIR)
            {
                int N = hexAuthor.NumRings;


                float r = 0.5f;
                float g = 0.5f;
                float b = 0.5f;
                if (colorMode != ColorMode.COLOR_RGB_AND_DIR)
                    Gizmos.color = new Color(r, g, b);


                if (coord.Q == coord.R)
                {
                    float t = Mathf.InverseLerp(N, -N, coord.S) / 2f;
                    r = 1.0f;
                    g = t;
                    b = t;
                    Gizmos.color = new Color(r, g, b);

                }
                if (coord.R == coord.S)
                {
                    float t = Mathf.InverseLerp(N, -N, coord.Q)/ 2f;
                    r = t;
                    g = 1.0f;
                    b = t;
                    Gizmos.color = new Color(r, g, b);

                }
                if (coord.Q == coord.S)
                {
                    float t = Mathf.InverseLerp(N, -N, coord.R)/ 2f;
                    r = t;
                    g = t;
                    b = 1.0f;
                    Gizmos.color = new Color(r, g, b);

                }
                if (coord.Q == coord.R && coord.R == coord.S)
                    Gizmos.color = Color.white;
            }


            if (isWire)
                Gizmos.DrawWireMesh(hexMesh, pos);
            else
                Gizmos.DrawMesh(hexMesh, pos);
        }

        Gizmos.matrix = originalMatrix;
        Gizmos.color = originalColor;
    }
}
