using UnityEngine;

[System.Serializable]

// TODO should this live on the WallMap class?
public class SaveableLevel
{
    public int Width;
    public int Height;
    public bool[] IsWall; // 1D flattening of 2D array

    public SaveableLevel(int width, int height, WallMap wallMap)
    {
        Width = width;
        Height = height;
        IsWall = wallMap.FlattenedWallMap();
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static SaveableLevel FromJson(string json)
    {
        return JsonUtility.FromJson<SaveableLevel>(json);
    }

    public WallMap LoadMap()
    {
        return WallMap.FromFlatMap(IsWall, Width, Height);
    }

    public override string ToString()
    {
        return $"SaveableLevel Width: {Width}, Height: {Height}, Flat Array: {IsWall})";
    }

}
