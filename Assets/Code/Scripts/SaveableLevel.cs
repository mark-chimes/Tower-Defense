using UnityEngine;

[System.Serializable]
public class SaveableLevel
{
    public int Width;
    public int Height;
    public bool[] IsWall; // 1D flattening of 2D array

    public SaveableLevel(int width, int height, bool[,] wallMap)
    {
        Width = width;
        Height = height;
        IsWall = FlattenWallMap(wallMap);
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
    
    public static SaveableLevel FromJson(string json)
    {
        return JsonUtility.FromJson<SaveableLevel>(json);
    }

    private bool[] FlattenWallMap(bool[,] wallMap)
    {
        bool[] flatMap = new bool[Width * Height];
        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                flatMap[z * Width + x] = wallMap[x, z];
            }
        }
        return flatMap;
    }

    public bool[,] WallMap()
    {
        bool[,] wallMap = new bool[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                wallMap[x, z] = IsWall[z * Width + x];
            }
        }
        return wallMap;
    }
}
