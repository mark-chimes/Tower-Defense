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

    public void TurnTo(Vector3 v)
    {
        transform.localRotation = Quaternion.LookRotation(new Vector3(v.x, 0f, v.z), Vector3.up);
        Show();
    }

    public void TurnTo(HexCompass dir)
    {
        if (dir == HexCompass.NONE)
        {
            Hide();
            return;
        }
        TurnTo(HexLayout.CoordsToWorld(dir.Offset()));
    }
}