using Hyprx.Collections.Generic;

namespace Hyprx.Rex.Collections;

public class Outputs : Map
{
    public Outputs()
        : base()
    {
    }

    public Outputs(IDictionary<string, object?> dictionary)
        : base(dictionary)
    {
    }

    public Outputs(IEqualityComparer<string>? comparer)
        : base(comparer ?? StringComparer.OrdinalIgnoreCase)
    {
    }

    public Outputs(int capacity)
        : base(capacity)
    {
    }
}