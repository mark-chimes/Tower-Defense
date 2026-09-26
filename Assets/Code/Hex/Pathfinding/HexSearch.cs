using System.Collections.Generic;

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
        public readonly IReadOnlyCollection<HexSignpost> Changed;

        public Delta(Phase phase, IReadOnlyCollection<HexSignpost> changed)
        {
            Phase = phase;
            Changed = changed;
        }
    }
}
