using UnityEngine;


public class CellView : MonoBehaviour, IHighlightable
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private TMPro.TextMeshPro numberLabel;
    public CellCoord Coord {get; private set;}

    private MaterialPropertyBlock block;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    
    public void Awake()
    {
        block = new MaterialPropertyBlock();
    }

    public void Initialize(CellCoord coord)
    {
        this.Coord = coord;
        numberLabel.text = coord.ToStringNumbersOnly();
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
