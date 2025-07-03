using System.Threading.Tasks;

using Hyprx.Exec;

namespace Hyprx.DotEnv.Expansion.Tests;

public static class DotEnvExpanderTests
{
    private static readonly string s_defaultEnv =
"""
# Default environment variables
NORMAL_VAR=normal value
MY_HOME=$HOME
MY_HOME2=${HOME}

MY_HOME3=${HOME3:-/default/home}
SECRET1=$(az keyvault secret show --name secret1 --vault-name kv-hyprx-tmp --query value -o tsv)
SECRET2=$(secret akv:///kv-hyprx-tmp/secret2)
""";

    [Fact]
    public static void OOB_Expansion_Test()
    {
        // normalize the environment variables
        if (OperatingSystem.IsWindows())
        {
            Environment.SetEnvironmentVariable("HOME", Environment.GetEnvironmentVariable("USERPROFILE"));
        }

        var home = Environment.GetEnvironmentVariable("HOME");
        var expander = new ExpansionBuilder().Build();
        var env = DotEnvSerializer.DeserializeDocument(s_defaultEnv);
        var summary = expander.Expand(env);

        Assert.NotNull(summary);
        Assert.True(summary.IsOk);

        env = summary.Value;
        Assert.Equal("normal value", env["NORMAL_VAR"]);
        Assert.Equal(home, env["MY_HOME"]);
        Assert.Equal(home, env["MY_HOME2"]);
        Assert.Equal("/default/home", env["MY_HOME3"]);

        // by default, command expansion is not enabled
        Assert.Equal("$(az keyvault secret show --name secret1 --vault-name kv-hyprx-tmp --query value -o tsv)", env["SECRET1"]);
        Assert.Equal("$(secret akv:///kv-hyprx-tmp/secret2)", env["SECRET2"]);
    }

    [Fact]
    public static void Verify_Command_Expansion()
    {
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

        if (OperatingSystem.IsWindows())
        {
            Environment.SetEnvironmentVariable("HOME", Environment.GetEnvironmentVariable("USERPROFILE"));
        }

        var home = Environment.GetEnvironmentVariable("HOME");
        var expander = new ExpansionBuilder()
            .WithExpressions()
            .Build();
        var env = DotEnvSerializer.DeserializeDocument(s_defaultEnv);
        var summary = expander.Expand(env);

        Assert.NotNull(summary);
        Assert.True(summary.IsError);
        Assert.True(summary.Errors.Count is 1);
        Assert.Equal("No secret vault expanders configured.", summary.Errors[0].Message);

        env = summary.Value;
        Assert.Equal("normal value", env["NORMAL_VAR"]);
        Assert.Equal(home, env["MY_HOME"]);
        Assert.Equal(home, env["MY_HOME2"]);
        Assert.Equal("/default/home", env["MY_HOME3"]);
        Assert.Equal("I am GROOT", env["SECRET1"]);
        Assert.Equal("$(secret akv:///kv-hyprx-tmp/secret2)", env["SECRET2"]);
    }
}
