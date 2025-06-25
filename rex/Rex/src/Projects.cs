namespace Hyprx.Rex;

public static class Project
{
    public static string? FindProject(string? workingDirectory)
    {
        var cwd = workingDirectory ?? Environment.CurrentDirectory;

        var redirect = Path.Join(cwd, "rexfile");
        if (File.Exists(redirect))
        {
            foreach (var line in File.ReadLines(redirect))
            {
                var text = line.Trim();
                if (text.StartsWith("rexfile:", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = text.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        text = parts[1].Trim();
                        if (!Path.IsPathFullyQualified(text))
                            return Path.GetFullPath(text, cwd);

                        return text;
                    }
                }
            }
        }

        var file = Path.Join(cwd, "rexfile.cs");
        if (File.Exists(file))
            return file;

        var rexDir = Path.Join(cwd, ".rex");
        if (!Directory.Exists(rexDir))
            return null;

        var first = Directory.EnumerateFiles(rexDir, "*.csproj").FirstOrDefault();
        if (first is not null)
            return first;

        var mainFile = Path.Join(rexDir, "main.cs");
        if (File.Exists(mainFile))
            return mainFile;

        return null;
    }
}