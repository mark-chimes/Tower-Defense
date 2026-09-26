using UnityEngine;

public class HexDirectionMarker : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshPro numberLabel;

    [SerializeField] private HexDirectionArrow arrow;

    [SerializeField] private Color pathHighlightColor = Color.cyan;
    [SerializeField] private Color frontierHighlightColor = Color.red;

    bool isOnPathableTerrain;
    bool isPathingVisible;
    bool isDistanceVisible;


    public HexCoord Coord { get; private set; }

    public void Initialize(HexCoord coord)
    {
        Coord = coord;
        UpdateDistance(-1);
        PointTo(HexCompass.NONE);
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

    private void PointTo(HexCompass dir)
    {
        arrow.TurnTo(dir);
    }

    public Highlightable HighlightableArrow()
    {
        return arrow;
    }

    public enum ArrowAccent
    {
        Normal,
        Frontier,
        Path
    }

    public void ExhibitSignpost(HexSignpost sign)
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

    public void SetIsOnPathableTerrain(bool newValue) 
    {
        isOnPathableTerrain = newValue;
        SetVisibility();
    }


    public void SetPathingVisible(bool isEnabled)
    {
        isPathingVisible = isEnabled;
        SetVisibility();
    }

    public void SetDistanceVisible(bool isEnabled)
    {
        isDistanceVisible = isEnabled;
        SetVisibility();
    }

    private void SetVisibility()
    {
        if (!isOnPathableTerrain)
        {
            numberLabel.gameObject.SetActive(false);
            arrow.gameObject.SetActive(false);
            return;
        }
        numberLabel.gameObject.SetActive(isDistanceVisible);
        arrow.gameObject.SetActive(isPathingVisible);
    }
}
