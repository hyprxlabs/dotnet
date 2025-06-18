#:package Hyprx.Core
#:package Hyprx.Exec
#:package Hyprx.Shell;
#:package Hyprx.Rex.Console

using Hyprx.Exec;
using static Hyprx.RexConsole;
using static Hyprx.Shell;

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
    var baseName = $"Hyprx.{name}";

    if (Path.Exists($"./{name}/src/{baseName}.csproj"))
    {
        Console.WriteLine($"Project {name} already exists");
        return;
    }

    Run($"""
        dotnet new hxlib -n {baseName} -o ./{name}/src \
        --use-framework-prop Fx --changelog --use-license-path --use-icon-path  --unsafe --cls
    """);

    Run($"""
        dotnet new hxtest -n {baseName}.Tests -o ./{name}/test \
        --use-framework-prop Fx
    """);

    Run($"dotnet new sln -n {baseName} -o ./{name}");
    Run($"dotnet sln . add ./{name}/src");
    Run($"dotnet sln . add ./{name}/test");

    Run($"dotnet sln ./{name}/{baseName}.sln add ./{name}/src");
    Run($"dotnet sln ./{name}/{baseName}.sln add ./{name}/test");
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

var ec = await RunTasksAsync(args);
return ec;