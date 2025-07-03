namespace Hyprx.DotEnv.Expansion;

public class ExpansionResult
{
    public string Value { get; set; } = string.Empty;

    public bool IsOk => this.Error is null;

    public bool IsError => this.Error is not null;

    public Exception? Error { get; set; }

    public int Position { get; set; } = -1;
}