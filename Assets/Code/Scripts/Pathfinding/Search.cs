using System.Collections.Generic;

public static class Search
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
        public readonly IReadOnlyCollection<Signpost> Changed;

        public Delta(Phase phase, IReadOnlyCollection<Signpost> changed)
        {
            Phase = phase;
            Changed = changed;
        }
    }
}