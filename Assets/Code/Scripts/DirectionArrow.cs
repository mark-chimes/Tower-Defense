using UnityEngine;

public class DirectionArrow : MonoBehaviour
{

    public enum Direction
    {
        North,
        South,
        East,
        West,
    }

    [SerializeField] private MeshRenderer meshRenderer;

    public void Show()
    {
        meshRenderer.enabled = true;
    }

    public void Hide()
    {
        meshRenderer.enabled = false;
    }

    void Reset() 
    { 
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void TurnTo(float x, float z) { 
        transform.localRotation = Quaternion.LookRotation(new Vector3(x, 0f, z), Vector3.up);
        Show();
    }

    public void TurnTo(Direction dir) { 
        switch (dir) 
        {
            case Direction.North: TurnTo(0,1); break;
            case Direction.East: TurnTo(1,0);  break;
            case Direction.South: TurnTo(0,-1); break;
            case Direction.West: TurnTo(-1,0);  break;
        }
    }
}