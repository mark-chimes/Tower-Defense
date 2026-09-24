
using System.Collections.Generic;

public class RectMap<T>
{

    private T[,] map;
    public readonly int Width;
    public readonly int Height;

    public RectMap(int width, int height)
    {
        map = new T[width, height];
        Width = width;
        Height = height;
    }

    public T At(Coord c)
    {
        return map[c.X, c.Z];
    }

    public void SetAt(Coord c, T t)
    {
        map[c.X, c.Z] = t;
    }

    public bool IsInBounds(Coord c)
    {
        return (0 <= c.X) && (c.X <= Width) && (0 <= c.Z) && (c.Z <= Height);
    }

    // fills the entire map with the value t 
    public void Fill(T t)
    {
        for (int x = 0; x < Width; x++)
            for (int z = 0; z < Height; z++)
                map[x, z] = t;
    }

    public IEnumerable<Coord> AllCoords()
    {
        for (int x = 0; x < Width; x++)
            for (int z = 0; z < Height; z++)
                yield return new Coord(x, z);
    }

    public T[] MapAsFlatArray()
    {
        T[] flatMap = new T[Width * Height];
        for (int x = 0; x < Width; x++)
            for (int z = 0; z < Height; z++)
                flatMap[z * Width + x] = map[x, z];
        return flatMap;
    }

    public static RectMap<T> MapFromArray(T[] arrayT, int width, int height)
    {
        T[,] mapT = new T[width, height];
        for (int x = 0; x < width; x++)
            for (int z = 0; z < height; z++)
                mapT[x, z] = arrayT[z * width + x];
        return new RectMap<T>(mapT);
    }

    public override string ToString()
    {
        return $"RectMap<{typeof(T).Name}> with Width: {Width}, Height: {Height}"; // TODO can we print type of T?
    }



    private RectMap(T[,] map)
    {
        this.map = map;
        Width = map.GetLength(0);
        Height = map.GetLength(1);
    }



}