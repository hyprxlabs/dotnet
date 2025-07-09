namespace Hyprx.DotEnv.Expansion;

public interface ISecretVaultExpander
{
    string Name { get; }

    string SecretsExpression { set; }

    bool CanHandle(IList<string> expressions);

    bool Synchronous { get; }

    bool IsDefault { get; }

    ExpansionResult Expand(IList<string> args);

    Task<ExpansionResult> ExpandAsync(
        IList<string> args,
        CancellationToken cancellationToken = default);
}