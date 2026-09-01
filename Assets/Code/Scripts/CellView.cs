using UnityEngine;


public class CellView : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private TMPro.TextMeshPro numberLabel;
    [SerializeField] private Color highlightColor = Color.yellow; 

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

    public void Highlight()
    {
        block.SetColor(BaseColorId, highlightColor);
        meshRenderer.SetPropertyBlock(block);
    }

    public void Unhighlight()
    {
        meshRenderer.SetPropertyBlock(null);
    }


}
