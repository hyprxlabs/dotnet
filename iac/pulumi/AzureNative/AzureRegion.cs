namespace Pulumi.AzureNative;

public readonly struct AzureRegion
{
    public AzureRegion(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        this.Name = name;
    }

    public string Name { get; }

    public static implicit operator AzureRegion(string name)
    {
        return new AzureRegion(name);
    }

    public static implicit operator string(AzureRegion region)
    {
        return region.Name;
    }

    public static AzureRegion EastUS => "eastus";

    public static AzureRegion EastUS2 => "eastus2";

    public static AzureRegion WestUS => "westus";

    public static AzureRegion WestUS2 => "westus2";

    public override string ToString()
    {
        return this.Name;
    }
}