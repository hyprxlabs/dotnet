using Hyprx.Rex.Collections;

namespace Hyprx.Rex.Tasks;

public class TaskHandlerMap : Map<ITaskHandler>
{
    public TaskHandlerMap()
    {
    }

    public TaskHandlerMap(int capacity)
        : base(capacity)
    {
    }

    public TaskHandlerMap(IEqualityComparer<string> comparer)
        : base(comparer)
    {
    }

    public TaskHandlerMap(IDictionary<string, ITaskHandler> dictionary)
        : base(dictionary)
    {
    }

    public static TaskHandlerMap Global { get; } = new TaskHandlerMap();
}