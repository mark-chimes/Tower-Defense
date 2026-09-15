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

    }

    private Coord spawnPos;
    private Coord goalPos; 

    public void Initialize(Coord spawnPos, Coord goalPos)
    {
        this.spawnPos = spawnPos;
        this.goalPos = goalPos;
    }
}