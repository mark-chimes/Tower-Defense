using UnityEngine;

// TODO this is an empty class for now, but its a default implementation for IHighlightable
// I don't really know how to put this on objects properly so I've been copy-pasting

public class Highlightable : MonoBehaviour, IHighlightable
{

    [SerializeField] private MeshRenderer meshRenderer;

    private MaterialPropertyBlock block;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    public void Awake()
    {
        block = new MaterialPropertyBlock();
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
