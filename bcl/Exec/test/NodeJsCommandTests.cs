namespace Hyprx.Exec.Tests;

public static class NodeJsCommandTests
{
    [Fact]
    public static void RunScript()
    {
        var node = PathFinder.Which("node");
        Assert.SkipWhen(node.IsNullOrWhiteSpace(), "NodeJs is not available on this system. Skipping test.");

        // Example of how to run a command and handle the output
        var output = new NodeJsCommand()
           .RunScript("console.log('Hello, World!')");

        Assert.Equal(0, output.ExitCode);
    }

    [Fact]
    public static async ValueTask RunScriptAsync()
    {
        var node = PathFinder.Which("node");
        Assert.SkipWhen(node.IsNullOrWhiteSpace(), "NodeJs is not available on this system. Skipping test.");

        // Example of how to run a command asynchronously and handle the output
        var output = await new NodeJsCommand()
           .RunScriptAsync("console.log('Hello, World!')", TestContext.Current.CancellationToken);

        Assert.Equal(0, output.ExitCode);
    }

    [Fact]
    public static void OutputScript()
    {
        var node = PathFinder.Which("node");
        Assert.SkipWhen(node.IsNullOrWhiteSpace(), "NodeJs is not available on this system. Skipping test.");

        // Example of how to run a command and get the output
        var output = new NodeJsCommand()
           .OutputScript("console.log('Hello, World!')");

        Assert.Equal(0, output.ExitCode);
        Assert.NotEmpty(output.Stdout);

        var text = output.Text().Trim();
        Assert.NotEmpty(text);
        Assert.Equal("Hello, World!", text);
    }

    [Fact]
    public static async ValueTask OutputScriptAsync()
    {
        var node = PathFinder.Which("node");
        Assert.SkipWhen(node.IsNullOrWhiteSpace(), "NodeJs is not available on this system. Skipping test.");

        // Example of how to run a command asynchronously and get the output
        var output = await new NodeJsCommand()
           .OutputScriptAsync("console.log('Hello, World!')", TestContext.Current.CancellationToken);

        Assert.Equal(0, output.ExitCode);
        Assert.NotEmpty(output.Stdout);
        Console.WriteLine(output.Stdout[output.Stdout.Length - 1]);
        Console.WriteLine("stdout length: {0}", output.Stdout.Length);

        var text = output.Text().Trim();
        Assert.NotEmpty(text);
        Assert.Equal("Hello, World!", text);
    }
}