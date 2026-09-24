using UnityEngine;

[System.Serializable]

public class SaveableLevel
{
    public int Width;
    public int Height;
    public bool[] IsWall; // 1D flattening of 2D array

    public SaveableLevel(int width, int height, RectMap<bool> wallMap)
    {
        Width = width;
        Height = height;
        IsWall = wallMap.MapAsFlatArray();
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static SaveableLevel FromJson(string json)
    {
        return JsonUtility.FromJson<SaveableLevel>(json);
    }

    public RectMap<bool> LoadMap()
    {
        return RectMap<bool>.MapFromArray(IsWall, Width, Height);
    }

    public override string ToString()
    {
        return $"SaveableLevel Width: {Width}, Height: {Height}";
    }

}
