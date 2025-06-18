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

Task("build", () =>
{
    Run("dotnet build");
});

Task("dev:setup", () =>
{
    Run("dotnet new install xunit.v3.templates");
});

Task("clean", () =>
{
    Run("dotnet clean");
});

Task("tpl", (ctx) =>
{
    var args = new List<string>(Argv);
    var remove = args.IndexOf("--remove") > -1;

    var dir = Directory.GetCurrentDirectory();
    string? gi = Path.Combine(dir, ".git");
    while (gi is not null && !Directory.Exists(gi))
    {
        dir = Path.GetDirectoryName(dir!);
        gi = Path.Combine(dir!, ".git");
    }

    if (string.IsNullOrEmpty(gi))
    {
        Console.WriteLine("Unable to find root directory");
        return;
    }

    var tplDir = Path.Combine(dir!, "tpl");
    if (remove)
    {
        Run($"dotnet new uninstall {Path.Join(tplDir, "hxlib")}");
        Run($"dotnet new uninstall {Path.Join(tplDir, "hxtest")}");
        return;
    }

    Run($"dotnet new install --force {Path.Join(tplDir, "hxlib")}");
    Run($"dotnet new install --force {Path.Join(tplDir, "hxtest")}");
});

Task("add", (ctx) =>
{
    var args = new List<string>(Argv);
    Echo(string.Join(" ", ctx.Args));
    var index = args.IndexOf("--name");
    if (index == -1)
    {
        Console.WriteLine("Please provide a name using --name");
        return;
    }

    var name = args[index + 1];
    var baseName = $"Hyprx.Pulumi.{name}";

    if (Path.Exists($"./{name}/src/{baseName}.csproj"))
    {
        Console.WriteLine($"Project {name} already exists");
        return;
    }

    Run($"""
        dotnet new hxlib -n {baseName} -o ./{name} \
        --use-framework-prop Fx --changelog --use-license-path --use-icon-path  --unsafe --cls
    """);

    Run($"dotnet sln . add ./{name}");
});



Task("rm", (ctx) =>
{
    var args = new List<string>(Argv);
    Echo(string.Join(" ", ctx.Args));
    var index = args.IndexOf("--name");
    if (index == -1)
    {
        Console.WriteLine("Please provide a name using --name");
        return;
    }

    var delete = args.IndexOf("--delete") > -1;

    var name = args[index + 1];
    var baseName = $"Hyprx.{name}";

    Run($"dotnet sln . remove ./{name}/src");
    Run($"dotnet sln . remove ./{name}/test");

    Run($"dotnet sln ./{name}/{baseName}.sln remove ./src");
    Run($"dotnet sln ./{name}/{baseName}.sln remove ./test");

    if (delete)
    {
        Fs.RemoveDir($"./{name}", true);
    }
});


Task("code", () =>
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