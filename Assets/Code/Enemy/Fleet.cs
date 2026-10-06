using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

// See Steering Behaviors For Autonomous Characters
// https://www.red3d.com/cwr/steer/gdc99/
// For some ideas, although I do not implement things exactly as in there?

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
    // See https://en.wikipedia.org/wiki/Proportional_control
    private float driftCorrectionGain = 0.5f;

    private float topSpeed = 10f; // meters per second, engine speed
    private float topSpeedReal = 20f; // meters per second, can't be pushed faster

    // TODO use the collision radius
    private float collisionRadius = 2.5f; // meters
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

    // A single step of the simulation
    public void StepDt(float deltaTime)
    {
        // TODO not quite sure of the order here
        // TODO we should probably find all updates based on other boats first,
        // and then update them all, in two separate loops.
        // If we do a ping-pong buffer I think this will work well.

        ShiftBoatsAwayFromEachOther(deltaTime);

        for (int i = 0; i < SlotsUsed; i++)
        {
            if (!IsAlive(i)) continue;

            ref BoatData boat = ref boats[i];

            // TODO multiple of these use the HexProjection.WorldToCoords(pos.x, pos.y)
            // can probably find it once and pass into them

            (float2 aim, float throttle) = Controls(boat);
            MoveBoat(ref boat, deltaTime, aim, throttle);
            AdjustAwayFromCoast(ref boat, deltaTime);

            if (DespawnsAtGoal && IsAtGoal(boat.Position))
            {
                DespawnIfPossible(i);
                // continue;
            }
        }
    }

    // TODO maybe split the calculation of this force from the actual application?
    private void AdjustAwayFromCoast(ref BoatData boat, float deltaTime)
    {
        // find hex boat is on
        float2 pos = boat.Position;
        HexCoord coord = HexProjection.WorldToCoords(pos.x, pos.y);

        // TODO deal with out-of-bounds by moving the boat in-bounds
        if (!treasureMap.Contains(coord)) return;

        HexCoord[] neighbors = coord.Neighbours();

        // Find nearest sea hex and push boat out
        if (treasureMap.IsLand(coord))
        {
            HexCoord? nearestSeaNeighbor = null;
            float nearestSeaSquareDistance = float.MaxValue;
            Debug.Log($"neighbors:{neighbors}, length: {neighbors.Length}");
            foreach (HexCoord neighbor in neighbors)
            {
                // can skip non-land hexes
                if (!treasureMap.Contains(coord) || treasureMap.IsLand(neighbor)) continue;
                float2 nPos = HexProjection.CoordsToFloat2(neighbor);
                float2 toNeighbor = nPos - pos;
                float dx = toNeighbor.x;
                float dy = toNeighbor.y;
                float dSquared = dx * dx + dy * dy;

                Debug.Log($"nPos:{nPos}, toNeighbor:{toNeighbor}, dx:{dx}, dy:{dy}, dSquared:{dSquared}, nearestSeaSquareDistance:{nearestSeaSquareDistance}, neighbor:{neighbor}, ");
                if (dSquared < nearestSeaSquareDistance)
                {
                    nearestSeaNeighbor = neighbor;
                    nearestSeaSquareDistance = dSquared;
                }
            }

            // If none of the valid neighbors is sea, nothing to be done for now
            if (nearestSeaNeighbor == null) return;

            // Find a nice point on the sea neighbor and put the boat there
            // my simple version for now, just put it somewhere we know is inside the new hex
            // pointing a little from the center of the hex towards its old position
            float2 nCenter = HexProjection.CoordsToFloat2(nearestSeaNeighbor.Value);
            float2 fromNeighbor = boat.Position - nCenter;
            float fromX = fromNeighbor.x;
            float fromY = fromNeighbor.y;
            float maxAllowed = 2.85f; // 5.7/2 ~ about half the hexagon side-length

            // if we scale both coordinates, can we be *sure* we land inside the hex?
            float maxOff = math.max(math.abs(fromX) / maxAllowed, math.abs(fromY) / maxAllowed);
            float2 newPos = nCenter + fromNeighbor / maxOff;
            boat.Position = newPos;
        }



        float pushFactor = 10; // Arbitrary value for now, TODO move out to top
        float shiftFactor = 2; // Arbitrary value for now, TODO move out to top

        foreach (HexCoord neighbor in neighbors)
        {
            // can skip non-land hexes
            if (!treasureMap.Contains(coord) || !treasureMap.IsLand(neighbor)) continue;

            // direction vector to center of land neighbor
            float2 nPos = HexProjection.CoordsToFloat2(neighbor);
            float2 toNeighbor = (nPos - pos);
            float dx = toNeighbor.x;
            float dy = toNeighbor.y;

            // find squared-distance to the neighbor
            float dSquared = dx * dx + dy * dy;
            // inverselerp, TODO pre-calcualate min values in consts
            // this uses 49, 36, and 49-36=13
            float push = pushFactor * math.min(1.0f, math.max(0, 49f - dSquared) / 13f);

            // this uses 49, 46, and 49-46=3
            float shift = shiftFactor * math.min(1.0f, math.max(0, 49f - dSquared) / 3f);

            // push away from neighbor - soft-bump
            // use position for hard-bump
            boat.Velocity += -push * deltaTime * toNeighbor;
            boat.Position += -shift * deltaTime * toNeighbor;

        }
    }

    private void ShiftBoatsAwayFromEachOther(float deltaTime)
    {
        float pushFactor = 10; // Arbitrary value for now, TODO move out to top
        float shiftFactor = 2; // Arbitrary value for now, TODO move out to top

        // Naive n^2 algorithm for now to get things working
        // Should register boats to some hex-array or something later
        for (int i = 0; i < SlotsUsed; i++)
        {
            if (!IsAlive(i)) continue;
            ref BoatData boat = ref boats[i];

            for (int j = 0; j < SlotsUsed; j++)
            {
                if (!IsAlive(j)) continue;
                // Shouldn't push away from itself
                if (i == j) continue;

                ref BoatData nbor = ref boats[j];

                float2 pos = boat.Position;
                float2 nPos = nbor.Position;
                float2 toNeighbor = (nPos - pos);
                float dx = toNeighbor.x;
                float dy = toNeighbor.y;

                float dSquared = dx * dx + dy * dy;

                float cr = collisionRadius;
                // inverselerp
                // just copying the land calculation for now
                // this uses collisionRadius=2.5f, 0f, and 2.5f-0f=2.5f
                float push = pushFactor * math.min(1.0f, math.max(0, cr - dSquared) / cr);

                // push away from neighbor - soft-bump
                // use position for hard-bump
                boat.Velocity += -push * deltaTime * toNeighbor;


                // this uses 1, 0, and 1-0=1
                float shift = shiftFactor * math.min(1.0f, math.max(0, 1f - dSquared) / 1f);
                boat.Position += -shift * deltaTime * toNeighbor;

            }
        }
    }

    // How the boat chooses to move. Aim is where it would point if it could choose, throttle is how fast
    private (float2 aim, float throttle) Controls(in BoatData boat)
    {
        float2 aim;
        float throttle;

        float2 forward = Forward(boat.Heading);
        float2? targetPos = TargetPosAt(boat.Position);

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

        return (aim, throttle);
    }

    // normalized vector, forward direction based on heading angle
    private static float2 Forward(float heading)
    {
        return HexProjection.DegreesHeadingToFloat2(heading);
    }

    // Boat over-aims slightly using the drift correction gain to correct for sideways-drift
    private float2 Aim(float2 targetPos, float2 position, float2 velocity, float driftCorrectionGain, float topSpeed)
    {
        float2 desiredVelocity = math.normalizesafe(targetPos - position) * topSpeed;
        float2 aimOffset = driftCorrectionGain * (desiredVelocity - velocity);
        return math.normalizesafe(desiredVelocity + aimOffset);
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


    // Update boat's heading, position, velocity, etc. based on the passed-in controls
    private void MoveBoat(ref BoatData boat, float deltaTime, float2 aim, float throttle)
    {
        boat.Heading = Turn(aim, boat.Heading, turnRate, deltaTime);
        float2 forward = Forward(boat.Heading);
        float2 acceleration = Acceleration(forward, throttle, forwardFriction, topSpeed);
        boat.Velocity += acceleration * deltaTime;
        ApplyFriction(ref boat, deltaTime);

        // clamp the velocity
        float vx = boat.Velocity.x;
        float vy = boat.Velocity.y;
        float topSpSqr = topSpeedReal * topSpeedReal;
        float velSqr = vx * vx + vy * vy;
        if (velSqr > topSpSqr)
        {
            // TODO inefficient, avoid sqrt if possible
            float factor = math.sqrt(velSqr / topSpSqr);
            boat.Velocity = velSqr / factor;
        }


        boat.Position += boat.Velocity * deltaTime;
    }


    // Calculates where the boat will turn based on where it's trying to aim
    private float Turn(float2 aim, float oldHeading, float turnRate, float deltaTime)
    {
        float aimHeading = HexProjection.Float2ToDegreesHeading(aim);
        float newHeading = Mathf.MoveTowardsAngle(oldHeading, aimHeading, turnRate * deltaTime);
        return Mathf.Repeat(newHeading, 360f);        // neaten the angle (not necessary)
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
