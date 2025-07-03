namespace Hyprx.DotEnv;

public class DotEnvLoadOptions : DotEnvSerializerOptions
{
    public IReadOnlyList<string> Files { get; set; } = Array.Empty<string>();

    public string? Content { get; set; }

    public bool OverrideEnvironment { get; set; }

    public override object Clone()
    {
        var copy = new DotEnvLoadOptions()
        {
            AllowBackticks = this.AllowBackticks,
            AllowJson = this.AllowJson,
            AllowYaml = this.AllowYaml,
            Files = this.Files,
            Content = this.Content,
            OverrideEnvironment = this.OverrideEnvironment,
        };

        return copy;
    }
}