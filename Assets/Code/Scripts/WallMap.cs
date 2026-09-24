
public class WallMap
{

    private bool[,] map;
    public readonly int Width;
    public readonly int Height;

    public WallMap(int width, int height)
    {
        map = new bool[width, height];
        Width = width;
        Height = height;
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
        bool[] flatMap = new bool[Width * Height];
        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                flatMap[z * Width + x] = map[x, z];
            }
        }
        return flatMap;
    }

    public static WallMap FromFlatMap(bool[] flatMap, int width, int height)
    {
        bool[,] wallMap = new bool[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                wallMap[x, z] = flatMap[z * width + x];
            }
        }
        return new WallMap(wallMap, width, height);
    }

    private WallMap(bool[,] map, int width, int height)
    {
        this.map = map;
        Width = width;
        Height = height;
    }

    public override string ToString()
    {
        return $"WallMap Width: {Width}, Height: {Height}";
    }
}