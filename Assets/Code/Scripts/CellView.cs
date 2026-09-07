using UnityEngine;


public class CellView : MonoBehaviour, IHighlightable
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private TMPro.TextMeshPro numberLabel;
    public CellCoord Coord {get; private set;}
    
    public int? DistanceNum{get; private set;}
     // For visualizing distance to target TODO rename

    private MaterialPropertyBlock block;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    
    public void Awake()
    {
        block = new MaterialPropertyBlock();
    }

    public void Initialize(CellCoord coord)
    {
        this.Coord = coord;
        UpdateDistance(null);
    }

    public void UpdateDistance(int? distanceNum)
    {
        this.DistanceNum = distanceNum;
        if (distanceNum == null)
        {
           numberLabel.text = "X"; 
           return;
        }
        numberLabel.text = DistanceNum.ToString();
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
