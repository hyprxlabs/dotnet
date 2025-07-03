namespace Hyprx.DotEnv.Expansion;

public class ExpansionResult
{
    public string Value { get; set; } = string.Empty;

    public bool IsOk { get; internal protected set; } = true;

    public bool IsError => !this.IsOk;

    public Exception? Error { get; set; }

    public int Position { get; set; } = -1;
}