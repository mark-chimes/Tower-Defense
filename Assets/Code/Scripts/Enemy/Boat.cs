using UnityEngine;

// TODO should this enemy control its own movement or be controlled by central authority?
public class Boat : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // TODO update this with its own update? Or done in a loop from enemy controller?
    void Update()
    {
        // var myTilePos = layout.CoordsToWorld(currentCoord);
        // var myPos = transform.localPosition;
        // transform.localPosition = myTilePos;
        transform.rotation = movementDirection;
    }



    private Coord goalPos; 
    GridLayout layout;

    Coord currentCoord;

    private TreasureMap treasureMap;

    Quaternion movementDirection = new Quaternion(0f, 0f, 0f, 0f);

    public void Initialize(Coord spawnPos, Coord goalPos, GridLayout layout)
    {
        this.currentCoord = spawnPos;
        this.goalPos = goalPos;
        this.layout = layout;
    }

    public void RecalculatePathing(TreasureMap treasureMap)
    {
        Debug.Log($"Recalculate enemy pathing for {name}.");
        this.treasureMap = treasureMap;
        Coord myTileCoord = currentCoord;

        Signpost signpostHere = treasureMap.SignpostAt(myTileCoord);
        movementDirection = layout.CompassToQuaternion(signpostHere.DirToGoal);
    }
}