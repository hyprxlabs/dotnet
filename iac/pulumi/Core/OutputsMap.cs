namespace Pulumi;

public class OutputsMap : Dictionary<string, object?>
{
    public OutputsMap() 
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    public OutputsMap(IDictionary<string, object?> dictionary) 
        : base(dictionary, StringComparer.OrdinalIgnoreCase)
    {
    }

    public OutputsMap(int capacity) 
        : base(capacity, StringComparer.OrdinalIgnoreCase)
    {
    }

    public OutputsMap(IEnumerable<KeyValuePair<string, object?>> collection) 
        : base(collection, StringComparer.OrdinalIgnoreCase)
    {
    }
}