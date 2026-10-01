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

    // TODO rename/restructure with wall/land pass
    private void HighlightAtHovered()
    {
        Color highlightColor = Color.magenta; // something went wrong if this is the highlight color
        if (hovered != null)
        {
            HexCoord c = hovered.Coord;
            HexSnapshot snapshot = treasureMap.At(c);
            if (snapshot.Landmark != TreasureMap.Landmark.None)
                highlightColor = blockedColor;
            else if (snapshot.IsLand && towerController.HasTower(c) && !DestroysTowerWithLand)
                highlightColor = lockedLandColor;
            else if (snapshot.IsLand)
                highlightColor = existingLandColor;
            else
                highlightColor = placeableColor;
        }
        if (highlighted != null) highlighted.Unhighlight();
        highlighted = hovered;
        if (highlighted != null) highlighted.Highlight(highlightColor);
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
        if (!treasureMap.IsLand(c)) return;
        if (towerController.HasTower(c) && !DestroysTowerWithLand) return;
        if (towerController.HasTower(c)) towerController.RemoveTower(c);
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
