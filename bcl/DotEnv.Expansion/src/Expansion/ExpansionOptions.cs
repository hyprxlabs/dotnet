namespace Hyprx.DotEnv.Expansion;

public class ExpansionOptions
{
    public bool EnableUnixArgs { get; set; } = false;

    public bool EnableWindowsVariables { get; set; } = false;

    public bool EnableCommandSubstitution { get; set; } = false;

    public bool EnableSecretSubstitution { get; set; } = false;

    public bool EnableShellExecution { get; set; } = false;

    public string UseShell { get; set; } = "bash";

    public string SecretsExpression { get; set; } = "secret";

    public Dictionary<string, string> Variables { get; } = new(StringComparer.OrdinalIgnoreCase);

    public List<ISecretVaultExpander> SecretVaultExpanders { get; } = new();
}