using UnityEngine;

// It tells you how far you are
public class DirectionMarker : MonoBehaviour, IHighlightable
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private TMPro.TextMeshPro numberLabel;

    [SerializeField] private DirectionArrow arrow;

    [SerializeField] private Color pathHighlightColor = Color.cyan;
    [SerializeField] private Color frontierHighlightColor = Color.red;



    public Coord Coord { get; private set; }

    private MaterialPropertyBlock block;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    public void Awake()
    {
        block = new MaterialPropertyBlock();
        arrow.TurnTo(Compass.North);
    }

    public void Initialize(Coord coord)
    {
        this.Coord = coord;
        UpdateDistance(-1);
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

    public void TurnAndHighlightArrowFrontier(Compass dir, bool shouldHighlightFrontier)
    {
        arrow.TurnTo(dir);
        if (shouldHighlightFrontier) arrow.Highlight(frontierHighlightColor);
        else arrow.Unhighlight();
    }

    public void HighlightArrowOnPath()
    {
        arrow.Highlight(pathHighlightColor);
    }


    public void UpdateArrow(Compass dirToGoal, bool isCritical)
    {
        arrow.TurnTo(dirToGoal);
        if (isCritical)
        {
            arrow.Highlight(pathHighlightColor);
        }
        else
        {
            arrow.Unhighlight();
        }
    }

    public void Highlight(Color color)
    {
        block.SetColor(BaseColorId, color);
        meshRenderer.SetPropertyBlock(block);
    }

    public void Unhighlight()
    {
        meshRenderer.SetPropertyBlock(null);
    }


}
