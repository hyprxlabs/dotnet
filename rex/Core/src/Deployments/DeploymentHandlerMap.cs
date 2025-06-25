using Hyprx.Rex.Collections;

namespace Hyprx.Rex.Deployments;

public class DeploymentHandlerMap : Map<IDeploymentHandler>
{
    public DeploymentHandlerMap()
    {
    }

    public DeploymentHandlerMap(int capacity)
        : base(capacity)
    {
    }

    public DeploymentHandlerMap(IEqualityComparer<string> comparer)
        : base(comparer)
    {
    }

    public DeploymentHandlerMap(IDictionary<string, IDeploymentHandler> dictionary)
        : base(dictionary)
    {
    }

    public static DeploymentHandlerMap Global { get; } = new DeploymentHandlerMap();
}