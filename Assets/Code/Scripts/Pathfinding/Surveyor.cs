public class Surveyor 
{ 
    public readonly FlowField flow;
    public Coord SpawnPos { get; }
    public Coord GoalPos { get; }

    public Surveyor(int width, int height, Coord spawnPos, Coord goalPos)
    {
        flow = new FlowField(width, height);

    }
}
