using Hyprx.DotEnv.Documents;

namespace Hyprx.DotEnv.Tokens;

public abstract class EnvScalarToken : EnvToken
{
    internal Capture Capture { get; set; } = Capture.None;
}