using UnityEngine;

// TODO should this enemy control its own movement or be controlled by central authority?
public class Boat : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    bool hasPath = false;
    readonly float speed = 10f; // TODO improve how this is handled

    Coord nextCoord;

    // TODO update this with its own update? Or done in a loop from enemy controller?

    void Update()
    {
        if (!hasPath) return;
        // if (nextCoord == currentCoord) return; // TODO coordinate updating

        transform.rotation = movementDirection;
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, waypoint, speed * Time.deltaTime);

        if (layout.AreVector3Close(transform.localPosition, waypoint))
        {
            currentCoord = nextCoord;
            if (currentCoord == goalCoord) { hasPath = false; return; }
            AimTowardsWaypoint();
        }

    }


    GridLayout layout;

    Coord currentCoord;
    private Coord goalCoord;

    Vector3 waypoint;

    private TreasureMap treasureMap;

    Quaternion movementDirection = Quaternion.identity;


    public void Initialize(Coord spawnCoord, Coord goalCoord, GridLayout layout)
    {
        this.currentCoord = spawnCoord;
        this.goalCoord = goalCoord;
        this.layout = layout;

        this.nextCoord = currentCoord;
        this.waypoint = transform.position;
    }

    public void RecalculatePathing(TreasureMap treasureMap)
    {
        Debug.Log($"Recalculate enemy pathing for {name}.");
        this.treasureMap = treasureMap;
        hasPath = true;
        AimTowardsWaypoint();
    }

    // TODO the code below and this structure can probably be simplified
    private void AimTowardsWaypoint()
    {
        Debug.Assert(hasPath);
        Debug.Log($"Recalculate pathing target for {name}.");

        Signpost signpostHere = treasureMap.SignpostAt(currentCoord);
        movementDirection = layout.CompassToQuaternion(signpostHere.DirToGoal);
        nextCoord = currentCoord.InDirection(signpostHere.DirToGoal);
        waypoint = layout.CoordsToWorld(nextCoord);

        Debug.Log($"Moving from currentCoord {currentCoord} to {nextCoord}.");
        Debug.Log($"Moving to waypoint {waypoint}.");
    }

    public void ClearPathing()
    {
        Debug.Log($"ClearPathing enemy pathing for {name}.");
        hasPath = false;
        // nextCoord = currentCoord; // TODO is this neccessary? 
    }
}