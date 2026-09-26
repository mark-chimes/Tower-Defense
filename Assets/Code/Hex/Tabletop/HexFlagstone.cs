using UnityEngine;

public class HexFlagstone : Highlightable
{
    public HexCoord Coord { get; private set; }

    public void Initialize(HexCoord coord)
    {
        Coord = coord;
    }
}
