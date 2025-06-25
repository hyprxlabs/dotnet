using System.CommandLine;

using Hyprx.Exec;
using Hyprx.Extras;
using static Hyprx.Ansi;

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

            var service = parseResult.GetValue(Options.Service);
            var context = parseResult.GetValue(Options.Context);
            var timeout = parseResult.GetValue(Options.Timeout);
            var env = parseResult.GetValue(Options.Env);
            var envFiles = parseResult.GetValue(Options.EnvFiles);
            var secretFile = parseResult.GetValue(Options.SecretFiles);
            var verbose = parseResult.GetValue(Options.Verbose);

            var remaining = new List<string>(parseResult.UnmatchedTokens);
            var args = new CommandArgs();
            var ext = Path.GetExtension(file);
            switch (ext)
            {
                case ".csproj":
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

            args.Add("--auto");

            if (verbose)
                args.Add("-v");
            if (timeout.HasValue)
                args.Add("--timeout", timeout.Value.ToString());

            if (!string.IsNullOrWhiteSpace(context))
                args.Add("--context", context);

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

            var tokens = parseResult.Tokens;
            var defaultToken = tokens.FirstOrDefault(o => o.Type == System.CommandLine.Parsing.TokenType.Argument && o.Value.StartsWith(":"));

            if (defaultToken is not null && service is null && target is not null)
            {
                remaining.Insert(0, target);
                target = defaultToken.Value.Substring(1);
            }

            if (verbose)
                Console.WriteLine($"Target: {target}, Service: {service}, Context: {context}, Remaining: {string.Join(", ", remaining)}, Tokens: {string.Join(", ", parseResult.Tokens)}");

            if (!service.IsNullOrWhiteSpace())
            {
                if (service[0] is '-' or ':' || target![0] is ':' || target.Any(o => o is ':'))
                {
                    if (service[0] is ':')
                        service = service.Substring(1);

                    if (target![0] is ':')
                        target = target.Substring(1);

                    remaining.Insert(0, service);
                    args.Add(target);
                }
                else
                {
                    args.Add($"{service}:{target}");
                }
            }
            else
            {
                if (target![0] is ':')
                    args.Add(target.Substring(1));
                else
                    args.Add(target);
            }

            if (remaining is not null)
                args.AddRange(remaining);

            WriteCommand(file, args, verbose);
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

            var service = parseResult.GetValue(Options.Service);
            var context = parseResult.GetValue(Options.Context);
            var timeout = parseResult.GetValue(Options.Timeout);
            var env = parseResult.GetValue(Options.Env);
            var envFiles = parseResult.GetValue(Options.EnvFiles);
            var secretFile = parseResult.GetValue(Options.SecretFiles);
            var verbose = parseResult.GetValue(Options.Verbose);
            var target = parseResult.GetValue(Options.Target) ?? "default";

            var remaining = new List<string>(parseResult.UnmatchedTokens);
            var args = new CommandArgs();
            var ext = Path.GetExtension(file);
            switch (ext)
            {
                case ".csproj":
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

            if (!string.IsNullOrWhiteSpace(context))
                args.Add("--context", context);

            if (!service.IsNullOrWhiteSpace())
            {
                if (service[0] is '-' or ':' || target[0] is ':' || target.Any(o => o is ':'))
                {
                    if (service[0] is ':')
                        service = service.Substring(1);

                    if (target[0] is ':')
                        target = target.Substring(1);

                    remaining.Insert(0, service);
                    args.Add(target);
                }
                else
                {
                    args.Add($"{service}:{target}");
                }
            }
            else
            {
                if (target[0] is ':')
                    args.Add(target.Substring(1));
                else
                    args.Add(target);
            }

            if (remaining is not null)
                args.AddRange(remaining);

            WriteCommand(file, args, verbose);
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

            var context = parseResult.GetValue(Options.Context);
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
                case ".csproj":
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

            if (verbose)
                Console.WriteLine($"Target: {string.Join(",", targets ?? [])}, Context: {context}, Remaining: {string.Join(", ", remaining)}");

            args.AddRange(commandArgsFactory());
            if (verbose)
                args.Add("-v");
            if (timeout.HasValue)
                args.Add("--timeout", timeout.Value.ToString());

            if (!string.IsNullOrWhiteSpace(context))
                args.Add("--context", context);

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
                args.AddRange(targets.Select(t => t.Trim(':')));

            if (remaining is not null)
                args.AddRange(remaining);

            WriteCommand(file, args, verbose);
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
                case ".csproj":
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
                case ".csproj":
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
                case ".csproj":
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

    public static Func<ParseResult, int> ListNamespacesAction()
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
                case ".csproj":
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

            args.Add("--list-namespaces");
            if (verbose)
                args.Add("-v");

            var dotnet = new Exec.Command("dotnet");
            var output = dotnet.Run(args);

            return output.ExitCode;
        };
    }

    public static Func<ParseResult, int> ListServicesAction()
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
                case ".csproj":
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

            args.Add("--list-services");
            if (verbose)
                args.Add("-v");

            var dotnet = new Exec.Command("dotnet");
            var output = dotnet.Run(args);

            return output.ExitCode;
        };
    }

    private static void WriteCommand(string file, CommandArgs args, bool verbose)
    {
        if (!verbose)
            return;

        Console.Write(Blue("dotnet"));
        Console.Write(" ");

        static string Orange(string text)
        {
            if (AnsiSettings.Current.Mode == AnsiMode.None)
                return text;

            if (AnsiSettings.Current.Mode == AnsiMode.TwentyFourBit)
                return Rgb(0xF28C28, text);
            else if (AnsiSettings.Current.Mode == AnsiMode.EightBit)
                return Rgb8(208, text);
            else
                return Yellow(text);
        }

        foreach (var arg in args)
        {
            Console.Write(" ");
            if (arg.StartsWith("-"))
            {
                Console.Write(Orange(arg));
            }
            else if (arg.Any(char.IsWhiteSpace))
            {
                Console.Write(Magenta($"\"{arg}\""));
            }
            else
            {
                Console.Write(arg);
            }
        }

        Console.WriteLine();
    }
}