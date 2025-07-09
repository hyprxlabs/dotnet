using System.Threading.Tasks;

using Hyprx.Collections.Generic;

using Hyprx.Exec;

namespace Hyprx.DotEnv.Expansion.Tests;

public class SopsEnvExpanderTests
{
    private static string? s_ageKeyOutput;

    [Fact]
    public static void Expand_Should_ExpandVariablesFromSopEnvFile()
    {
        if (!SopsEnabled())
        {
            Assert.Skip("Sops and Age CLI must be installed to run this test.");
        }

        var sopsData = """
        MY_SECRET="I am GROOT"
        NEW_SECRET="I am a new secret"
        """;

        var dir = Directory.GetCurrentDirectory();
        var sopsFilePath = Path.Combine(dir, "sops_values_1.env");
        if (File.Exists(sopsFilePath))
        {
            File.Delete(sopsFilePath);
        }

        var (privateKey, publicKey) = GetOrGenerateAgeKey();
        CreateEncryptedSopsFile(sopsFilePath, sopsData);
        var args = SecretExpression.ParseArgs($"secret sops-env:///{sopsFilePath} --name MY_SECRET");
        var expander = new SopsEnvExpander();
        expander.AgeKey = privateKey;
        expander.AgeRecipients = publicKey;
        Assert.True(expander.CanHandle(args), "Expander should be able to handle the Sops CLI Age Env expression.");
        var result = expander.Expand(args);
        Assert.NotNull(result);
        if (result.IsError)
        {
            Assert.Fail($"Expansion failed: {result.Error}");
        }

        Assert.Equal("I am GROOT", result.Value);
    }

    [Fact]
    public static void Expand_Should_CreateAndExpandSecrets()
    {
        if (!SopsEnabled())
        {
            Assert.Skip("Sops and Age CLI must be installed to run this test.");
        }

        var sopsData = """
        MY_SECRET="I am GROOT"
        NEW_SECRET="I am a new secret"
        """;

        var dir = Directory.GetCurrentDirectory();
        var sopsFilePath = Path.Combine(dir, "sops_values_2.env");
        if (File.Exists(sopsFilePath))
        {
            File.Delete(sopsFilePath);
        }

        var (privateKey, publicKey) = GetOrGenerateAgeKey();
        CreateEncryptedSopsFile(sopsFilePath, sopsData);
        var args = SecretExpression.ParseArgs($"secret sops-env:///{sopsFilePath} --name NEW_NEW_SECRET --create --size 10");
        var expander = new SopsEnvExpander();
        expander.AgeKey = privateKey;
        expander.AgeRecipients = publicKey;
        Assert.True(expander.CanHandle(args), "Expander should be able to handle the Sops CLI Age Env expression.");
        var result = expander.Expand(args);
        Assert.NotNull(result);
        if (result.IsError)
        {
            Assert.Fail($"Expansion failed: {result.Error}");
        }

        Console.WriteLine($"Created secret: {result.Value}");

        Assert.Equal(10, result.Value.Length);
        var data = File.ReadAllText(sopsFilePath);
        Assert.Contains("NEW_NEW_SECRET=", data, StringComparison.Ordinal);
    }

    [Fact]
    public static void Expand_Should_HandleRelativeFiles()
    {
        if (!SopsEnabled())
        {
            Assert.Skip("Sops and Age CLI must be installed to run this test.");
        }

        var sopsData = """
        MY_SECRET="I am GROOT"
        NEW_SECRET="I am a new secret"
        """;

        var dir = Directory.GetCurrentDirectory();
        var sopsFilePath = "./sops_age_3.env";
        if (File.Exists(sopsFilePath))
        {
            File.Delete(sopsFilePath);
        }

        var (privateKey, publicKey) = GetOrGenerateAgeKey();
        CreateEncryptedSopsFile(sopsFilePath, sopsData);
        var args = SecretExpression.ParseArgs($"secret sops-env:///$CWD/{sopsFilePath} --name new.new.secret --create --size 10");
        var expander = new SopsEnvExpander();
        expander.AgeKey = privateKey;
        expander.AgeRecipients = publicKey;
        Assert.True(expander.CanHandle(args), "Expander should be able to handle the Sops CLI Age Env expression.");
        var result = expander.Expand(args);
        Assert.NotNull(result);
        if (result.IsError)
        {
            Assert.Fail($"Expansion failed: {result.Error}");
        }

        Console.WriteLine($"Created secret: {result.Value}");

        Assert.Equal(10, result.Value.Length);
        var data = File.ReadAllText(sopsFilePath);
        Assert.Contains("NEW_NEW_SECRET=", data, StringComparison.Ordinal);
    }

    private static void CreateEncryptedSopsFile(string filePath, string content)
    {
        if (!SopsEnabled())
        {
            throw new InvalidOperationException("Sops and Age CLI must be installed to run this test.");
        }

        var (privateKey, publicKey) = GetOrGenerateAgeKey();

        Environment.SetEnvironmentVariable("SOPS_AGE_KEY", privateKey);
        Environment.SetEnvironmentVariable("SOPS_AGE_RECIPIENTS", publicKey);

        // Create a temporary file with the content
        File.WriteAllText(filePath, content);

        var o = new Exec.Command("sops")
            .WithArgs(["-e", "-i", filePath])
            .WithEnv(new StringMap()
            {
                { "SOPS_AGE_KEY", privateKey },
                { "SOPS_AGE_RECIPIENTS", publicKey },
            })
            .Output();

        o.ThrowOnBadExit();
    }

    private static (string PrivateKey, string PublicKey) GetOrGenerateAgeKey()
    {
        if (s_ageKeyOutput is null)
        {
            // Generate a new key pair using the Age CLI
            var o = new Exec.Command("age-keygen").Output();
            o.ThrowOnBadExit();

            s_ageKeyOutput = o.Text();
        }

        var output = s_ageKeyOutput.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var publicKey = output[1].Substring("# public key: ".Length).Trim();
        var privateKey = output[2].Trim();

        // If the key is already generated, return it
        return (privateKey, publicKey);
    }

    private static bool SopsEnabled()
    {
        var sopsPath = PathFinder.Which("sops");
        if (string.IsNullOrWhiteSpace(sopsPath))
        {
            return false; // Sops CLI is not installed
        }

        var agePath = PathFinder.Which("age");
        if (string.IsNullOrWhiteSpace(agePath))
        {
            return false; // Age CLI is not installed
        }

        return true; // Both Sops and Age CLI are installed
    }
}