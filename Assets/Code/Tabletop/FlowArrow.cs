using UnityEngine;

public class FlowArrow : Highlightable
{

    public void Show()
    {
        MeshRenderer.enabled = true;
    }

    public void Hide()
    {
        MeshRenderer.enabled = false;
    }
    
    public void TurnTo(HexCompass dir)
    {
        if (dir == HexCompass.NONE)
        {
            Hide();
            return;
        }
        transform.localRotation = HexProjection.CompassToQuaternion(dir);
        Show();
    }
}