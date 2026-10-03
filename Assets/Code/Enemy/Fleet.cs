using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class Fleet
{

    private float turnRate = 90f; // degrees per second
    private float startSpeed = 10f; // meters per second
    private float collisionRadius = 2.5f; // meters
    private float maxSpawnOffset = 2f; // meters

    private Random random;
    uint randomSeed = 1;

    public bool DespawnsAtGoal { get; set; } = false;

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
        Dead, Idle, Moving // keep Dead as first member: assumed as default value
    }

    private BoatData[] boats;

    private TreasureMap treasureMap;

    public int BoatCount { get; private set; }
    public int SlotsUsed { get; private set; }



    public Fleet(TreasureMap treasureMap, int capacity)
    {
        this.treasureMap = treasureMap;
        boats = new BoatData[capacity];
        BoatCount = 0;
        SlotsUsed = 0;
        random = new Random(randomSeed);
    }

    public bool CanSpawn()
    {
        return BoatCount < boats.Length;
    }

    public bool CanDespawn(int slot)
    {
        return slot >= 0 && slot < SlotsUsed && IsAlive(slot);
    }

    public int Spawn(HexCoord spawnCoord, bool shouldStart)
    {
        if (!CanSpawn()) return -1;
        int slot = FirstDeadSlot();
        BoatCount++;
        if (slot >= SlotsUsed) SlotsUsed++;
        BoatData newBoat = NewBoat(RandomPositionAtCoord(spawnCoord));
        FaceNextPos(ref newBoat);
        if (shouldStart) newBoat.State = BoatState.Moving;
        boats[slot] = newBoat;
        return slot;
    }

    private float2 RandomPositionAtCoord(HexCoord coord)
    {
        float2 pos = HexProjection.CoordsToFloat2(coord);
        // Distribution: this places more boats near the centre than near the edge
        pos += random.NextFloat2Direction() * random.NextFloat(0f, maxSpawnOffset);
        return pos;
    }

    public void DespawnIfPossible(int slot)
    {
        if (!CanDespawn(slot)) return;
        boats[slot].State = BoatState.Dead;
        BoatCount--;
    }

    public void DespawnAll()
    {
        for (int i = 0; i < SlotsUsed; i++)
        {
            DespawnIfPossible(i);
        }
    }

    private BoatData NewBoat(float2 startPosition)
    {
        BoatData boat = new BoatData();
        boat.Position = startPosition;
        boat.Velocity = float2.zero;
        boat.Heading = 0;
        boat.Speed = startSpeed;
        boat.Radius = collisionRadius;
        boat.State = BoatState.Idle;
        return boat;
    }

    public int Capacity() => boats.Length;

    public float2 PositionOf(int slot) => boats[slot].Position;

    public float HeadingOf(int slot) => boats[slot].Heading;

    public bool IsAlive(int slot) => boats[slot].State != BoatState.Dead;

    public void StartAll()
    {
        for (int i = 0; i < SlotsUsed; i++)
        {
            if (boats[i].State != BoatState.Idle) continue;
            boats[i].State = BoatState.Moving;
            FaceNextPos(ref boats[i]);
        }
    }

    public void StopAll()
    {
        for (int i = 0; i < SlotsUsed; i++)
        {
            if (!IsAlive(i)) continue;
            boats[i].State = BoatState.Idle;
        }
    }

    public void StepDt(float deltaTime)
    {
        for (int i = 0; i < SlotsUsed; i++)
        {
            if (!IsAlive(i)) continue;
            if (DespawnsAtGoal && IsAtGoal(boats[i].Position))
            {
                DespawnIfPossible(i);
                continue;
            }

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

            float rads = math.radians(boat.Heading);
            math.sincos(rads, out float s, out float c);
            boat.Velocity = new float2(s, c) * boat.Speed;
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

    private bool IsAtGoal(float2 pos)
    {
        return HexProjection.WorldToCoords(pos.x, pos.y) == treasureMap.GoalCoord;
    }

    private void FaceNextPos(ref BoatData boat)
    {
        HexCoord? targetCoord = TargetCoordAt(boat.Position);
        if (targetCoord == null) return;
        float2 nextPos = HexProjection.CoordsToFloat2(targetCoord.Value);
        boat.Heading = HexProjection.Float2ToDegreesHeading(nextPos - boat.Position);
    }

    private int FirstDeadSlot()
    {
        for (int i = 0; i < boats.Length; i++)
        {
            if (boats[i].State == BoatState.Dead) return i;
        }
        return -1;
    }
}
