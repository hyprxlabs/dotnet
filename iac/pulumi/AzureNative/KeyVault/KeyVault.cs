
using Pulumi.AzureNative.DependencyMap;
using Pulumi.AzureNative.KeyVault.Inputs;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Resources.Inputs;

namespace Pulumi.AzureNative.KeyVault;

public static class KeyVaults
{

    public static void New(string id)
    {
        new Pulumi.AzureNative.KeyVault.Vault($"kv-{id}", new VaultArgs()
        {
            VaultName = AzureNameBuilder.Build(AzureResourcePrefix.KeyVault, id, DeployEnv.Production, AzureRegion.EastUS),
            Location = AzureRegion.EastUS,
            Properties = new VaultPropertiesArgs
            {
                EnableRbacAuthorization = true,
                Sku = new SkuArgs
                {
                    Name = SkuName.Standard,
                },

            },
        },

        new CustomResourceOptions()
        {

        })
    }
}

public class KeyVaultBuilder
{
    private string id;

    private bool? rbacAuthorization;

    private SkuName? skuName;

    private AzureRegion? region;

    public DeployEnv? env;

    private CustomResourceOptions? options;

    private Input<string>? resourceGroupName;

    private VaultPropertiesArgs properties = new();

    private InputMap<string>? tags;

    public KeyVaultBuilder(string id, AzureRegion region, DeployEnv? env)
    {
        this.id = id;
        this.region = region;
        this.env = env;
        this.properties.EnableRbacAuthorization = true;
        this.properties.Sku = KeyVaultSkus.Standard;
    }

    public KeyVaultBuilder WithId(string id)
    {
        this.id = id;
        return this;
    }

    public VaultPropertiesArgs WithProperties(VaultPropertiesArgs properties)
    {
        this.properties = properties;
        return this.properties;
    }

    public KeyVaultBuilder WithRbacAuthorization(bool rbacAuthorization = true)
    {
        this.properties.EnableRbacAuthorization = rbacAuthorization;
        return this;
    }

    public KeyVaultBuilder WithEnablePurgeProtection(bool enable = true)
    {
        this.properties.EnableSoftDelete = enable;
        return this;
    }

    public KeyVaultBuilder WithEnableForDiskEncryption(bool enable = true)
    {
        this.properties.EnabledForDiskEncryption = enable;
        return this;
    }

    public KeyVaultBuilder WithEnableForDeployment(bool enable = true)
    {
        this.properties.EnabledForDeployment = enable;

        return this;
    }

    public KeyVaultBuilder WithEnableForTemplateeployment(bool enable = true)
    {
        this.properties.EnabledForDeployment = enable;
        return this;
    }

    public KeyVaultBuilder WithAccessPolicies(List<AccessPolicyEntryArgs> accessPolicies)
    {
        this.properties.AccessPolicies = accessPolicies;
        return this;
    }

    public KeyVaultBuilder WithSku(Inputs.SkuArgs sku)
    {
        this.properties.Sku = sku;
        return this;
    }

    public KeyVaultBuilder WithRegion(AzureRegion region)
    {
        this.region = region;
        return this;
    }

    public KeyVaultBuilder DisableSoftDelete()
    {
        this.properties.EnableSoftDelete = false;
        this.properties.SoftDeleteRetentionInDays = 0;
        return this;
    }

    public KeyVaultBuilder DisablePublicNetworkAccess()
    {
        this.properties.PublicNetworkAccess = "disabled";
        return this;
    }

    public KeyVaultBuilder WithResourceGroup(Input<string> resourceGroupName)
    {
        this.resourceGroupName = resourceGroupName;
        return this;
    }

    public KeyVaultBuilder WithResourceGroup(ResourceGroup group)
    {
        this.resourceGroupName = group.Name;
        return this;
    }

    public KeyVaultBuilder WithTags(Dictionary<string, string> tags)
    {
        this.tags = tags;
        return this;
    }

    public KeyVaultBuilder SetTag(string name, string value)
    {
        this.tags ??= new InputMap<string>();
        this.tags[name] = value;
        return this;
    }

    public Vault Build()
    {
        return new Vault(
            $"kv-{this.id}",
            new VaultArgs()
            {
                VaultName = AzureNameBuilder.Build(AzureResourcePrefix.KeyVault, this.id, this.env ?? DeployEnv.Production, this.region ?? AzureRegion.EastUS),
                Location = this.region ?? AzureRegion.EastUS,
                ResourceGroupName = this.resourceGroupName!,
                Properties = this.properties,
                Tags
            },
            this.options);
    }

}