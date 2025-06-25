using System.CommandLine;

using Hyprx.Exec;

namespace Hyprx.Rex.Commands;

public static class Handlers
{
    public static Func<ParseResult, int> GenerateAutoAction(string target)
    {
        return (parseResult) =>
        {
            var fileInfo = parseResult.GetValue(Options.File);
            var file = fileInfo?.FullName;
            if (file is null)
            {
                file = Project.FindProject(Environment.CurrentDirectory);
            }

            if (file is null)
            {
                Console.WriteLine($"No rexfile.cs, .rex/main.cs or .rex/*.csproj found in the {Environment.CurrentDirectory}.");
                return 1;
            }

            if (!Path.IsPathFullyQualified(file))
            {
                file = Path.GetFullPath(file, Environment.CurrentDirectory);
            }

            var timeout = parseResult.GetValue(Options.Timeout);
            var env = parseResult.GetValue(Options.Env);
            var envFiles = parseResult.GetValue(Options.EnvFiles);
            var secretFile = parseResult.GetValue(Options.SecretFiles);
            var verbose = parseResult.GetValue(Options.Verbose);

            var remaining = parseResult.UnmatchedTokens;
            var args = new CommandArgs();
            var ext = Path.GetExtension(file);
            switch (ext)
            {
                case ".cspoj":
                    {
                        args.Add("run", "-v", "quiet");
                        args.Add("--project", file, "--");
                    }

                    break;

                case ".cs":
                    {
                        args.Add("run", "-v", "quiet", file, "--");
                    }

                    break;
                case ".dll":
                    {
                        args.Add(file);
                    }

                    break;

                default:
                    Console.WriteLine($"Unsupported file type: {ext}. Supported types are .csproj, .cs, and .dll.");
                    return 1;
            }

            args.Add(target);
            if (verbose)
                args.Add("-v");
            if (timeout.HasValue)
                args.Add("--timeout", timeout.Value.ToString());

            if (env is not null)
            {
                foreach (var e in env)
                    args.Add("--env", e);
            }

            if (envFiles is not null)
            {
                foreach (var f in envFiles)
                    args.Add("--env-file", f.FullName);
            }

            if (secretFile is not null)
            {
                foreach (var f in secretFile)
                    args.Add($"--secret-file {f.FullName}");
            }

            if (remaining is not null)
                args.AddRange(remaining);

            var dotnet = new Exec.Command("dotnet");
            var output = dotnet.Run(args);

            return output.ExitCode;
        };
    }

    public static Func<ParseResult, int> GenerateTargetAction(Func<Hyprx.Exec.CommandArgs> commandArgsFactory)
    {
        return (parseResult) =>
        {
            var fileInfo = parseResult.GetValue(Options.File);
            var file = fileInfo?.FullName;
            if (file is null)
            {
                file = Project.FindProject(Environment.CurrentDirectory);
            }

            if (file is null)
            {
                Console.WriteLine($"No rexfile.cs, .rex/main.cs or .rex/*.csproj found in the {Environment.CurrentDirectory}.");
                return 1;
            }

            if (!Path.IsPathFullyQualified(file))
            {
                file = Path.GetFullPath(file, Environment.CurrentDirectory);
            }

            var timeout = parseResult.GetValue(Options.Timeout);
            var env = parseResult.GetValue(Options.Env);
            var envFiles = parseResult.GetValue(Options.EnvFiles);
            var secretFile = parseResult.GetValue(Options.SecretFiles);
            var verbose = parseResult.GetValue(Options.Verbose);
            var target = parseResult.GetValue(Options.Target);

            var remaining = parseResult.UnmatchedTokens;
            var args = new CommandArgs();
            var ext = Path.GetExtension(file);
            switch (ext)
            {
                case ".cspoj":
                    {
                        args.Add("run", "-v", "quiet");
                        args.Add("--project", file, "--");
                    }

                    break;

                case ".cs":
                    {
                        args.Add("run", "-v", "quiet", file, "--");
                    }

                    break;
                case ".dll":
                    {
                        args.Add(file);
                    }

                    break;

                default:
                    Console.WriteLine($"Unsupported file type: {ext}. Supported types are .csproj, .cs, and .dll.");
                    return 1;
            }

            args.AddRange(commandArgsFactory());
            if (verbose)
                args.Add("-v");
            if (timeout.HasValue)
                args.Add("--timeout", timeout.Value.ToString());

            if (env is not null)
            {
                foreach (var e in env)
                    args.Add("--env", e);
            }

            if (envFiles is not null)
            {
                foreach (var f in envFiles)
                    args.Add("--env-file", f.FullName);
            }

            if (secretFile is not null)
            {
                foreach (var f in secretFile)
                    args.Add($"--secret-file {f.FullName}");
            }

            if (target is not null)
                args.Add(target);

            if (remaining is not null)
                args.AddRange(remaining);

            var dotnet = new Exec.Command("dotnet");
            var output = dotnet.Run(args);

            return output.ExitCode;
        };
    }

    public static Func<ParseResult, int> GenerateTargetsAction(Func<Hyprx.Exec.CommandArgs> commandArgsFactory)
    {
        return (parseResult) =>
        {
            var fileInfo = parseResult.GetValue(Options.File);
            var file = fileInfo?.FullName;
            if (file is null)
            {
                file = Project.FindProject(Environment.CurrentDirectory);
            }

            if (file is null)
            {
                Console.WriteLine($"No rexfile.cs, .rex/main.cs or .rex/*.csproj found in the {Environment.CurrentDirectory}.");
                return 1;
            }

            if (!Path.IsPathFullyQualified(file))
            {
                file = Path.GetFullPath(file, Environment.CurrentDirectory);
            }

            var timeout = parseResult.GetValue(Options.Timeout);
            var env = parseResult.GetValue(Options.Env);
            var envFiles = parseResult.GetValue(Options.EnvFiles);
            var secretFile = parseResult.GetValue(Options.SecretFiles);
            var verbose = parseResult.GetValue(Options.Verbose);
            var targets = parseResult.GetValue(Options.Targets);

            var remaining = parseResult.UnmatchedTokens;
            var args = new CommandArgs();
            var ext = Path.GetExtension(file);
            switch (ext)
            {
                case ".cspoj":
                    {
                        args.Add("run", "-v", "quiet");
                        args.Add("--project", file, "--");
                    }

                    break;

                case ".cs":
                    {
                        args.Add("run", "-v", "quiet", file, "--");
                    }

                    break;
                case ".dll":
                    {
                        args.Add(file);
                    }

                    break;

                default:
                    Console.WriteLine($"Unsupported file type: {ext}. Supported types are .csproj, .cs, and .dll.");
                    return 1;
            }

            args.AddRange(commandArgsFactory());
            if (verbose)
                args.Add("-v");
            if (timeout.HasValue)
                args.Add("--timeout", timeout.Value.ToString());

            if (env is not null)
            {
                foreach (var e in env)
                    args.Add("--env", e);
            }

            if (envFiles is not null)
            {
                foreach (var f in envFiles)
                    args.Add("--env-file", f.FullName);
            }

            if (secretFile is not null)
            {
                foreach (var f in secretFile)
                    args.Add($"--secret-file {f.FullName}");
            }

            if (targets is not null)
                args.AddRange(targets);

            if (remaining is not null)
                args.AddRange(remaining);

            var dotnet = new Exec.Command("dotnet");
            var output = dotnet.Run(args);

            return output.ExitCode;
        };
    }

    public static Func<ParseResult, int> ListTasksAction()
    {
        return (parseResult) =>
        {
            var fileInfo = parseResult.GetValue(Options.File);
            var file = fileInfo?.FullName;
            if (file is null)
            {
                file = Project.FindProject(Environment.CurrentDirectory);
            }

            if (file is null)
            {
                Console.WriteLine($"No rexfile.cs, .rex/main.cs or .rex/*.csproj found in the {Environment.CurrentDirectory}.");
                return 1;
            }

            if (!Path.IsPathFullyQualified(file))
            {
                file = Path.GetFullPath(file, Environment.CurrentDirectory);
            }

            var verbose = parseResult.GetValue(Options.Verbose);
            var args = new CommandArgs();
            var ext = Path.GetExtension(file);
            switch (ext)
            {
                case ".cspoj":
                    {
                        args.Add("run", "-v", "quiet");
                        args.Add("--project", file, "--");
                    }

                    break;

                case ".cs":
                    {
                        args.Add("run", "-v", "quiet", file, "--");
                    }

                    break;
                case ".dll":
                    args.Add(file);
                    break;

                default:
                    Console.WriteLine($"Unsupported file type: {ext}. Supported types are .csproj, .cs, and .dll.");
                    return 1;
            }

            args.Add("--list", "--tasks");
            if (verbose)
                args.Add("-v");

            var dotnet = new Exec.Command("dotnet");
            var output = dotnet.Run(args);

            return output.ExitCode;
        };
    }

    public static Func<ParseResult, int> ListJobsAction()
    {
        return (parseResult) =>
        {
            var fileInfo = parseResult.GetValue(Options.File);
            var file = fileInfo?.FullName;
            if (file is null)
            {
                file = Project.FindProject(Environment.CurrentDirectory);
            }

            if (file is null)
            {
                Console.WriteLine($"No rexfile.cs, .rex/main.cs or .rex/*.csproj found in the {Environment.CurrentDirectory}.");
                return 1;
            }

            if (!Path.IsPathFullyQualified(file))
            {
                file = Path.GetFullPath(file, Environment.CurrentDirectory);
            }

            var timeout = parseResult.GetValue(Options.Timeout);
            var verbose = parseResult.GetValue(Options.Verbose);
            var args = new CommandArgs();
            var ext = Path.GetExtension(file);
            switch (ext)
            {
                case ".cspoj":
                    {
                        args.Add("run", "-v", "quiet");
                        args.Add("--project", file, "--");
                    }

                    break;

                case ".cs":
                    {
                        args.Add("run", "-v", "quiet", file, "--");
                    }

                    break;
                case ".dll":
                    args.Add(file);
                    break;

                default:
                    Console.WriteLine($"Unsupported file type: {ext}. Supported types are .csproj, .cs, and .dll.");
                    return 1;
            }

            args.Add("--list", "--jobs");
            if (verbose)
                args.Add("-v");

            var dotnet = new Exec.Command("dotnet");
            var output = dotnet.Run(args);

            return output.ExitCode;
        };
    }

    public static Func<ParseResult, int> ListAllAction()
    {
        return (parseResult) =>
        {
            var fileInfo = parseResult.GetValue(Options.File);
            var file = fileInfo?.FullName;
            if (file is null)
            {
                file = Project.FindProject(Environment.CurrentDirectory);
            }

            if (file is null)
            {
                Console.WriteLine($"No rexfile.cs, .rex/main.cs or .rex/*.csproj found in the {Environment.CurrentDirectory}.");
                return 1;
            }

            if (!Path.IsPathFullyQualified(file))
            {
                file = Path.GetFullPath(file, Environment.CurrentDirectory);
            }

            var verbose = parseResult.GetValue(Options.Verbose);
            var args = new CommandArgs();
            var ext = Path.GetExtension(file);
            switch (ext)
            {
                case ".cspoj":
                    {
                        args.Add("run", "-v", "quiet");
                        args.Add("--project", file, "--");
                    }

                    break;

                case ".cs":
                    {
                        args.Add("run", "-v", "quiet", file, "--");
                    }

                    break;
                case ".dll":
                    args.Add(file);
                    break;

                default:
                    Console.WriteLine($"Unsupported file type: {ext}. Supported types are .csproj, .cs, and .dll.");
                    return 1;
            }

            args.Add("--list");
            if (verbose)
                args.Add("-v");

            var dotnet = new Exec.Command("dotnet");
            var output = dotnet.Run(args);

            return output.ExitCode;
        };
    }
}