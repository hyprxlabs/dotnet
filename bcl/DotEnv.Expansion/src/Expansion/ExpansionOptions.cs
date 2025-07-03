namespace Hyprx.DotEnv.Expansion;

public class ExpansionOptions
{
    public bool EnableUnixArgs { get; set; } = false;

    public bool EnableWindowsVariables { get; set; } = false;

    public bool EnableSubExpressions { get; set; } = false;

    public bool EnableShell { get; set; } = false;

    public string Shell { get; set; } = "bash";

    public string SecretsExpression { get; set; } = "secret";

    public Dictionary<string, string> Variables { get; } = new(StringComparer.OrdinalIgnoreCase);

    public List<ISecretVaultExpander> SecretVaultExpanders { get; } = new();
}