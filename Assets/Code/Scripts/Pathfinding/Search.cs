using System.Collections.Generic;

public class Search
{

    readonly struct Delta
    {
        readonly Phase phase;
        readonly IReadOnlyList<Signpost> Changed;
    }

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
        Dual
    }



}