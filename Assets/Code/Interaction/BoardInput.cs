using UnityEngine;
using UnityEngine.InputSystem;

// Possibly call the building functionality BuildTool later
public class BoardInput
{


    private Flagstone hovered;
    private Highlightable highlighted;


    private Color placeableColor = Color.green;
    private Color blockedColor = Color.red;
    private Color existingWallColor = Color.yellow;


    private const float maxRayDistance = 500f;

    private TreasureMap treasureMap;
    private Camera cam;
    private TowerController towerController;

    public enum BuildType
    {
        Wall,
        Tower,
    }

    public BuildType BuildMode { get; private set; } = BuildType.Wall;


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

        if (BuildMode == BuildType.Tower)
        {
            if (hovered != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                    towerController.PlaceTower(hovered.Coord);
                if (Mouse.current.rightButton.wasPressedThisFrame)
                    towerController.RemoveTower(hovered.Coord);
            }
            return;
        }

        HighlightAtHovered();
        if (Mouse.current.leftButton.wasPressedThisFrame) PlaceWallAtHovered();
        if (Mouse.current.rightButton.wasPressedThisFrame) DestroyWallAtHovered();
    }

    public void SetBuildMode(BuildType buildMode)
    {
        this.BuildMode = buildMode;
        if (buildMode == BuildType.Tower)
        {
            if (highlighted != null) highlighted.Unhighlight();
            highlighted = null;
        }
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
            else if (snapshot.HasWall)
                highlightColor = existingWallColor;
            else
                highlightColor = placeableColor;
        }
        if (highlighted != null) highlighted.Unhighlight();
        highlighted = hovered;
        if (highlighted != null) highlighted.Highlight(highlightColor);
    }

    private Flagstone RaycastForFlagstone()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance)) return null;
        return hit.collider.GetComponentInParent<Flagstone>();
    }

    private void PlaceWallAtHovered()
    {
        if (hovered == null) return;
        HexCoord c = hovered.Coord;
        if (!treasureMap.CanPlaceWall(c)) return; // TODO: red ghost
        treasureMap.SetWall(c, true);
    }

    private void DestroyWallAtHovered()
    {
        if (hovered == null) return;
        HexCoord c = hovered.Coord;
        if (!treasureMap.HasWall(c)) return;
        if (towerController.HasTower(c)) return; // TODO this will be parameter-controlled later
        treasureMap.SetWall(c, false);
    }
}
