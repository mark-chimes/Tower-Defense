public class Flagstone : Highlightable
{
    public HexCoord Coord { get; private set; }

    public void Initialize(HexCoord coord)
    {
        Coord = coord;
    }
}
