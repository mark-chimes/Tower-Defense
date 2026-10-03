using Unity.Mathematics;
using UnityEngine;

public class Fleet
{

    private float turnRate = 90f; // degrees per second
    private float startSpeed = 10f; // meters per second
    private float collisionRadius = 2.5f; // meters

    private struct BoatData
    {
        public float2 Position; // meters
        public float2 Velocity; // meters per second

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
        float2 startPosition = HexProjection.CoordsToFloat2(spawnCoord);
        boats[slot] = NewBoat(startPosition);
        FaceNextPos(ref boats[slot]);
        return slot;
    }

    private BoatData NewBoat(float2 startPosition)
    {
        BoatData boat = new BoatData();
        boat.Position = startPosition;
        boat.Velocity = new float2(0, 0);
        boat.Heading = 0;
        boat.Speed = startSpeed;
        boat.Radius = collisionRadius;
        boat.State = BoatState.Idle;
        return boat;
    }

    public int Capacity() => boats.Length;

    private float2 PositionOf(int slot) => boats[slot].Position;

    public Vector3 Position3DOf(int slot) => HexProjection.Float2ToWorld(boats[slot].Position);

    public float HeadingOf(int slot) => boats[slot].Heading;

    public bool IsAlive(int slot) => boats[slot].State != BoatState.Dead;

    public void StartAll()
    {
        for (int i = 0; i < boats.Length; i++)
        {
            if (boats[i].State != BoatState.Idle) continue;
            boats[i].State = BoatState.Moving;
            FaceNextPos(ref boats[i]);
        }
    }

    public void StopAll()
    {
        for (int i = 0; i < boats.Length; i++)
        {
            if (!IsAlive(i)) continue;
            boats[i].State = BoatState.Idle;
        }
    }

    public void StepDt(float deltaTime)
    {
        for (int i = 0; i < boats.Length; i++)
        {
            if (boats[i].State != BoatState.Moving) continue;

            ref BoatData boat = ref boats[i];
            float2 pos = boat.Position;

            HexCoord? targetCoord = TargetCoordAt(pos);
            if (targetCoord == null) continue;

            float2 target = HexProjection.CoordsToFloat2(targetCoord.Value);
            float2 toTarget = target - boat.Position;
            float targetHeading = HexProjection.Float2ToDegreesHeading(toTarget);
            boat.Heading = Mathf.MoveTowardsAngle(boat.Heading, targetHeading, turnRate * deltaTime);
            boat.Heading = Mathf.Repeat(boat.Heading, 360f);

            float rads = boat.Heading * Mathf.Deg2Rad;
            boat.Velocity = new float2(Mathf.Sin(rads), Mathf.Cos(rads)) * boat.Speed;
            boat.Position += boat.Velocity * deltaTime;
        }
    }

    private HexCoord? TargetCoordAt(float2 pos)
    {
        HexCoord coord = HexProjection.WorldToCoords(pos.x, pos.y);
        if (!treasureMap.Contains(coord) || treasureMap.IsLand(coord)) return null;
        HexCompass dir = treasureMap.DirectionAt(coord);
        if (dir == HexCompass.NONE) return null;
        HexCoord targetCoord = coord.InDirection(dir);
        return targetCoord;
    }

    private void FaceNextPos(ref BoatData boat)
    {
        HexCoord? targetCoord = TargetCoordAt(boat.Position);
        if (targetCoord == null) return;
        float2 nextPos = HexProjection.CoordsToFloat2(targetCoord.Value);
        boat.Heading = HexProjection.Float2ToDegreesHeading(nextPos - boat.Position);
    }
}
