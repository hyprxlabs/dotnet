namespace Pulumi.AzureNative;

public abstract class AzureResourceBuilder<TBuilder, TResource, TResourceArgs>
   where TBuilder : AzureResourceBuilder<TBuilder, TResource, TResourceArgs>
   where TResourceArgs : class, new()
{
    private string workload = string.Empty;

    private Input<string> resourceGroupName = string.Empty;

    protected TResourceArgs Args { get; private set; }

    protected CustomResourceOptions? Options { get; private set; }

    protected AzureResourceBuilder()
    {
        this.Args = new TResourceArgs();
    }

    public TBuilder WithArgs(TResourceArgs args)
    {
        this.Args = args;
        return (TBuilder)this;
    }

    protected abstract string GenerateId();

    public virtual TResource Build()
    {
        return (TResource)Activator.CreateInstance(
            typeof(TResource),
            this.GenerateId(),
            this.Args,
            this.Options) !;
    }
}
