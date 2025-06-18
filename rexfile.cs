#!/usr/bin/dotnet run
#:package Hyprx.Ansi
#:package Hyprx.Core
#:package Hyprx.Exec
#:package Hyprx.Shell;
#:package Hyprx.Rex.Console
#:property NoWarn SA1516
using Hyprx.Exec;
using static Hyprx.Ansi;
using static Hyprx.RexConsole;
using static Hyprx.Shell;

#pragma warning disable

Task("open", () =>
{
    var index = Argv.IndexOf("--name");
    if (index == -1)
    {
        index = Argv.IndexOf("-n");
    }

    if (index == -1)
    {
        Console.WriteLine("Please specify a project to open");
        return;
    }
    var dir = Argv[index + 1];

    var cwd = Directory.GetCurrentDirectory();
    var gi = Path.Combine(cwd, ".git");
    while (gi is not null && !Directory.Exists(gi))
    {
        cwd = Path.GetDirectoryName(cwd)!;
        gi = Path.Combine(cwd, ".git");
    }

    if (string.IsNullOrEmpty(gi))
    {
        Console.WriteLine("Unable to find root project directory");
        return;
    }

    var map = new Dictionary<string, string>()
    {
        ["pulumi"] = "iac/pulumi",
        ["tf"] = "iac/terraform",
        ["terraform"] = "iac/terraform",
    };

    if (map.TryGetValue(dir, out var newDir))
    {
        dir = Path.Combine(cwd, newDir);
    }
    else
    {
        dir = Path.Combine(cwd, dir);
    }

    Echo($"Opening {Blue(dir)}");
    Run($"code {dir}");
});

var ec = await RunTasksAsync(args);
return ec;