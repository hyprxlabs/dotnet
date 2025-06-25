using System.CommandLine;

namespace Hyprx.Rex.Commands;

public static class Options
{
    public static Option<bool> Verbose { get; } = new("--verbose", "-v")
    {
        Description = "Enable verbose output.",
        Arity = ArgumentArity.ZeroOrOne,
        DefaultValueFactory = _ => false,
    };

    public static Option<FileInfo?> File { get; } = new("--file", "-f")
    {
        Description = "The file to run.",
        Arity = ArgumentArity.ExactlyOne,
    };

    public static Argument<string> Target { get; } = new("target")
    {
        Description = "The target to run.",
        Arity = ArgumentArity.ExactlyOne,
    };

    public static Option<string> Context { get; } = new("--context", "-c")
    {
        Description = "The context to run the command in.",
        Arity = ArgumentArity.ExactlyOne,
        DefaultValueFactory = _ => "default",
    };

    public static Argument<string?> Service { get; } = new("service")
    {
        Description = "The service to run.",
        Arity = ArgumentArity.ZeroOrOne,
    };

    public static Argument<string[]> Targets { get; } = new("targets")
    {
        Description = "The targets to run.",
        Arity = ArgumentArity.ZeroOrMore,
    };

    public static Option<string[]> Env { get; } = new("--env", "-e")
    {
        Description = "Set one or more environment variables in the form KEY=VALUE.",
        Arity = ArgumentArity.ZeroOrMore,
        DefaultValueFactory = _ => [],
    };

    public static Option<FileInfo[]> EnvFiles { get; } = new("--env-file", "-E")
    {
        Description = "Load environment variables from one or more files.",
        Arity = ArgumentArity.ZeroOrMore,
        DefaultValueFactory = _ => [],
    };

    public static Option<FileInfo[]> SecretFiles { get; } = new("--secret-file", "-S")
    {
        Description = "Load secrets from one or more files.",
        Arity = ArgumentArity.ZeroOrMore,
        DefaultValueFactory = _ => [],
    };

    public static Option<int?> Timeout { get; } = new("--timeout", "-t")
    {
        Description = "Set the timeout in minutes for the command.",
        Arity = ArgumentArity.ExactlyOne,
    };
}