
public class WallMap
{

    private bool[,] map;
    private int width;
    private int height;

    public WallMap(int Width, int Height)
    {
        map = new bool[Width, Height];
        width = Width;
        height = Height;
    }

    public bool HasWall(Coord c)
    {
        return map[c.X, c.Z];
    }

    public void SetWall(Coord c, bool isWall)
    {
        map[c.X, c.Z] = isWall;
    }

    public bool[] FlattenedWallMap()
    {
        bool[] flatMap = new bool[width * height];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                flatMap[z * width + x] = map[x, z];
            }
        }
        return flatMap;
    }

    public static WallMap FromFlatMap(bool[] flatMap, int Width, int Height)
    {
        bool[,] wallMap = new bool[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                wallMap[x, z] = flatMap[z * Width + x];
            }
        }
        return new WallMap(wallMap, Width, Height);
    }

    private WallMap(bool[,] map, int Width, int Height)
    {
        this.map = map;
        width = Width;
        height = Height;
    }

    public override string ToString()
    {
        return $"WallMap Width: {width}, Height: {height}, Flat Array: ({map[0, 0]})";
    }
}