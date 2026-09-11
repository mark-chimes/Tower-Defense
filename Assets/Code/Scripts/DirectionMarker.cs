using UnityEngine;

// It tells you how far you are
public class DirectionMarker : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private TMPro.TextMeshPro numberLabel;

    [SerializeField] private DirectionArrow arrow;

    [SerializeField] private Color pathHighlightColor = Color.cyan;
    [SerializeField] private Color frontierHighlightColor = Color.red;

    private IHighlightable highlightableTile;

    public Coord Coord { get; private set; }

    public void Awake()
    {
        highlightableTile = GetComponentInChildren<Tile>();

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

    public void UnhighlightArrow()
    {
        arrow.Unhighlight();
    }

    public void HighlightFrontierArrow()
    {
        arrow.Highlight(frontierHighlightColor);
    }


    public void HighlightArrowOnPath()
    {
        arrow.Highlight(pathHighlightColor);
    }

    public void PointTo(Compass dir)
    {
        arrow.TurnTo(dir);
    }


    public void UpdateArrow(Compass dir, bool isCritical)
    {
        PointTo(dir);
        if (isCritical)
        {
            arrow.Highlight(pathHighlightColor);
        }
        else
        {
            arrow.Unhighlight();
        }
    }

    public IHighlightable HighlightableTile()
    {
        return highlightableTile;
    }


}
