using UnityEngine;

public class DirectionArrow : Highlightable
{

    public void Show()
    {
        MeshRenderer.enabled = true;
    }

    public void Hide()
    {
        MeshRenderer.enabled = false;
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
}