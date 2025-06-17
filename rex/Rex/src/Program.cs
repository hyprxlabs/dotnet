// See https://aka.ms/new-console-template for more information
#pragma warning disable SA1516
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;

using Rex;

var cwd = Environment.GetEnvironmentVariable("REX_PWD");
if (!string.IsNullOrEmpty(cwd))
{
    Environment.CurrentDirectory = cwd;
}

var fileOption = new Option<FileInfo?>(["--file", "-f"], () => null, "The rexfile.cs or tasks.cs file to use. If not specified, it will look for rexfile.cs or tasks.cs in the current directory.");
var timeoutOption = new Option<int?>(["--timeout", "-t"], () => null, "The timeout in seconds for each task. If not specified, there is no timeout.");
var enviroment = new Option<string[]>(["--env", "-e"], () => [], "The environment to use. This will set the REX_ENV environment variable.");
var environmentFileOption = new Option<FileInfo[]>(["--env-file"], () => [], "The environment file to use. This will set the REX_ENV_FILE environment variable.");
var secretFileOption = new Option<FileInfo[]>(["--secret-file"], () => [], "The secret file to use. This will set the REX_SECRET_FILE environment variable.");
var verboseOption = new Option<bool>(["--verbose", "-v"], () => false, "Enable verbose logging.");
var targetsArgument = new Argument<string[]>("targets", () => Array.Empty<string>(), "The targets to run.");
var listCommand = new Command("list", "List all available tasks.")
{
    fileOption,
}
.WithHandler((ctx) =>
{
    var fileInfo = ctx.ParseResult.GetValueForOption(fileOption);
    var file = fileInfo?.FullName;
    if (file is null)
    {
        file = Project.FindProject(Environment.CurrentDirectory);
    }
    else
    {
        if (!Path.IsPathFullyQualified(file))
        {
            file = Path.GetFullPath(file, Environment.CurrentDirectory);
        }
    }

    if (file is null)
    {
        Console.WriteLine($"No rexfile.cs, .rex/main.cs or .rex/*.csproj found in the {Environment.CurrentDirectory}.");
        return 1;
    }

    if (file.EndsWith(".csproj"))
    {
        Process.Start("dotnet", $"run -v quiet --project {file} --list")?.WaitForExit();
        return 0;
    }

    if (file.EndsWith(".cs"))
    {
        Process.Start("dotnet", $"run -v quiet {file} --list")?.WaitForExit();
        return 0;
    }

    if (file.EndsWith(".dll"))
    {
        Process.Start("dotnet", $"{file} --list")?.WaitForExit();
        return 0;
    }

    return 1;
});

Func<InvocationContext, int> runTask = (ctx) =>
{
    var fileInfo = ctx.ParseResult.GetValueForOption(fileOption);
    var file = fileInfo?.FullName;
    if (file is null)
    {
        file = Project.FindProject(Environment.CurrentDirectory);
    }
    else
    {
        if (!Path.IsPathFullyQualified(file))
        {
            file = Path.GetFullPath(file, Environment.CurrentDirectory);
        }
    }

    if (file is null)
    {
        Console.WriteLine($"No rexfile.cs, .rex/main.cs or .rex/*.csproj found in the {Environment.CurrentDirectory}.");
        return 1;
    }

    var timeout = ctx.ParseResult.GetValueForOption(timeoutOption);
    var env = ctx.ParseResult.GetValueForOption(enviroment);
    var envFiles = ctx.ParseResult.GetValueForOption(environmentFileOption);
    var secretFile = ctx.ParseResult.GetValueForOption(secretFileOption);
    var verbose = ctx.ParseResult.GetValueForOption(verboseOption);
    var targets = ctx.ParseResult.GetValueForArgument<string[]>(targetsArgument);

    var remaining = ctx.ParseResult.UnparsedTokens;

    var args = new List<string>()
    {
        "--task",
    };
    if (verbose)
        args.Add("-v");
    if (timeout.HasValue)
        args.Add($"-t {timeout.Value}");

    if (env is not null)
    {
        foreach (var e in env)
            args.Add($"-e {e}");
    }

    if (envFiles is not null)
    {
        foreach (var f in envFiles)
            args.Add($"--env-file {f.FullName}");
    }

    if (secretFile is not null)
    {
        foreach (var f in secretFile)
            args.Add($"--secret-file {f.FullName}");
    }

    args.AddRange(targets);

    if (remaining is not null)
        args.AddRange(remaining);

    if (file.EndsWith(".csproj"))
    {
        var p = Process.Start("dotnet", $"run -v quiet --project {file} -- {string.Join(' ', args)}");
        p?.WaitForExit();
        return p?.ExitCode ?? 1;
    }

    if (file.EndsWith(".cs"))
    {
        var p = Process.Start("dotnet", $"run -v quiet {file} -- {string.Join(' ', args)}");
        p?.WaitForExit();
        return p?.ExitCode ?? 1;
    }

    if (file.EndsWith(".dll"))
    {
        var p = Process.Start("dotnet", $"{file} -- {string.Join(' ', args)}");
        p?.WaitForExit();
        return p?.ExitCode ?? 1;
    }

    return 1;
};

var taskCommand = new Command("tasks", "Run a specific task.")
{
    fileOption,
    timeoutOption,
    enviroment,
    environmentFileOption,
    secretFileOption,
    verboseOption,
    targetsArgument,
}.WithHandler(runTask);

var taskRunCommand = new Command("run", "Run a specific task.")
{
    fileOption,
    timeoutOption,
    enviroment,
    environmentFileOption,
    secretFileOption,
    verboseOption,
    targetsArgument,
}.WithHandler(runTask);

taskCommand.AddCommand(taskRunCommand);

Func<InvocationContext, int> runJobs = (ctx) =>
{
    var fileInfo = ctx.ParseResult.GetValueForOption(fileOption);
    var file = fileInfo?.FullName;
    if (file is null)
    {
        file = Project.FindProject(Environment.CurrentDirectory);
    }
    else
    {
        if (!Path.IsPathFullyQualified(file))
        {
            file = Path.GetFullPath(file, Environment.CurrentDirectory);
        }
    }

    if (file is null)
    {
        Console.WriteLine($"No rexfile.cs, .rex/main.cs or .rex/*.csproj found in the {Environment.CurrentDirectory}.");
        return 1;
    }

    var timeout = ctx.ParseResult.GetValueForOption(timeoutOption);
    var env = ctx.ParseResult.GetValueForOption(enviroment);
    var envFiles = ctx.ParseResult.GetValueForOption(environmentFileOption);
    var secretFile = ctx.ParseResult.GetValueForOption(secretFileOption);
    var verbose = ctx.ParseResult.GetValueForOption(verboseOption);
    var targets = ctx.ParseResult.GetValueForArgument<string[]>(targetsArgument);

    var remaining = ctx.ParseResult.UnparsedTokens;

    var args = new List<string>()
    {
        "--job",
    };
    if (verbose)
        args.Add("-v");
    if (timeout.HasValue)
        args.Add($"-t {timeout.Value}");
    if (env is not null)
    {
        foreach (var e in env)
            args.Add($"-e {e}");
    }

    if (envFiles is not null)
    {
        foreach (var f in envFiles)
            args.Add($"--env-file {f.FullName}");
    }

    if (secretFile is not null)
    {
        foreach (var f in secretFile)
            args.Add($"--secret-file {f.FullName}");
    }

    args.AddRange(targets);

    if (remaining is not null)
        args.AddRange(remaining);

    if (file.EndsWith(".csproj"))
    {
        var p = Process.Start("dotnet", $"run -v quiet --project {file} -- {string.Join(' ', args)}");
        p?.WaitForExit();
        return p?.ExitCode ?? 1;
    }

    if (file.EndsWith(".cs"))
    {
        var p = Process.Start("dotnet", $"run -v quiet {file} -- {string.Join(' ', args)}");
        p?.WaitForExit();
        return p?.ExitCode ?? 1;
    }

    if (file.EndsWith(".dll"))
    {
        var p = Process.Start("dotnet", $"{file} -- {string.Join(' ', args)}");
        p?.WaitForExit();
        return p?.ExitCode ?? 1;
    }

    return 1;
};

var jobsCommand = new Command("jobs", "Manage background jobs.")
{
    fileOption,
    timeoutOption,
    enviroment,
    environmentFileOption,
    secretFileOption,
    verboseOption,
    targetsArgument,
}
.WithHandler(runJobs);

var jobsRunCommand = new Command("run", "Run a specific job.")
{
    fileOption,
    timeoutOption,
    enviroment,
    environmentFileOption,
    secretFileOption,
    verboseOption,
    targetsArgument,
}.WithHandler(runJobs);
jobsCommand.AddCommand(jobsRunCommand);

var deployCommand = new Command("deploy", "Deploy a specific deployment.")
{
    fileOption,
    timeoutOption,
    enviroment,
    environmentFileOption,
    secretFileOption,
    verboseOption,
    targetsArgument,
}.WithHandler((ctx) =>
{
    var fileInfo = ctx.ParseResult.GetValueForOption(fileOption);
    var file = fileInfo?.FullName;
    if (file is null)
    {
        file = Project.FindProject(Environment.CurrentDirectory);
    }
    else
    {
        if (!Path.IsPathFullyQualified(file))
        {
            file = Path.GetFullPath(file, Environment.CurrentDirectory);
        }
    }

    if (file is null)
    {
        Console.WriteLine($"No rexfile.cs, .rex/main.cs or .rex/*.csproj found in the {Environment.CurrentDirectory}.");
        return 1;
    }

    var timeout = ctx.ParseResult.GetValueForOption(timeoutOption);
    var env = ctx.ParseResult.GetValueForOption(enviroment);
    var envFiles = ctx.ParseResult.GetValueForOption(environmentFileOption);
    var secretFile = ctx.ParseResult.GetValueForOption(secretFileOption);
    var verbose = ctx.ParseResult.GetValueForOption(verboseOption);
    var targets = ctx.ParseResult.GetValueForArgument<string[]>(targetsArgument);

    var remaining = ctx.ParseResult.UnparsedTokens;

    var args = new List<string>()
    {
        "--deploy",
    };
    if (verbose)
        args.Add("-v");
    if (timeout.HasValue)
        args.Add($"-t {timeout.Value}");
    if (env is not null)
    {
        foreach (var e in env)
            args.Add($"-e {e}");
    }

    if (envFiles is not null)
    {
        foreach (var f in envFiles)
            args.Add($"--env-file {f.FullName}");
    }

    if (secretFile is not null)
    {
        foreach (var f in secretFile)
            args.Add($"--secret-file {f.FullName}");
    }

    args.AddRange(targets);

    if (remaining is not null)
        args.AddRange(remaining);

    if (file.EndsWith(".csproj"))
    {
        var p = Process.Start("dotnet", $"run -v quiet --project {file} -- {string.Join(' ', args)}");
        p?.WaitForExit();
        return p?.ExitCode ?? 1;
    }

    if (file.EndsWith(".cs"))
    {
        var p = Process.Start("dotnet", $"run -v quiet {file} -- {string.Join(' ', args)}");
        p?.WaitForExit();
        return p?.ExitCode ?? 1;
    }

    if (file.EndsWith(".dll"))
    {
        var p = Process.Start("dotnet", $"{file} -- {string.Join(' ', args)}");
        p?.WaitForExit();
        return p?.ExitCode ?? 1;
    }

    return 1;
});

var rollbackCommand = new Command("rollback", "Rollback a specific deployment.")
{
    fileOption,
    timeoutOption,
    enviroment,
    environmentFileOption,
    secretFileOption,
    verboseOption,
    targetsArgument,
}.WithHandler((ctx) =>
{
    var fileInfo = ctx.ParseResult.GetValueForOption(fileOption);
    var file = fileInfo?.FullName;
    if (file is null)
    {
        file = Project.FindProject(Environment.CurrentDirectory);
    }
    else
    {
        if (!Path.IsPathFullyQualified(file))
        {
            file = Path.GetFullPath(file, Environment.CurrentDirectory);
        }
    }

    if (file is null)
    {
        Console.WriteLine($"No rexfile.cs, .rex/main.cs or .rex/*.csproj found in the {Environment.CurrentDirectory}.");
        return 1;
    }

    var timeout = ctx.ParseResult.GetValueForOption(timeoutOption);
    var env = ctx.ParseResult.GetValueForOption(enviroment);
    var envFiles = ctx.ParseResult.GetValueForOption(environmentFileOption);
    var secretFile = ctx.ParseResult.GetValueForOption(secretFileOption);
    var verbose = ctx.ParseResult.GetValueForOption(verboseOption);
    var targets = ctx.ParseResult.GetValueForArgument<string[]>(targetsArgument);

    var remaining = ctx.ParseResult.UnparsedTokens;

    var args = new List<string>()
    {
        "--rollback",
    };
    if (verbose)
        args.Add("-v");
    if (timeout.HasValue)
        args.Add($"-t {timeout.Value}");
    if (env is not null)
    {
        foreach (var e in env)
            args.Add($"-e {e}");
    }

    if (envFiles is not null)
    {
        foreach (var f in envFiles)
            args.Add($"--env-file {f.FullName}");
    }

    if (secretFile is not null)
    {
        foreach (var f in secretFile)
            args.Add($"--secret-file {f.FullName}");
    }

    args.AddRange(targets);

    if (remaining is not null)
        args.AddRange(remaining);

    if (file.EndsWith(".csproj"))
    {
        var p = Process.Start("dotnet", $"run -v quiet --project {file} -- {string.Join(' ', args)}");
        p?.WaitForExit();
        return p?.ExitCode ?? 1;
    }

    if (file.EndsWith(".cs"))
    {
        var p = Process.Start("dotnet", $"run -v quiet {file} -- {string.Join(' ', args)}");
        p?.WaitForExit();
        return p?.ExitCode ?? 1;
    }

    if (file.EndsWith(".dll"))
    {
        var p = Process.Start("dotnet", $"{file} -- {string.Join(' ', args)}");
        p?.WaitForExit();
        return p?.ExitCode ?? 1;
    }

    return 1;
});

var destroyCommand = new Command("destroy", "Destroy a specific deployment.")
{
    fileOption,
    timeoutOption,
    enviroment,
    environmentFileOption,
    secretFileOption,
    verboseOption,
    targetsArgument,
}.WithHandler((ctx) =>
{
    var fileInfo = ctx.ParseResult.GetValueForOption(fileOption);
    var file = fileInfo?.FullName;
    if (file is null)
    {
        file = Project.FindProject(Environment.CurrentDirectory);
    }
    else
    {
        if (!Path.IsPathFullyQualified(file))
        {
            file = Path.GetFullPath(file, Environment.CurrentDirectory);
        }
    }

    if (file is null)
    {
        Console.WriteLine($"No rexfile.cs, .rex/main.cs or .rex/*.csproj found in the {Environment.CurrentDirectory}.");
        return 1;
    }

    var timeout = ctx.ParseResult.GetValueForOption(timeoutOption);
    var env = ctx.ParseResult.GetValueForOption(enviroment);
    var envFiles = ctx.ParseResult.GetValueForOption(environmentFileOption);
    var secretFile = ctx.ParseResult.GetValueForOption(secretFileOption);
    var verbose = ctx.ParseResult.GetValueForOption(verboseOption);
    var targets = ctx.ParseResult.GetValueForArgument<string[]>(targetsArgument);

    var remaining = ctx.ParseResult.UnparsedTokens;

    var args = new List<string>()
    {
        "--destroy",
    };
    if (verbose)
        args.Add("-v");
    if (timeout.HasValue)
        args.Add($"-t {timeout.Value}");

    if (env is not null)
    {
        foreach (var e in env)
            args.Add($"-e {e}");
    }

    if (envFiles is not null)
    {
        foreach (var f in envFiles)
            args.Add($"--env-file {f.FullName}");
    }

    if (secretFile is not null)
    {
        foreach (var f in secretFile)
            args.Add($"--secret-file {f.FullName}");
    }

    args.AddRange(targets);

    if (remaining is not null)
        args.AddRange(remaining);

    if (file.EndsWith(".csproj"))
    {
        var p = Process.Start("dotnet", $"run -v quiet --project {file} -- {string.Join(' ', args)}");
        p?.WaitForExit();
        return p?.ExitCode ?? 1;
    }

    if (file.EndsWith(".cs"))
    {
        var p = Process.Start("dotnet", $"run -v quiet {file} -- {string.Join(' ', args)}");
        p?.WaitForExit();
        return p?.ExitCode ?? 1;
    }

    if (file.EndsWith(".dll"))
    {
        var p = Process.Start("dotnet", $"{file} -- {string.Join(' ', args)}");
        p?.WaitForExit();
        return p?.ExitCode ?? 1;
    }

    return 1;
});

var root = new RootCommand("rex")
{
    listCommand,
    taskCommand,
    jobsCommand,
    deployCommand,
    rollbackCommand,
    destroyCommand,
    fileOption,
    timeoutOption,
    enviroment,
    environmentFileOption,
    secretFileOption,
    verboseOption,
    targetsArgument,
};

root.WithHandler(ctx =>
{
    var fileInfo = ctx.ParseResult.GetValueForOption(fileOption);
    var file = fileInfo?.FullName;
    if (file is null)
    {
        file = Project.FindProject(Environment.CurrentDirectory);
    }
    else
    {
        if (!Path.IsPathFullyQualified(file))
        {
            file = Path.GetFullPath(file, Environment.CurrentDirectory);
        }
    }

    if (file is null)
    {
        Console.WriteLine($"No rexfile.cs, .rex/main.cs or .rex/*.csproj found in the {Environment.CurrentDirectory}.");
        return 1;
    }

    var timeout = ctx.ParseResult.GetValueForOption(timeoutOption);
    var env = ctx.ParseResult.GetValueForOption(enviroment);
    var envFiles = ctx.ParseResult.GetValueForOption(environmentFileOption);
    var secretFile = ctx.ParseResult.GetValueForOption(secretFileOption);
    var verbose = ctx.ParseResult.GetValueForOption(verboseOption);
    var targets = ctx.ParseResult.GetValueForArgument<string[]>(targetsArgument);

    var remaining = ctx.ParseResult.UnparsedTokens;

    var args = new List<string>()
    {
    };
    if (verbose)
        args.Add("-v");
    if (timeout.HasValue)
        args.Add($"-t {timeout.Value}");

    if (env is not null)
    {
        foreach (var e in env)
            args.Add($"-e {e}");
    }

    if (envFiles is not null)
    {
        foreach (var f in envFiles)
            args.Add($"--env-file {f.FullName}");
    }

    if (secretFile is not null)
    {
        foreach (var f in secretFile)
            args.Add($"--secret-file {f.FullName}");
    }

    args.AddRange(targets);

    if (remaining is not null)
        args.AddRange(remaining);

    if (file.EndsWith(".csproj"))
    {
        Process.Start("dotnet", $"run -v quiet --project {file} {string.Join(" ", args)}")?.WaitForExit();
        return 0;
    }

    if (file.EndsWith(".cs"))
    {
        Process.Start("dotnet", $"run -v quiet {file}  {string.Join(" ", args)}")?.WaitForExit();
        return 0;
    }

    if (file.EndsWith(".dll"))
    {
        Process.Start("dotnet", $"{file}  {string.Join(" ", args)}")?.WaitForExit();
        return 0;
    }

    return 1;
});

await root.InvokeAsync(args);
