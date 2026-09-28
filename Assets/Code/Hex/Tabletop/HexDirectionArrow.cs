using UnityEngine;

public class HexDirectionArrow : Highlightable
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
        transform.localRotation = HexLayout.CompassToQuaternion(dir);
        Show();
    }
}