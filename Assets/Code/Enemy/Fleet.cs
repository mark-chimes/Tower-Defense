using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class Fleet
{

    /****************************************/
    // These variables should go on some sort of "boat personality" 
    // or "boat type" data, that can be referenced
    private float turnRate = 90f; // degrees per second

    // friction slows down boat proportional to current speed
    // so its units are "per-second"
    // friction 2 /s means "lose 2x your speed per second".
    private float forwardFriction = 2f;

    private float sidewaysFriction = 10f;

    // how much the boat aims beyond its target to correct for drift
    // 0 means no correction, boat tries to aim directly at target
    // at a high value, the boat will over-correct, snaking back and forth
    // can be higher than 1
    private float driftCorrectionGain = 0.5f;

    private float topSpeed = 10f; // meters per second

    // TODO use the collision radius
    // private float collisionRadius = 2.5f; // meters
    /****************************************/


    private float maxSpawnOffset = 2f; // meters

    private Random random;
    uint randomSeed = 1;

    public bool DespawnsAtGoal { get; set; } = false;

    private struct BoatData
    {
        public float2 Position; // meters
        public float2 Velocity; // meters per second

        public float Heading; // degrees

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

            ref BoatData boat = ref boats[i];

            BoatDoesStuff(ref boat, deltaTime);

            if (DespawnsAtGoal && IsAtGoal(boat.Position))
            {
                DespawnIfPossible(i);
                // continue;
            }
        }
    }

    private void BoatDoesStuff(ref BoatData boat, float deltaTime)
    {
        float2 aim;
        float2? targetPos = TargetPosAt(boat.Position);
        float throttle;
        float2 forward = Forward(boat.Heading);


        if (boat.State == BoatState.Moving && targetPos != null)
        {
            aim = Aim(targetPos.Value, boat.Position, boat.Velocity, driftCorrectionGain, topSpeed);
            throttle = Throttle(aim, forward);
        }
        else
        {
            aim = forward;
            throttle = 0;
        }

        float2 acceleration = Acceleration(forward, throttle, forwardFriction, topSpeed);
        boat.Heading = Turn(aim, boat.Heading, turnRate, deltaTime);
        boat.Velocity += acceleration * deltaTime;
        ApplyFriction(ref boat, deltaTime);
        boat.Position += boat.Velocity * deltaTime;
    }

    // Boat over-aims slightly using the drift correction gain to correct for sideways-drift
    private float2 Aim(float2 targetPos, float2 position, float2 velocity, float driftCorrectionGain, float topSpeed)
    {
        float2 desiredVelocity = math.normalizesafe(targetPos - position) * topSpeed;
        float2 aimOffset = driftCorrectionGain * (desiredVelocity - velocity);
        return math.normalizesafe(desiredVelocity + aimOffset);
    }

    // Calculates where the boat will turn based on where it's trying to aim
    private float Turn(float2 aim, float oldHeading, float turnRate, float deltaTime)
    {
        float aimHeading = HexProjection.Float2ToDegreesHeading(aim);
        float newHeading = Mathf.MoveTowardsAngle(oldHeading, aimHeading, turnRate * deltaTime);
        return Mathf.Repeat(newHeading, 360f);        // neaten the angle (not necessary)
    }

    // calculates an under-thrust (throttle) to make up for being off-target
    // This particular algorithm uses normalized aim and normalized forward vector
    // and to be used for reducing thrust based on the dot-product of those.
    // It's best suited for tighter turns
    // Higher throttle is faster acceleration 
    private float Throttle(float2 aim, float2 forward)
    {
        // how well the bow points where we want to go: 1 = exactly, 0 = perpendicular
        // dot(a, b) = |a| |b| cos(theta)
        float alignment = math.dot(forward, aim);

        // If we are off-target, we don't go as fast, so that we can steer back more easily,
        // since a slower boat turns tighter 
        // max value of 1
        return math.max(0f, alignment);
    }

    private float2 Acceleration(float2 forward, float throttle, float forwardFriction, float topSpeed)
    {
        // max acceleration; forward friction balances it exactly at topSpeed
        float maxAcceleration = topSpeed * forwardFriction;

        // throttle is scalar multiplier 0 <= throttle <= 1
        // maxAcceleration is scalar coefficient
        // forward is normalized direction vector |forward|=1
        return throttle * maxAcceleration * forward;
    }

    private void ApplyFriction(ref BoatData boat, float deltaTime)
    {
        float2 forward = Forward(boat.Heading);
        float2 right = new float2(forward.y, -forward.x);

        float forwardSpeed = math.dot(boat.Velocity, forward);
        float sidewaysSpeed = math.dot(boat.Velocity, right);

        forwardSpeed *= 1f - math.min(forwardFriction * deltaTime, 1f);
        sidewaysSpeed *= 1f - math.min(sidewaysFriction * deltaTime, 1f);

        boat.Velocity = forward * forwardSpeed + right * sidewaysSpeed;
    }

    // normalized vector, forward direction based on heading angle
    private static float2 Forward(float heading)
    {
        return HexProjection.DegreesHeadingToFloat2(heading);
    }

    private void FaceNextPos(ref BoatData boat)
    {
        float2? targetPos = TargetPosAt(boat.Position);
        if (targetPos == null) return;
        boat.Heading = HexProjection.Float2ToDegreesHeading(targetPos.Value - boat.Position);
    }

    // finds the next target position using the flow field (treasure map)
    private float2? TargetPosAt(float2 pos)
    {
        HexCoord coord = HexProjection.WorldToCoords(pos.x, pos.y);
        if (!treasureMap.Contains(coord) || treasureMap.IsLand(coord)) return null;
        HexCompass dir = treasureMap.DirectionAt(coord);
        if (dir == HexCompass.NONE) return null;
        HexCoord targetCoord = coord.InDirection(dir);
        return HexProjection.CoordsToFloat2(targetCoord);
    }


    private bool IsAtGoal(float2 pos)
    {
        return HexProjection.WorldToCoords(pos.x, pos.y) == treasureMap.GoalCoord;
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
