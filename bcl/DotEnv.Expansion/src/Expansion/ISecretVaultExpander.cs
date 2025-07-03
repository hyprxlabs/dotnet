namespace Hyprx.DotEnv.Expansion;

public interface ISecretVaultExpander
{
    string Name { get; }

    string SecretsExpression { set; }

    bool CanHandle(string innerExpression);

    bool Synchronous { get; }

    ExpansionResult Expand(string innerExpression);

    Task<ExpansionResult> ExpandAsync(
        string innerExpression,
        CancellationToken cancellationToken = default);
}