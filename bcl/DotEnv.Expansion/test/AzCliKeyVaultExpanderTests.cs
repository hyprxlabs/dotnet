using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

using Hyprx.Exec;

namespace Hyprx.DotEnv.Expansion.Tests;

public static class AzCliKeyVaultExpanderTests
{
    [Fact]
    public static void Verify_AzCli_KeyVault_Expansion()
    {
        var content =
"""
SECRET3=$(secret akv://kv-hyprx-tmp/secret3 --create --size 32)
""";

        var azPath = PathFinder.Which("az");
        if (string.IsNullOrWhiteSpace(azPath))
        {
            Assert.Skip("Azure CLI (az) is not installed.");
        }

        var az = new Command(azPath);

        var o1 = az.Run(["account", "list-locations"]);
        if (o1.ExitCode != 0)
        {
            Assert.Skip("Azure CLI is not logging in.");
        }

        var o = az.Run(["keyvault", "secret", "show", "--name", "secret3", "--vault-name", "kv-hyprx-tmp", "--query", "value", "-o", "tsv"]);
        if (o.IsOk)
        {
            o = az.Run(["keyvault", "secret", "delete", "--name", "secret3", "--vault-name", "kv-hyprx-tmp"]);
            if (o.IsError)
            {
                Assert.Fail($"Failed to delete secret3: {o.Error}");
            }

            o = az.Run(["keyvault", "secret", "purge", "--name", "secret3", "--vault-name", "kv-hyprx-tmp"]);
            if (o.IsError)
            {
                Assert.Fail($"Failed to purge secret3: {o.Error}");
            }
        }

        var expander = new ExpansionBuilder()
            .WithExpressions()
            .AddAzureCliKeyVault()
            .Build();

        var env = DotEnvSerializer.DeserializeDocument(content);
        var summary = expander.Expand(env);

        if (summary.IsError)
        {
            foreach (var error in summary.Errors)
            {
                Console.WriteLine($"Error: {error}");
            }
        }

        var value = env["SECRET3"];
        Console.WriteLine($"SECRET3: {value}");
        Assert.NotEqual("$(secret akv:///kv-hyprx-tmp/secret3 --create --size 32)", value);
    }
}