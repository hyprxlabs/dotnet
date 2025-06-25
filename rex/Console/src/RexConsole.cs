using System.Reflection.Metadata;
using System.Runtime.InteropServices;

using Hyprx.DotEnv.Documents;
using Hyprx.Extras;
using Hyprx.Lodi;
using Hyprx.Results;

using Hyprx.Rex.Collections;
using Hyprx.Rex.Deployments;
using Hyprx.Rex.Execution;
using Hyprx.Rex.Jobs;
using Hyprx.Rex.Messaging;
using Hyprx.Rex.Tasks;
using Hyprx.Secrets;

using static Hyprx.Ansi;

namespace Hyprx;

public static class RexConsole
{
    public static TaskMap Tasks { get; } = TaskMap.Global;

    public static JobMap Jobs { get; } = JobMap.Global;

    public static DeploymentMap Deployments { get; } = DeploymentMap.Default;

    public static DeploymentBuilder Deployment(CodeDeployment deployment, Action<DeploymentBuilder>? configure = null)
    {
        Deployments[deployment.Id] = deployment;
        var builder = new DeploymentBuilder(deployment, Tasks);
        configure?.Invoke(builder);
        return builder;
    }

    public static DeploymentBuilder Deployment(string id, Func<DeploymentContext, CancellationToken, Task<Result<Outputs>>> deployAsync, Action<DeploymentBuilder>? configure = null)
    {
        var deployment = new DelegateDeployment(id, deployAsync);
        Deployments[id] = deployment;
        var builder = new DeploymentBuilder(deployment, Tasks);
        configure?.Invoke(builder);
        return builder;
    }

    public static DeploymentBuilder Deployment(string id, Func<DeploymentContext, Task<Result<Outputs>>> run, Action<DeploymentBuilder>? configure = null)
    {
        var deployment = new DelegateDeployment(id, run);
        Deployments[id] = deployment;
        var builder = new DeploymentBuilder(deployment, Tasks);
        configure?.Invoke(builder);
        return builder;
    }

    public static DeploymentBuilder Deployment(string id, Func<DeploymentContext, CancellationToken, Outputs> run, Action<DeploymentBuilder>? configure = null)
    {
        var deployment = new DelegateDeployment(id, run);
        Deployments[id] = deployment;
        var builder = new DeploymentBuilder(deployment, Tasks);
        configure?.Invoke(builder);
        return builder;
    }

    public static DeploymentBuilder Deployment(string id, Func<DeploymentContext, CancellationToken, Task<Outputs>> run, Action<DeploymentBuilder>? configure = null)
    {
        var deployment = new DelegateDeployment(id, run);
        Deployments[id] = deployment;
        var builder = new DeploymentBuilder(deployment, Tasks);
        configure?.Invoke(builder);
        return builder;
    }

    public static DeploymentBuilder Deployment(string id, Action<DeploymentContext> run, Action<DeploymentBuilder>? configure = null)
    {
        var deployment = new DelegateDeployment(id, run);
        Deployments[id] = deployment;
        var builder = new DeploymentBuilder(deployment, Tasks);
        configure?.Invoke(builder);
        return builder;
    }

    public static DeploymentBuilder Deployment(string id, Action run, Action<DeploymentBuilder>? configure = null)
    {
        var deployment = new DelegateDeployment(id, run);
        Deployments[id] = deployment;
        var builder = new DeploymentBuilder(deployment, Tasks);
        configure?.Invoke(builder);
        return builder;
    }

    public static JobBuilder Job(CodeJob job, Action<JobBuilder>? configure = null)
    {
        Jobs[job.Id] = job;
        var builder = new JobBuilder(job, Tasks);
        configure?.Invoke(builder);
        return builder;
    }

    public static JobBuilder Job(string id, Action<JobBuilder>? configure = null)
    {
        var job = new CodeJob(id);
        Jobs[id] = job;
        var builder = new JobBuilder(job, Tasks);
        configure?.Invoke(builder);
        return builder;
    }

    public static TaskBuilder Task(CodeTask task)
    {
        Tasks[task.Id] = task;
        return new TaskBuilder(task);
    }

    public static TaskBuilder Task(string id, RunTaskAsync run, Action<TaskBuilder>? configure = null)
    {
        var task = new DelegateTask(id, run);
        Tasks[id] = task;
        var builder = new TaskBuilder(task);
        configure?.Invoke(builder);
        return builder;
    }

    public static TaskBuilder Task(string id, string[] needs, RunTaskAsync run, Action<TaskBuilder>? configure = null)
    {
        var task = new DelegateTask(id, run);
        task.Needs = needs;
        Tasks[id] = task;
        var builder = new TaskBuilder(task);
        configure?.Invoke(builder);
        return builder;
    }

    public static TaskBuilder Task(string id, Func<TaskContext, Task<Result<Outputs>>> run, Action<TaskBuilder>? configure = null)
    {
        var task = new DelegateTask(id, run);
        Tasks[id] = task;
        var builder = new TaskBuilder(task);
        configure?.Invoke(builder);
        return builder;
    }

    public static TaskBuilder Task(string id, Func<TaskContext, CancellationToken, Outputs> run, Action<TaskBuilder>? configure = null)
    {
        var task = new DelegateTask(id, run);
        Tasks[id] = task;
        var builder = new TaskBuilder(task);
        configure?.Invoke(builder);
        return builder;
    }

    public static TaskBuilder Task(string id, string[] needs, Func<TaskContext, CancellationToken, Task<Outputs>> run, Action<TaskBuilder>? configure = null)
    {
        var task = new DelegateTask(id, run);
        task.Needs = needs;
        Tasks[id] = task;
        var builder = new TaskBuilder(task);
        configure?.Invoke(builder);
        return builder;
    }

    public static TaskBuilder Task(string id, Action<TaskContext> run, Action<TaskBuilder>? configure = null)
    {
        var task = new DelegateTask(id, run);
        Tasks[id] = task;
        var builder = new TaskBuilder(task);
        configure?.Invoke(builder);
        return builder;
    }

    public static TaskBuilder Task(string id, Action run, Action<TaskBuilder>? configure = null)
    {
        var task = new DelegateTask(id, run);
        Tasks[id] = task;
        var builder = new TaskBuilder(task);
        configure?.Invoke(builder);
        return builder;
    }

    public static Task<int> RunTasksAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var options = new RexConsoleOptions();
        var settings = new RexConsoleSettings()
        {
            Tasks = Tasks,
            Jobs = Jobs,
        };

        return RunTasksAsync(args, options, settings, cancellationToken);
    }

    public static Task<int> RunTasksAsync(string[] args, RexConsoleSettings settings, CancellationToken cancellationToken = default)
    {
        var options = new RexConsoleOptions();
        return RunTasksAsync(args, options, settings, cancellationToken);
    }

    public static async Task<int> RunTasksAsync(string[] args, RexConsoleOptions options, RexConsoleSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        ParseArgs(args, options);

        if (settings.Services is null)
        {
            var sp = new LodiServiceProvider();
            sp.RegisterSingleton(_ => new TaskPipeline());
            sp.RegisterSingleton(_ => new SequentialTasksPipeline());
            sp.RegisterSingleton(_ => new JobPipeline());
            sp.RegisterSingleton(_ => new SequentialJobsPipeline());
            sp.RegisterSingleton(_ => new ExecutionDefaults { Timeout = 60 * 60 * 1000 }); // Default timeout is 1 hour
            sp.RegisterSingleton(_ => settings.SecretMasker ?? new SecretMasker());
            sp.RegisterSingleton(_ => new DeploymentPipeline());
            sp.RegisterSingleton(_ =>
            {
                var bus = new ConsoleMessageBus();

                if (options.Debug)
                    bus.SetMinimumLevel(DiagnosticLevel.Debug);
                else if (options.Verbose)
                    bus.SetMinimumLevel(DiagnosticLevel.Trace);
                else
                    bus.SetMinimumLevel(DiagnosticLevel.Info);

                bus.Subscribe(new ConsoleSink(), "*");

                return (IMessageBus)bus;
            });

            settings.Services = sp;
        }

        var serviceProvider = settings.Services;
        var defaults = serviceProvider.GetService(typeof(ExecutionDefaults)) as ExecutionDefaults;
        if (defaults is not null)
        {
            defaults.Timeout = options.Timeout > 0 ? options.Timeout : 0;
        }

        var masker = serviceProvider.GetService(typeof(ISecretMasker)) as ISecretMasker;
        if (masker is not null)
        {
            foreach (var secret in settings.Secrets)
            {
                masker.Add(secret.Value);
            }
        }

        var env = new StringMap();
        foreach (var kvp in options.Env)
        {
            env[kvp.Key] = kvp.Value;
            Env.Set(kvp.Key, kvp.Value);
        }

        var autoload = Path.Join(Environment.CurrentDirectory, ".env");
        if (File.Exists(autoload) && !options.EnvFiles.Contains(autoload))
        {
            options.EnvFiles.Insert(0, autoload);
        }

        if (options.EnvFiles.Count > 0)
        {
            var doc = DotEnv.DotEnvLoader.Parse(new DotEnv.DotEnvLoadOptions()
            {
                Files = options.EnvFiles,
                Expand = true,
                OverrideEnvironment = true,
            });

            foreach (var node in doc)
            {
                if (node is DotEnvEntry entry)
                {
                    env[entry.Name] = entry.Value;
                    Env.Set(entry.Name, entry.Value);
                }
            }
        }

        if (options.SecretFiles.Count > 0)
        {
            var doc = DotEnv.DotEnvLoader.Parse(new DotEnv.DotEnvLoadOptions()
            {
                Files = options.SecretFiles,
                Expand = true,
                OverrideEnvironment = false,
            });

            foreach (var node in doc)
            {
                if (node is DotEnvEntry entry)
                {
                    settings.Secrets[entry.Name] = entry.Value;
                    masker?.Add(entry.Value);
                    env[entry.Name] = entry.Value;
                    Env.Set(entry.Name, entry.Value);
                }
            }
        }

        var globalTasks = settings.Tasks;
        var globalJobs = settings.Jobs;
        var globalDeployments = settings.Deployments;

        if (globalTasks.Count == 0 && globalJobs.Count == 0)
        {
            Console.WriteLine("No tasks or jobs defined.");
            return 1;
        }

        if (options.Cmd == "auto")
        {
            var first = options.Targets.FirstOrDefault();
            if (first is null)
            {
                Console.WriteLine("No targets specified.");
                return 1;
            }

            if (globalJobs.ContainsKey(first))
            {
                options.Cmd = options.Targets.Length > 1 ? "jobs" : "job";
            }
            else if (globalTasks.ContainsKey(first))
            {
                options.Cmd = options.Targets.Length > 1 ? "tasks" : "task";
            }
            else
            {
                Console.WriteLine($"Target '{first}' not found in tasks or jobs.");
                return 1;
            }
        }

        var runContext = new RunContext(options.Context, serviceProvider, options.Args);

        switch (options.Cmd)
        {
            case "tasks":
                {
                    var ctx = new SequentialTasksPipelineContext(runContext, options.Targets, settings.Tasks);
                    var pipeline = serviceProvider.GetService(typeof(SequentialTasksPipeline)) as SequentialTasksPipeline ?? new SequentialTasksPipeline();
                    var summary = await pipeline!.RunAsync(ctx, cancellationToken);
                    if (summary.Exception is not null)
                    {
                        Console.WriteLine($"Task execution failed: {summary.Exception}");
                        return 1;
                    }

                    if (summary.Status == RunStatus.Failed)
                    {
                        Console.WriteLine("One or more tasks failed.");
                        return 1;
                    }

                    return 0;
                }

            case "task":
                {
                    settings.Tasks.TryGetValue(options.Targets[0], out var task);
                    if (task is null)
                    {
                        Console.WriteLine($"Task '{Blue(options.Targets[0])}' is '{Red("not found.")}'");
                        return 1;
                    }

                    var ctx = new TaskPipelineContext(new BusContext(runContext), task, new CodeTaskData(task.Id));
                    var pipeline = serviceProvider.GetService(typeof(TaskPipeline)) as TaskPipeline ?? new TaskPipeline();
                    var summary = await pipeline!.RunAsync(ctx, cancellationToken);
                    switch (summary.Status)
                    {
                        case RunStatus.Skipped:
                            return 0;

                        case RunStatus.Cancelled:
                            Console.WriteLine($"Task '{Blue(task.Id)}' was cancelled.");
                            return 1;

                        case RunStatus.Failed:
                            Console.WriteLine(Red($"Task '{Blue(task.Id)}' failed: {summary.Error?.ToString() ?? "Unknown error"}"));
                            return 1;

                        default:
                            return 0;
                    }
                }

            case "jobs":
                {
                    var ctx = new JobsPipelineContext(runContext, options.Targets, settings.Jobs);
                    var pipeline = serviceProvider.GetService(typeof(SequentialJobsPipeline)) as SequentialJobsPipeline;
                    var summary = await pipeline!.RunAsync(ctx, cancellationToken);
                    if (summary.Exception is not null)
                    {
                        Console.WriteLine($"Job execution failed: {summary.Exception.Message} \n{summary.Exception.StackTrace}");
                        return 1;
                    }

                    if (summary.Status == RunStatus.Failed)
                    {
                        Console.WriteLine("One or more jobs failed.");
                        return 1;
                    }

                    return 0;
                }

            case "job":
                {
                    settings.Jobs.TryGetValue(options.Targets[0], out var job);
                    if (job is null)
                    {
                        Console.WriteLine($"Job '{Blue(options.Targets[0])}' is '{Red("not found.")}'");
                        return 1;
                    }

                    var ctx = new JobPipelineContext(new BusContext(runContext), job, new CodeJobData() { Id = job.Id });
                    var pipeline = serviceProvider.GetService(typeof(JobPipeline)) as JobPipeline ?? new JobPipeline();
                    var summary = await pipeline!.RunAsync(ctx, cancellationToken);
                    switch (summary.Status)
                    {
                        case RunStatus.Skipped:
                            return 0;

                        case RunStatus.Cancelled:
                            Console.WriteLine($"Job '{Blue(job.Id)}' was cancelled.");
                            return 1;

                        case RunStatus.Failed:
                            Console.WriteLine(Red($"Job '{Blue(job.Id)}' failed: {summary.Error?.ToString() ?? "Unknown error"}"));
                            return 1;

                        default:
                            return 0;
                    }
                }

            case "help":
                {
                    var help = """
                    Version: 0.0.0

                    RexConsole is meant to be executed using dotnet-rex command line tool which will
                    call the appropriate arguments. However, you can also run it directly using
                    dotnet run command in the root of the project.

                    Usage: 
                        dotnet run -v quiet rexfile.cs -- [options] <target...> [<remainingArgs>]
                        dotnet run -v quiet rexfile.cs -- --list 
                        dotnet run -v quiet rexfile.cs -- rex --list --tasks
                        dotnet run -v quiet rexfile.cs -- rex --list --jobs
                        dotnet run -v quiet rexfile.cs -- build customArg --custom-option
                        dotnet run -v quiet rexfile.cs -- -e MY_VAR=someValue --env-file .env --task myTask
                        dotnet run -v quiet rexfile.cs -- -v --job myJob 
                        dotnet run -v quiet rexfile.cs -- -v --jobs myJob1 myJob2
                        dotnet run -v quiet rexfile.cs -- -v myJob1 myJob2
                        dotnet run -v quiet rexfile.cs -- -c myContext --auto target1

                    Options:
                        --auto                 Automatically determine the command based on the target. 
                        --context, -c <name>   Specify the context to use (default: 'default').
                        --dry-run, -d          Perform a dry run without executing tasks or jobs.
                        --env, -e <variable>   Set an environment variable (can be used multiple times). The format is <name>=<value>.
                        --env-file, -E <file>  Load environment variables from a file (can be used multiple times).
                        --help, -h             Show this help message.
                        --job, -J              Indicate that the targets are jobs.
                        --jobs                 Indicate that the targets are jobs (multiple).
                        --list, -l <targets>   List available tasks, jobs, or deployments. Targets can be 'tasks' or 'jobs'.
                        --task, -T             Indicate that the targets are tasks.
                        --tasks                Indicate that the targets are tasks (multiple).
                        --verbose, -v          Enable verbose output.
                    """;
                    Console.WriteLine(help);
                    return 0;
                }

            case "list":
                {
                    if (options.ListTargets.Length == 0)
                        options.ListTargets = ["tasks", "jobs"];

                    var la = options.ListTargets.ToList();

                    if (la.Contains("tasks") && globalTasks.Count > 0)
                    {
                        Console.WriteLine("TASKS:");
                        var max = globalTasks.Keys.Max(k => k.Length);
                        foreach (var kvp in globalTasks)
                        {
                            Console.WriteLine($"  {Ansi.Blue(kvp.Key.PadRight(max))}   {kvp.Value.Description}");
                        }

                        Console.WriteLine();
                    }

                    if (la.Contains("jobs") && globalJobs.Count > 0)
                    {
                        Console.WriteLine("JOBS:");
                        var max = globalJobs.Keys.Max(k => k.Length);
                        foreach (var kvp in globalJobs)
                        {
                            Console.WriteLine($"  {Ansi.Blue(kvp.Key.PadRight(max))}   {kvp.Value.Description}");
                        }

                        Console.WriteLine();
                    }
                }

                return 0;

            default:
                Console.WriteLine($"Unknown command '{options.Cmd}'.");
                return 1;
        }
    }

    private static void ParseArgs(string[] args, RexConsoleOptions options)
    {
        var targets = new List<string>();
        var additionalArgs = new List<string>();
        var many = false;
        var hasTarget = false;

        var isList = Array.IndexOf(args, "--list") >= 0 || Array.IndexOf(args, "-l") >= 0;
        var hasCommand = false;
        var isRemaining = false;

        if (args.Length == 1)
        {
            if (args[0] == "--help" || args[0] == "-h")
            {
                options.Cmd = "help";
                return;
            }
        }

        for (var i = 0; i < args.Length; i++)
            {
                var current = args[i];
                if (current.Length == 0)
                {
                    continue;
                }

                if (current.IsNullOrWhiteSpace())
                    continue;

                if (isRemaining)
                {
                    additionalArgs.Add(current);
                    continue;
                }

                var c = current[0];

                if (!hasTarget && c is not '-')
                {
                    targets.Add(current);

                    if (many)
                    {
                        var j = i + 1;
                        while (j < args.Length && args[j].Length > 0 && args[j][0] is not '-')
                        {
                            targets.Add(args[j]);
                            j++;
                        }
                    }

                    hasTarget = true;
                }

                if (current.Length == 2 && c is '-' && current[1] is '-')
                {
                    isRemaining = true;
                    continue;
                }

                if (c is not '-')
                {
                    additionalArgs.Add(current);
                    continue;
                }

                switch (current)
                {
                    case "--clean":
                    case "--build":
                    case "--test":
                    case "--restore":
                    case "--pack":
                    case "--publish":
                    case "--up":
                    case "--down":
                    case "--rollback":
                        {

                            if (isList)
                                continue;

                            var target = current[2..];

                            if (hasCommand)
                            {
                                var t = targets.FirstOrDefault();
                                if (t is not null && t != target)
                                {
                                    Console.WriteLine(Yellow($"Command '{options.Cmd}' is already set. {current} will be ignored."));
                                    continue;
                                }
                            }

                            hasCommand = true;
                            options.Cmd = "auto";
                            if (targets.Count == 0 || !targets.Contains(options.Cmd))
                            {
                                targets.Add(options.Cmd);
                                hasTarget = true;
                            }
                        }

                        break;

                    case "--many":
                        if (isList)
                            continue;

                        many = true;
                        options.Cmd = "auto";
                        break;
                    case "--auto":
                        options.Cmd = "auto";
                        break;

                    case "--tasks":
                        if (isList)
                            continue;

                        many = true;
                        options.Cmd = "tasks";
                        break;

                    case "--task":
                    case "-T":
                        if (isList)
                            continue;

                        many = false;
                        options.Cmd = "task";
                        break;

                    case "--job":
                    case "-J":
                        if (isList)
                            continue;

                        many = false;
                        options.Cmd = "job";
                        break;
                    case "--jobs":
                        if (isList)
                            continue;

                        many = true;
                        options.Cmd = "jobs";
                        break;

                    case "--list":
                    case "-l":
                        options.Cmd = "list";
                        var listArgs = new List<string>();
                        if (Array.IndexOf(args, "--tasks") >= 0 || Array.IndexOf(args, "--task") >= 0)
                        {
                            listArgs.Add("tasks");
                        }

                        if (Array.IndexOf(args, "--jobs") >= 0 || Array.IndexOf(args, "--job") >= 0)
                        {
                            listArgs.Add("jobs");
                        }

                        options.ListTargets = listArgs.ToArray();
                        break;

                    case "--dry-run":
                    case "--what-if":
                    case "-w":
                        options.DryRun = true;
                        break;

                    case "--verbose":
                    case "-v":
                        options.Verbose = true;
                        break;

                    case "--debug":
                    case "-d":
                        options.Debug = true;
                        break;

                    case "--timeout":
                    case "-t":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out var timeout))
                        {
                            options.Timeout = timeout;
                            i++;
                        }
                        else
                        {
                            Console.WriteLine("Timeout value is missing or invalid.");
                        }

                        break;
                    case "--env-file":
                    case "-E":
                        {
                            var j = i + 1;
                            var next = j < args.Length ? args[j] : null;
                            if (next is null)
                            {
                                Console.WriteLine("--env-file is missing path.");
                            }
                            else if (next[0] is '-' or '/')
                            {
                                Console.WriteLine("--env-file is missing path.");
                            }
                            else
                            {
                                options.EnvFiles.Add(next);
                                i++;
                            }
                        }

                        break;

                    case "--env":
                    case "-e":
                        {
                            var j = i + 1;
                            var next = j < args.Length ? args[j] : null;
                            if (next is null)
                            {
                                Console.WriteLine("--env is missing key=value.");
                            }
                            else if (next[0] is '-' or '/')
                            {
                                Console.WriteLine("--env is missing key=value.");
                            }
                            else
                            {
                                var eq = next.IndexOf('=');
                                if (eq > 0)
                                {
                                    var key = next.Substring(0, eq);
                                    var value = next.Substring(eq + 1);
                                    if (value[0] is '"' or '\'')
                                    {
                                        value = value[1..];
                                    }

                                    if (value.Length > 0 && value[^1] is '"' or '\'')
                                    {
                                        value = value[..^1];
                                    }

                                    options.Env[key] = value;
                                }
                                else
                                {
                                    Console.WriteLine("--env is missing '='. e.g. -e key=value.");
                                }

                                i++;
                            }
                        }

                        break;

                    default:
                        additionalArgs.Add(current);
                        break;
                }
            }

        options.Targets = targets.Count == 0 ? new[] { "default" } : targets.ToArray();
        options.Args = additionalArgs.ToArray();
    }
}