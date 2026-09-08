using UnityEngine;

public class DirectionArrow : MonoBehaviour, IHighlightable
{
    [SerializeField] private MeshRenderer meshRenderer;

    private MaterialPropertyBlock block;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    public void Awake()
    {
        block = new MaterialPropertyBlock();
    }

    void Reset() 
    { 
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Show()
    {
        meshRenderer.enabled = true;
    }

    public void Hide()
    {
        meshRenderer.enabled = false;
    }

    public void TurnTo(float x, float z) { 
        transform.localRotation = Quaternion.LookRotation(new Vector3(x, 0f, z), Vector3.up);
        Show();
    }

    public void TurnTo(Compass dir) { 
        switch (dir) 
        {
            case Compass.North: TurnTo(0,1); break;
            case Compass.East: TurnTo(1,0);  break;
            case Compass.South: TurnTo(0,-1); break;
            case Compass.West: TurnTo(-1,0);  break;
            case Compass.None: Hide(); break;  
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