namespace Hyprx.DotEnv;

public class DotEnvSerializerOptions
{
    public bool AllowBackticks { get; set; } = true;

    public bool AllowJson { get; set; }

    public bool AllowYaml { get; set; }

    public bool AllowSubExpressions { get; set; } = false;

    public virtual object Clone()
    {
        var copy = new DotEnvSerializerOptions()
        {
            AllowBackticks = this.AllowBackticks,
            AllowJson = this.AllowJson,
            AllowYaml = this.AllowYaml,
        };

        return copy;
    }
}