using System.Collections.Generic;

// Keep name for now until it's better organized
public class HexSearch
{
    public enum Phase
    {
        ExpandFrontier,
        TracePath,
        Done,
    }

    public enum Dir
    {
        FromStart,
        FromEnd,
        Dual,
    }

    public readonly struct Delta
    {
        public readonly Phase Phase;
        public readonly IReadOnlyCollection<FlowSample> Changed;

        public Delta(Phase phase, IReadOnlyCollection<FlowSample> changed)
        {
            Phase = phase;
            Changed = changed;
        }
    }
}
