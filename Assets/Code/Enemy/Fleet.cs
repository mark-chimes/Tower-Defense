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

    private float bigBoatRadius = 2.5f; // meters
    private float smallBoatRadius = 0.5f; // meters

    // between 0 and 1
    // 0 means boats will wait politely for each other
    // 1 means boats will shove each other roughly
    private float pushiness = 0.5f;


    /****************************************/

    // Coast values
    // Probably constant for all boats, but could put some on boat personalities later

    // soft push starts when the hull is this close to land
    private float coastMargin = 0.5f; // meters

    private float coastSoftRate = 5f;    // per second

    // added once the hull overlaps the land
    private float coastStrongRate = 10f;  // per second

    // How close a ship's center is placed from land when it gets moved out of a land hex
    private float landGap = 0.01f;

    /****************************************/


    // Other constants and tunable values

    float veryClose = 0.001f; // boats within 1mm of each other

    private float maxSpawnOffset = 2f; // meters

    uint randomSeed = 1;

    /****************************************/

    private Random random;

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

    // Minimum translation vector: the smallest move that resolves an overlap
    // Sums of minimum translations needed to move boats apart, applied after all pairs are checked
    float2[] mtvSums;

    // Sum of velocity changes from boat contacts, applied after all pairs are checked
    float2[] vcSums;

    private TreasureMap treasureMap;

    public int BoatCount { get; private set; }
    public int SlotsUsed { get; private set; }



    public Fleet(TreasureMap treasureMap, int capacity)
    {
        this.treasureMap = treasureMap;
        boats = new BoatData[capacity];
        mtvSums = new float2[capacity];
        vcSums = new float2[capacity];

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
            AdjustForLand(ref boat, deltaTime);

            if (DespawnsAtGoal && IsAtGoal(boat.Position))
            {
                DespawnIfPossible(i);
                // continue;
            }
        }
    }

    // TODO maybe split the calculation of this force from the actual application?
    private void AdjustForLand(ref BoatData boat, float deltaTime)
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
            MoveOutOfLand(ref boat, coord, neighbors);
            return;
        }
        PushFromCoast(ref boat, coord, neighbors, deltaTime);
    }

    private void PushFromCoast(ref BoatData boat, HexCoord coord,
        HexCoord[] neighbors, float deltaTime)
    {
        float2 seaCenter = HexProjection.CoordsToFloat2(coord);

        // cap at 1 in case of large deltaTime. 1 already pushes us all the way out.
        float softPerStep = math.min(coastSoftRate * deltaTime, 1f);
        float strongPerStep = math.min(coastStrongRate * deltaTime, 1f);

        foreach (HexCoord neighbor in neighbors)
        {
            // can skip non-land hexes, but off-map hexes are treated as land
            if (treasureMap.Contains(neighbor) && !treasureMap.IsLand(neighbor)) continue;
            float2 nCenter = HexProjection.CoordsToFloat2(neighbor);
            float2 seaToEdgeDir = (nCenter - seaCenter) / HexProjection.CellWidth;

            float2 pos = boat.Position;

            float2 closestEdgePoint = ClosestEdgePoint(seaCenter, seaToEdgeDir, pos);
            float2 edgeToBoat = pos - closestEdgePoint;

            // This requires sqrt. Check it when profiling
            float distFromEdge = math.length(edgeToBoat);

            float2 pushDir;
            if (distFromEdge < veryClose)
            {
                // if we are too close for a direction vector to make sense
                // just push us perpendicularly away from the edge
                pushDir = -seaToEdgeDir;
            }
            else
            {
                // unit vector from the edge to the boat
                pushDir = edgeToBoat / distFromEdge;
            }

            float hullOverhang = bigBoatRadius - distFromEdge;

            // if we are heading into the land
            // then remove all the velocity pointing towards the land
            if (hullOverhang > 0)
            {
                var intoLandSpeed = math.dot(boat.Velocity, -pushDir);
                if (intoLandSpeed > 0)
                {
                    boat.Velocity += pushDir * intoLandSpeed;
                }
            }

            float softDepth = math.max(0f, hullOverhang + coastMargin);
            float strongDepth = math.max(0f, hullOverhang);

            float softPush = softDepth * softPerStep;
            float strongPush = strongDepth * strongPerStep;

            // push away from edge
            boat.Position += (softPush + strongPush) * pushDir;


        }
    }

    private static float2 ClosestEdgePoint(float2 hexCenter, float2 hexToEdgeDir, float2 pos)
    {
        float2 edgeMid = hexCenter + hexToEdgeDir * HexProjection.Inradius;

        // perpendicular vector to hexToEdgeDir 
        float2 alongEdgeDir = new float2(hexToEdgeDir.y, -hexToEdgeDir.x);

        float2 midToPos = pos - edgeMid;

        // projection giving how far from center of the edge the pos is
        float offsetAlongEdge = math.dot(midToPos, alongEdgeDir);
        offsetAlongEdge = math.clamp(offsetAlongEdge, -HexProjection.HalfEdge, HexProjection.HalfEdge);

        float2 closestEdgePoint = edgeMid + alongEdgeDir * offsetAlongEdge;
        return closestEdgePoint;
    }


    private void MoveOutOfLand(ref BoatData boat, HexCoord coord, HexCoord[] neighbors)
    {
        float2 pos = boat.Position;
        float2 landCenter = HexProjection.CoordsToFloat2(coord);

        float nearestDistSquared = float.MaxValue;
        float2 bestToSeaDir = float2.zero;
        float2 bestPoint = float2.zero;
        bool isNearestSeaFound = false;

        foreach (HexCoord neighbor in neighbors)
        {
            // can skip non-land hexes and off-map hexes
            // TODO maybe we can re-use this code for off-map hexes?
            if (!treasureMap.Contains(neighbor) || treasureMap.IsLand(neighbor)) continue;

            isNearestSeaFound = true;
            float2 nPos = HexProjection.CoordsToFloat2(neighbor);

            // unit vector: land hex towards the edge shared with this sea neighbor
            float2 landToSeaDir = (nPos - landCenter) / HexProjection.CellWidth;
            float2 closestEdgePoint = ClosestEdgePoint(landCenter, landToSeaDir, pos);
            float distToClosestEdgePointSqr = math.distancesq(pos, closestEdgePoint);

            if (distToClosestEdgePointSqr < nearestDistSquared)
            {
                bestPoint = closestEdgePoint;
                bestToSeaDir = landToSeaDir;
                nearestDistSquared = distToClosestEdgePointSqr;
            }
        }

        // If none of the valid neighbors is sea, nothing to be done for now
        if (!isNearestSeaFound) return;
        boat.Position = bestPoint + bestToSeaDir * landGap;

        // If we are moving back towards the land, stop that!
        float intoLandSpeed = math.dot(boat.Velocity, -bestToSeaDir);
        if (intoLandSpeed > 0)
        {
            boat.Velocity += bestToSeaDir * intoLandSpeed;
        }

    }

    private void ShiftBoatsAwayFromEachOther(float deltaTime)
    {
        float pushFactor = 10; // Arbitrary value for now, TODO move out to top
        float shiftFactor = 2; // Arbitrary value for now, TODO move out to top
        float separationRate = 10;

        // Only check each pair of boats once
        // Outer loop starts at 1, inner loop stays strictly below i
        for (int i = 1; i < SlotsUsed; i++)
        {
            if (!IsAlive(i)) continue;
            ref BoatData boat1 = ref boats[i];
            for (int j = 0; j < i; j++)
            {
                if (!IsAlive(j)) continue;
                ref BoatData boat2 = ref boats[j];
                float R = bigBoatRadius + bigBoatRadius; // boat 1 radius + boat 2 radius

                float2 pos1 = boat1.Position;
                float2 pos2 = boat2.Position;
                float2 oneToTwo = (pos2 - pos1);
                float dx = oneToTwo.x;
                float dy = oneToTwo.y;

                float dSquared = dx * dx + dy * dy;
                // boats aren't touching: 
                if (dSquared > R * R) continue;

                // TODO I don't want to use sqrt in a loop like this if I can help it
                float d = math.sqrt(dSquared);
                float overlap = R - d;

                float2 dir1to2;
                if (d < veryClose)
                {
                    // arbitrarily move one East and the other West
                    dir1to2 = new float2(1f, 0f);
                }
                else
                {
                    dir1to2 = oneToTwo / d;
                }

                // minimum translation vectors - smallest move that resolves the overlap
                float2 mtv2 = dir1to2 * overlap;
                float2 mtv1 = -mtv2; // shift boat 1 away from 2

                mtvSums[i] += mtv1;
                mtvSums[j] += mtv2;

                // velocity change to correct for being close to each other
                float2 relativeVelocity = boat1.Velocity - boat2.Velocity;
                float closingSpeed = math.dot(relativeVelocity, dir1to2);

                // boats are moving closer together, so we should cancel velocity in that direction
                if (closingSpeed > 0)
                {
                    float2 vc2 = dir1to2 * closingSpeed * 0.5f * (1f - pushiness);
                    float2 vc1 = -vc2; // boat 1 doesn't drive into boat 2
                    vcSums[i] += vc1;
                    vcSums[j] += vc2;
                }
            }
        }

        float separationAmount = math.min(separationRate * deltaTime, 1f) * 0.5f;
        for (int i = 0; i < SlotsUsed; i++)
        {
            boats[i].Position += mtvSums[i] * separationAmount;
            boats[i].Velocity += vcSums[i]; // doesn't use deltaTime

            mtvSums[i] = float2.zero;
            vcSums[i] = float2.zero;
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
            boat.Velocity = boat.Velocity / factor;
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
