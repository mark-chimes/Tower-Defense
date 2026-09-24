using UnityEngine;

// It tells you how far you are
public class DirectionMarker : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshPro numberLabel;

    [SerializeField] private DirectionArrow arrow;

    [SerializeField] private Color pathHighlightColor = Color.cyan;
    [SerializeField] private Color frontierHighlightColor = Color.red;

    private Highlightable highlightableTile;

    public Coord Coord { get; private set; }

    public void Awake()
    {
        highlightableTile = GetComponentInChildren<Flagstone>();

    }

    public void Initialize(Coord coord)
    {
        Coord = coord;
        UpdateDistance(-1);
        PointTo(Compass.None);
    }

    public void UpdateDistance(int distanceNum)
    {
        if (distanceNum < 0)
        {
            numberLabel.text = "X";
            return;
        }
        numberLabel.text = distanceNum.ToString();
    }

    private void PointTo(Compass dir)
    {
        arrow.TurnTo(dir);
    }

    public Highlightable HighlightableTile()
    {
        return highlightableTile;
    }

    public enum ArrowAccent
    {
        Normal,
        Frontier,
        Path
    }

    public void ExhibitSignpost(Signpost sign)
    {
        UpdateDistance(sign.DistanceToGoal);
        PointTo(sign.DirToGoal);
    }

    public void ExhibitAccent(ArrowAccent accent)
    {
        switch (accent)
        {
            case ArrowAccent.Normal: arrow.Unhighlight(); break;
            case ArrowAccent.Frontier: arrow.Highlight(frontierHighlightColor); break;
            case ArrowAccent.Path: arrow.Highlight(pathHighlightColor); break;
        }
    }

    public void SetPathingVisible(bool isVisible)
    {
        arrow.gameObject.SetActive(isVisible);
    }

    public void SetDistanceVisible(bool isVisible)
    {
        numberLabel.gameObject.SetActive(isVisible);
    }


}
