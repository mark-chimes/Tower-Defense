using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class GridMouseHighlightIO
{


    private DirectionMarker hoveredErf;
    private Highlightable highlighted;
    private Camera cam;


    [SerializeField] private Color placeableColor = Color.green;
    [SerializeField] private Color blockedColor = Color.red;
    [SerializeField] private Color existingWallColor = Color.yellow;


    private const float maxRayDistance = 500f;

    TreasureMap treasureMap;

    private GridWalls wallHandler;

    public GridMouseHighlightIO(GridWalls wallHandler, TreasureMap treasureMap, Camera cam)
    {
        this.wallHandler = wallHandler;
        this.treasureMap = treasureMap;
        this.cam = cam;
    }

    public void HandleMouse()
    {
        if (Mouse.current == null) return;

        HighlightAtHoveredErf();
        if (Mouse.current.leftButton.wasPressedThisFrame) PlaceWallAtHovered();
        if (Mouse.current.rightButton.wasPressedThisFrame) DestroyWallAtHovered();
    }

    private void HighlightAtHoveredErf()
    {
        hoveredErf = RaycastForErf();
        Highlightable target = null;
        Color highlightColor = Color.magenta; // something went wrong if this is the highlight color
        if (hoveredErf != null)
        {
            Coord c = hoveredErf.Coord;
            ErfSnapshot erf = treasureMap.At(c);
            if (erf.Kind != SpawnGoalKind.Floor)
                highlightColor = blockedColor;
            else if (erf.HasWall)
                highlightColor = existingWallColor;
            else
                highlightColor = placeableColor;

            Highlightable maybeWall = wallHandler.MaybeWall(c);
            target = (maybeWall != null ? maybeWall : hoveredErf.HighlightableTile());
        }
        highlighted?.Unhighlight();
        target?.Highlight(highlightColor);
        highlighted = target;
    }


    /// Currently assumes Walls have colliders off. Revisit if colliders turned on.
    private DirectionMarker RaycastForErf()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance)) return null;
        return hit.collider.GetComponentInParent<DirectionMarker>();
    }

    private void PlaceWallAtHovered()
    {
        if (hoveredErf == null) return;
        if (!treasureMap.CanPlaceWall(hoveredErf.Coord)) return; // TODO: red ghost
        wallHandler.SpawnWall(hoveredErf.Coord);

    }

    private void DestroyWallAtHovered()
    {
        if (hoveredErf == null) return;
        wallHandler.DespawnWall(hoveredErf.Coord);
    }

    public void ClearHighlightIfMatching(Highlightable toMatch)
    {
        if (ReferenceEquals(highlighted, toMatch)) highlighted = null;
    }

    // TODO update camera method if camera can ever change


}
