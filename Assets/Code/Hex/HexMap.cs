
// See https://www.redblobgames.com/grids/hexagons
// We use an axial coordinate system which provides a cubic interface

using System;
using System.Collections.Generic;

public class HexMap<T>
{
    // See https://www.redblobgames.com/grids/hexagons/#map-storage
    // We currently store it using a "wasted space" array using only Q and R
    private T[,] map;
    public readonly int NumRings; // 0 rings is a single hex.
    private readonly int arraySize; // side of square of array

    public HexMap(int numRings)
    {
        arraySize = 2 * numRings + 1;
        map = new T[arraySize, arraySize];
        NumRings = numRings;
    }

    public T At(HexCoord c)
    {
        // We use a square array with wasted space so we have to shift it
        return map[c.Q + NumRings, c.R + NumRings];
    }

    public void SetAt(HexCoord c, T t)
    {
        // We use a square array with wasted space so we have to shift it
        map[c.Q + NumRings, c.R + NumRings] = t;
    }

    public bool IsCoordOnMap(HexCoord c)
    {
        return c.Length() <= NumRings;
    }

    // fills the entire map with the value t 
    public void Fill(T t)
    {
        for (int q = 0; q < arraySize; q++)
            for (int r = 0; r < arraySize; r++)
                map[q, r] = t;
    }


    // See https://www.redblobgames.com/grids/hexagons/#range
    public IEnumerable<HexCoord> AllCoords()
    {
        for (int q = -NumRings; q <= NumRings; q++)
            for (int r = Math.Max(-NumRings, -q - NumRings); r <= Math.Min(NumRings, -q + NumRings); r++)
                yield return new HexCoord(q, r);
    }

    // Implementation assumes a square map
    public T[] MapAsFlatArray()
    {
        T[] flatMap = new T[arraySize * arraySize];
        for (int x = 0; x < arraySize; x++)
            for (int z = 0; z < arraySize; z++)
                flatMap[z * arraySize + x] = map[x, z];
        return flatMap;
    }

    // Implementation assumes a square map
    public static HexMap<T> MapFromArray(T[] arrayT, int numRings)
    {
        int arraySize = 2 * numRings + 1;
        T[,] mapT = new T[arraySize, arraySize];
        for (int x = 0; x < arraySize; x++)
            for (int z = 0; z < arraySize; z++)
                mapT[x, z] = arrayT[z * arraySize + x];
        return new HexMap<T>(mapT, numRings);
    }

    public override string ToString()
    {
        return $"HexMap<{typeof(T).Name}> with Num Rings: {NumRings}";
    }

    private HexMap(T[,] map, int numRings)
    {
        this.map = map;
        arraySize = map.GetLength(0); //  = 2 * numRings + 1; 
        NumRings = numRings; 
    }

}