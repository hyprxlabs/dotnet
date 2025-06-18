using Pulumi.AzureNative.KeyVault;

namespace Pulumi.AzureNative.KeyVault;

public static class KeyVaultSkus
{
    public static Inputs.SkuArgs Standard => new Inputs.SkuArgs
    {
        Family = SkuFamily.A,
        Name = SkuName.Standard,
    };

    public static Inputs.SkuArgs Premium => new Inputs.SkuArgs
    {
        Family = SkuFamily.A,
        Name = SkuName.Premium,
    };
}