using Hyprx.Collections.Generic;

namespace Hyprx.Rex.Collections;

public class Inputs : Map
{
    public Inputs()
        : base()
    {
    }

    public Inputs(IDictionary<string, object?> dictionary)
        : base(dictionary)
    {
    }

    public Inputs(IEqualityComparer<string>? comparer)
        : base(comparer ?? StringComparer.OrdinalIgnoreCase)
    {
    }

    public Inputs(int capacity)
        : base(capacity)
    {
    }
}