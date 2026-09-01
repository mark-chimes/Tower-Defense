using UnityEngine;

public class Wall : MonoBehaviour, IHighlightable
{

    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Color highlightColor = Color.red; 

    private MaterialPropertyBlock block;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    public void Awake()
    {
        block = new MaterialPropertyBlock();
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
