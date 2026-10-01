using UnityEngine;
using UnityEngine.InputSystem;

// Possibly call the building functionality BuildTool later
public class BoardInput
{

    //** Split by Shared, Land, and Tower **//

    //** === Shared === **//

    private const float maxRayDistance = 500f;
    private TreasureMap treasureMap;
    private Camera cam;
    private TowerController towerController;
    private Flagstone hovered;

    public enum BuildType
    {
        Land,
        Tower,
    }
    public BuildType BuildMode { get; private set; } = BuildType.Land;

    public BoardInput(TreasureMap treasureMap, Camera cam, TowerController towerController)
    {
        this.treasureMap = treasureMap;
        this.cam = cam;
        this.towerController = towerController;
    }

    public void HandleMouse()
    {
        if (Mouse.current == null) return;

        hovered = RaycastForFlagstone();

        switch (BuildMode)
        {
            case BuildType.Tower: HandleMouseForTowerMode(); break;
            case BuildType.Land: HandleMouseForLandMode(); break;
        }
    }

    private Flagstone RaycastForFlagstone()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance)) return null;
        return hit.collider.GetComponentInParent<Flagstone>();
    }

    public void SetBuildMode(BuildType buildMode)
    {
        BuildMode = buildMode;

        if (buildMode == BuildType.Tower)
        {
            if (highlighted != null) highlighted.Unhighlight();
            highlighted = null;
        }
        else
        {
            towerController.HideGhosts();
        }
    }

    //** === Land === **//

    private Highlightable highlighted;
    private Color placeableColor = Color.darkGreen;
    private Color blockedColor = Color.grey;
    private Color existingLandColor = Color.darkSalmon;
    private Color lockedLandColor = Color.grey;

    // land-removal policy touches towers. TODO: moves to the board later
    public bool DestroysTowerWithLand { get; set; } = false;

    private void HandleMouseForLandMode()
    {
        HighlightAtHovered();
        if (Mouse.current.leftButton.wasPressedThisFrame) PlaceLandAtHovered();
        if (Mouse.current.rightButton.wasPressedThisFrame) RemoveLandAtHovered();
    }

    private void HighlightAtHovered()
    {
        if (highlighted != null) highlighted.Unhighlight();

        highlighted = hovered;
        if (highlighted == null) return;
        highlighted.Highlight(HighlightColorAt(hovered.Coord));
    }

    private Color HighlightColorAt(HexCoord coord)
    {
        if (treasureMap.IsLand(coord))
            return CanRemoveLand(coord) ? existingLandColor : lockedLandColor;

        return treasureMap.CanPlaceLand(coord) ? placeableColor : blockedColor;
    }

    private bool CanRemoveLand(HexCoord coord)
    {
        if (!treasureMap.IsLand(coord)) return false;
        return !towerController.HasTower(coord) || DestroysTowerWithLand;
    }

    private void PlaceLandAtHovered()
    {
        if (hovered == null) return;
        HexCoord c = hovered.Coord;
        if (!treasureMap.CanPlaceLand(c)) return; // TODO: red ghost
        treasureMap.SetLand(c, true);
    }

    private void RemoveLandAtHovered()
    {
        if (hovered == null) return;
        HexCoord c = hovered.Coord;

        if (!CanRemoveLand(c)) return;

        towerController.RemoveTowerIfPresent(c);
        treasureMap.SetLand(c, false);
    }

    //** === Tower === **//

    private HexCoord? justPlacedAt;

    private void HandleMouseForTowerMode()
    {
        if (hovered == null)
        {
            justPlacedAt = null;
            towerController.HideGhosts();
            return;
        }

        HexCoord c = hovered.Coord;

        if (justPlacedAt != c)
            justPlacedAt = null;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (towerController.CanPlaceTower(c))
            {
                towerController.PlaceTower(c);
                justPlacedAt = c;
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
            towerController.RemoveTowerIfPresent(c);

        if (towerController.CanPlaceTower(c))
            towerController.ShowPlaceableGhostAt(c);
        else if (towerController.HasTower(c) && justPlacedAt != c)
            towerController.ShowBlockedGhostAt(c);
        else
            towerController.HideGhosts();
    }
}
