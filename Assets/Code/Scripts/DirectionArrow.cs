using UnityEngine;

public class DirectionArrow : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
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

    public void TurnTo(Cardinal dir) { 
        switch (dir) 
        {
            case Cardinal.North: TurnTo(0,1); break;
            case Cardinal.East: TurnTo(1,0);  break;
            case Cardinal.South: TurnTo(0,-1); break;
            case Cardinal.West: TurnTo(-1,0);  break;
            case Cardinal.None: Hide(); break;  
        }
    }
}