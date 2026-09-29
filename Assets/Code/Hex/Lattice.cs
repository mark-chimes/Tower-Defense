
// See https://www.redblobgames.com/grids/hexagons
// We use an axial coordinate system which provides a cubic interface

using System.Collections.Generic;

public class Lattice<T>
{
    // See https://www.redblobgames.com/grids/hexagons/#map-storage
    // We currently store it using a "wasted space" array using only Q and R
    private T[,] array2D;
    public readonly int NumRings; // 0 rings is a single hex.
    private readonly int arrayWidth; // side of square of array

    public Lattice(int numRings)
    {
        arrayWidth = 2 * numRings + 1;
        array2D = new T[arrayWidth, arrayWidth];
        NumRings = numRings;
    }

    public T At(HexCoord c)
    {
        // We use a square array with wasted space so we have to shift it
        return array2D[c.Q + NumRings, c.R + NumRings];
    }

    public void SetAt(HexCoord c, T t)
    {
        // We use a square array with wasted space so we have to shift it
        array2D[c.Q + NumRings, c.R + NumRings] = t;
    }

    public bool Contains(HexCoord c)
    {
        return c.Length() <= NumRings;
    }

    // fills the entire map with the value t 
    public void Fill(T t)
    {
        for (int q = 0; q < arrayWidth; q++)
            for (int r = 0; r < arrayWidth; r++)
                array2D[q, r] = t;
    }


    public IEnumerable<HexCoord> AllCoords()
    {
        return HexCoord.AllWithinRings(NumRings);
    }

    public IEnumerable<T> All()
    {
        foreach (HexCoord c in HexCoord.AllWithinRings(NumRings))
        {
            yield return At(c);
        }
    }

    // Implementation assumes a square map
    public T[] ToFlatArray()
    {
        T[] flat = new T[arrayWidth * arrayWidth];
        for (int x = 0; x < arrayWidth; x++)
            for (int z = 0; z < arrayWidth; z++)
                flat[z * arrayWidth + x] = array2D[x, z];
        return flat;
    }

    // Implementation assumes a square map
    public static Lattice<T> FromFlatArray(T[] flat, int numRings)
    {
        int arrayWidth = 2 * numRings + 1;
        T[,] array2D = new T[arrayWidth, arrayWidth];
        for (int x = 0; x < arrayWidth; x++)
            for (int z = 0; z < arrayWidth; z++)
                array2D[x, z] = flat[z * arrayWidth + x];
        return new Lattice<T>(array2D, numRings);
    }

    public override string ToString()
    {
        return $"Lattice<{typeof(T).Name}> with Num Rings: {NumRings}";
    }

    private Lattice(T[,] array2D, int numRings)
    {
        this.array2D = array2D;
        arrayWidth = array2D.GetLength(0); //  = 2 * numRings + 1; 
        NumRings = numRings;
    }

}