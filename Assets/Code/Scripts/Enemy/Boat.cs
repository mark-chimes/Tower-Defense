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
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPos, speed * Time.deltaTime);

        if (layout.AreVector3Close(transform.localPosition, goalPos)) { hasPath = false; return; }

        if (layout.AreVector3Close(transform.localPosition, targetPos)) 
        {
            BeginMoveToPathingTarget();
        }

    }


    GridLayout layout;

    Coord currentCoord;
    Vector3 targetPos;
    private Vector3 goalPos;

    private TreasureMap treasureMap;

    Quaternion movementDirection = Quaternion.identity;


    public void Initialize(Coord spawnCoord, Coord goalCoord, GridLayout layout)
    {
        this.currentCoord = spawnCoord;
        this.goalPos = layout.CoordsToWorld(goalCoord); // TODO this will need to be updated in RecalculatePathing if it can change
        this.layout = layout;

        this.nextCoord = currentCoord;
        this.targetPos = transform.position;
    }

    public void RecalculatePathing(TreasureMap treasureMap)
    {
        Debug.Log($"Recalculate enemy pathing for {name}.");
        this.treasureMap = treasureMap;
        hasPath = true;
        // nextCoord = currentCoord;

        BeginMoveToPathingTarget();
    }

    // TODO the code below and this structure can probably be simplified
    private void BeginMoveToPathingTarget()
    {
        Debug.Assert(hasPath);
        Debug.Log($"Recalculate pathing target for {name}.");

        currentCoord = nextCoord;
        Signpost signpostHere = treasureMap.SignpostAt(currentCoord);
        movementDirection = layout.CompassToQuaternion(signpostHere.DirToGoal);
        nextCoord = currentCoord.InDirection(signpostHere.DirToGoal);
        Debug.Log($"Moving from currentCoord {currentCoord} to {nextCoord}.");

        targetPos = layout.CoordsToWorld(nextCoord);
        Debug.Log($"Moving to targetPos {targetPos}.");

    }

    public void ClearPathing()
    {
        hasPath = false;
        nextCoord = currentCoord; // TODO is this neccessary? 
    }
}