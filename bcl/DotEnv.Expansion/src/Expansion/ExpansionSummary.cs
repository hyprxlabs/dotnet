using System.ComponentModel;

using Hyprx.DotEnv.Documents;

namespace Hyprx.DotEnv.Expansion;

public class ExpansionSummary
{
    public ExpansionSummary(DotEnvDocument value)
    {
        this.Value = value;
    }

    public DotEnvDocument Value { get; set; }

    public List<DotEnvExpansionError> Errors { get; set; } = new();

    public bool IsOk => this.Errors.Count == 0;

    public bool IsError => !this.IsOk;
}

public class DotEnvExpansionError
{
    public string Message { get; set; } = string.Empty;

    public Exception? Exception { get; set; }

    public int Index { get; set; } = -1;

    public string Key { get; set; } = string.Empty;

    public int Position { get; set; } = -1;

    public override string ToString()
    {
        return this.Message;
    }
}