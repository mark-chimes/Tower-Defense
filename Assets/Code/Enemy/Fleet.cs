using UnityEngine;

public class Fleet
{

    private float turnRate = 180f; // degrees per second
    private float startSpeed = 10f; // meters per second
    private float collisionRadius = 2.5f; // meters

    private struct BoatData
    {
        public Vector2 Position; // meters
        public Vector2 Velocity; // meters per second

        public float Heading; // degrees
        public float Speed; // meters per second
        public float Radius; // meters

        public BoatState State;
    }

    private enum BoatState
    {
        Dead, Idle, Moving
    }

    private BoatData[] boats;

    private TreasureMap treasureMap;

    public int BoatCount { get; private set; }

    public Fleet(TreasureMap treasureMap, int capacity)
    {
        this.treasureMap = treasureMap;
        boats = new BoatData[capacity];
        BoatCount = 0;
    }

    public bool CanSpawn()
    {
        return BoatCount < boats.Length;
    }

    public int Spawn(HexCoord spawnCoord)
    {
        Debug.Assert(CanSpawn());
        if (!CanSpawn()) return -1;
        int slot = BoatCount;
        BoatCount++;
        Vector2 startPosition = HexProjection.CoordsToVector2(spawnCoord);
        boats[slot] = NewBoat(startPosition);
        return slot;
    }

    private BoatData NewBoat(Vector2 startPosition)
    {
        BoatData boat = new BoatData();
        boat.Position = startPosition;
        boat.Velocity = new Vector2(0, 0);
        boat.Heading = 0;
        boat.Speed = startSpeed;
        boat.Radius = collisionRadius;
        boat.State = BoatState.Idle;
        return boat;
    }

    public Vector2 PositionOf(int slot) => boats[slot].Position;
}
