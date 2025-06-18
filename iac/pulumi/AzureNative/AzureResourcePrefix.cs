namespace Pulumi.AzureNative;

public readonly struct AzureResourcePrefix
{
    public AzureResourcePrefix(string prefix)
    {
        ArgumentException.ThrowIfNullOrEmpty(prefix, nameof(prefix));
        this.Prefix = prefix;
    }

    public string Prefix { get; }

    public static implicit operator AzureResourcePrefix(string prefix)
    {
        return new AzureResourcePrefix(prefix);
    }

    public static implicit operator string(AzureResourcePrefix prefix)
    {
        return prefix.Prefix;
    }

    public static AzureResourcePrefix AppService => new AzureResourcePrefix("app");

    public static AzureResourcePrefix ContainerRegistry => new AzureResourcePrefix("cr");

    public static AzureResourcePrefix ManagementGroup => "mg";

    public static AzureResourcePrefix KubernetesCluster => "aks";

    public static AzureResourcePrefix KubernetesSystemNodePool => "npsystem";

    public static AzureResourcePrefix KubernetesUserNodePool => "np";

    public static AzureResourcePrefix CloudService => new AzureResourcePrefix("cld");

    public static AzureResourcePrefix ContainerApp => new AzureResourcePrefix("ca");

    public static AzureResourcePrefix ContainerAppEnvironment => new AzureResourcePrefix("cae");

    public static AzureResourcePrefix ResourceGroup => new AzureResourcePrefix("rg");

    public static AzureResourcePrefix StorageAccount => new AzureResourcePrefix("st");

    public static AzureResourcePrefix FileShare => new AzureResourcePrefix("fs");

    public static AzureResourcePrefix BackupVault => new AzureResourcePrefix("bvault");

    public static AzureResourcePrefix AzureStorSimple => new AzureResourcePrefix("ssimp");

    public static AzureResourcePrefix KeyVault => new AzureResourcePrefix("kv");

    public static AzureResourcePrefix SshKey => new AzureResourcePrefix("sshkey");

    public static AzureResourcePrefix VirtualNetwork => new AzureResourcePrefix("vnet");

    public static AzureResourcePrefix Subnet => new AzureResourcePrefix("snet");

    public static AzureResourcePrefix VirtualWanHub => new AzureResourcePrefix("vhub");

    public static AzureResourcePrefix VirtualWan => new AzureResourcePrefix("vwan");

    public static AzureResourcePrefix VirtualNetworkPeering => new AzureResourcePrefix("peer");

    public static AzureResourcePrefix PublicIpAddress => new AzureResourcePrefix("pip");

    public static AzureResourcePrefix NetworkSecurityGroup => new AzureResourcePrefix("nsg");

    public static AzureResourcePrefix NetworkInterface => new AzureResourcePrefix("nic");

    public static AzureResourcePrefix VirtualMachine => new AzureResourcePrefix("vm");

    public static AzureResourcePrefix VirtualMachineScaleSet => new AzureResourcePrefix("vmss");

    public static AzureResourcePrefix LoadBalancer => new AzureResourcePrefix("lb");

    public static AzureResourcePrefix WebApp => new AzureResourcePrefix("app");

    public static AzureResourcePrefix FunctionApp => new AzureResourcePrefix("func");

    public static AzureResourcePrefix StaticWebApp => new AzureResourcePrefix("swa");

    public static AzureResourcePrefix ManagedIdentity => new AzureResourcePrefix("id");

    public static AzureResourcePrefix ContainerInstance => new AzureResourcePrefix("ci");

    public override string ToString()
    {
        return this.Prefix;
    }
}