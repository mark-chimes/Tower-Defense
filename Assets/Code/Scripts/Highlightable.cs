using UnityEngine;

public class Highlightable : MonoBehaviour, IHighlightable
{
    private MeshRenderer meshRenderer;
    protected MeshRenderer MeshRenderer =>
        meshRenderer ??= GetComponentInChildren<MeshRenderer>();
        
    private MaterialPropertyBlock block;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    public void Awake()
    {
        block = new MaterialPropertyBlock();
    }

    public void Highlight(Color color)
    {
        block.SetColor(BaseColorId, color);
        MeshRenderer.SetPropertyBlock(block);
    }

    public void Unhighlight()
    {
        MeshRenderer.SetPropertyBlock(null);
    }


}
