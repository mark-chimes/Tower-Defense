using UnityEngine;

// TODO should this enemy control its own movement or be controlled by central authority?
public class Boat : MonoBehaviour
{
    bool hasPath = false;
    readonly float speed = 10f; // TODO improve how this is handled

    HexCoord currentCoord;
    HexCoord nextCoord;


    private HexCoord goalCoord;

    Vector3 waypoint;

    private TreasureMap treasureMap;

    Quaternion movementDirection = Quaternion.identity;


    public void Initialize(HexCoord spawnCoord, HexCoord goalCoord)
    {
        currentCoord = spawnCoord;
        this.goalCoord = goalCoord;

        nextCoord = currentCoord;
        waypoint = transform.position;
    }


    // TODO update this with its own update? Or done in a loop from enemy controller?

    void Update()
    {
        if (!hasPath) return;
        // if (nextCoord == currentCoord) return; // TODO coordinate updating

        transform.rotation = movementDirection;
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, waypoint, speed * Time.deltaTime);

        if (HexProjection.AreVector3Close(transform.localPosition, waypoint))
        {
            currentCoord = nextCoord;
            if (currentCoord == goalCoord) { hasPath = false; return; }
            FindNextWaypoint();
        }

    }


    public void RecalculatePathing(TreasureMap treasureMap)
    {
        Debug.Log($"Recalculate enemy pathing for {name}.");
        this.treasureMap = treasureMap;
        hasPath = true;
        FindNextWaypoint();
    }

    private void FindNextWaypoint()
    {
        Debug.Assert(hasPath);
        Debug.Log($"Recalculate pathing target for {name}.");

        FlowSample signpostHere = treasureMap.SignpostAt(currentCoord);
        if (signpostHere.DirToGoal == HexCompass.NONE)
        {
            Debug.Log($"No direction, boat {name} freezing in place");
            ClearPathing();
            return;
        }
        movementDirection = HexProjection.CompassToQuaternion(signpostHere.DirToGoal);
        nextCoord = currentCoord.InDirection(signpostHere.DirToGoal);
        waypoint = HexProjection.CoordsToWorld(nextCoord);

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