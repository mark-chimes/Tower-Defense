using UnityEngine;

// It tells you how far you are
public class Signpost : MonoBehaviour, IHighlightable
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private TMPro.TextMeshPro numberLabel;

    [SerializeField] private DirectionArrow arrow;

    [SerializeField] private Color arrowHighlightColor;


    public Coord Coord { get; private set; }

    private MaterialPropertyBlock block;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    public void Awake()
    {
        block = new MaterialPropertyBlock();
        arrow.TurnTo(Cardinal.North);
        arrow.Show();
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

    public void UpdateArrow(Cardinal cameFrom, bool isCritical)
    {
        arrow.TurnTo(cameFrom);
        if (isCritical)
        {
            arrow.Highlight(arrowHighlightColor);
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
