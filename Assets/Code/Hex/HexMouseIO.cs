using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class HexMouseIO
{


    private HexFlagstone hovered;
    private Highlightable highlighted;


    private Color placeableColor = Color.green;
    private Color blockedColor = Color.red;
    private Color existingWallColor = Color.yellow;


    private const float maxRayDistance = 500f;

    private HexTreasureMap treasureMap;
    private HexGridView gridView;
    private Camera cam;


    // private GridWalls wallHandler;

    public HexMouseIO(HexGridView gridView, HexTreasureMap treasureMap, Camera cam)
    {
        this.gridView = gridView;
        this.treasureMap = treasureMap;
        this.cam = cam;
    }

    public void HandleMouse()
    {
        if (Mouse.current == null) return;

        HighlightAtHovered();
        if (Mouse.current.leftButton.wasPressedThisFrame) PlaceWallAtHovered();
        if (Mouse.current.rightButton.wasPressedThisFrame) DestroyWallAtHovered();
    }

    private void HighlightAtHovered()
    {
        hovered = RaycastForFlagstone();
        Color highlightColor = Color.magenta; // something went wrong if this is the highlight color
        if (hovered != null)
        {
            HexCoord c = hovered.Coord;
            HexSnapshot snapshot = treasureMap.At(c);
            if (snapshot.TileMarker != HexTreasureMap.TileMarker.None)
                highlightColor = blockedColor;
            else if (snapshot.HasWall)
                highlightColor = existingWallColor;
            else
                highlightColor = placeableColor;
        }
        if (highlighted != null) highlighted.Unhighlight();
        highlighted = hovered;
        if (highlighted != null) highlighted.Highlight(highlightColor);
    }


    /// Currently assumes Walls have colliders off. Revisit if colliders turned on.
    private HexFlagstone RaycastForFlagstone()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance)) return null;
        return hit.collider.GetComponentInParent<HexFlagstone>();
    }

    private void PlaceWallAtHovered()
    {
        if (hovered == null) return;
        HexCoord c = hovered.Coord;
        if (!treasureMap.CanPlaceWall(c)) return; // TODO: red ghost
        treasureMap.SetWall(c, true);
        gridView.SetTerrainAt(c, true);
        treasureMap.Recompute();
    }

    private void DestroyWallAtHovered()
    {
        if (hovered == null) return;
        HexCoord c = hovered.Coord;
        if (!treasureMap.HasWall(c)) return;
        treasureMap.SetWall(c, false);
        gridView.SetTerrainAt(c, false);
        treasureMap.Recompute();
    }
}
