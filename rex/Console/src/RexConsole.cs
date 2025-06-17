using System.Reflection.Metadata;

using Hyprx.DotEnv.Documents;

using Hyprx.Lodi;
using Hyprx.Results;

using Hyprx.Rex.Collections;
using Hyprx.Rex.Deployments;
using Hyprx.Rex.Execution;
using Hyprx.Rex.Jobs;
using Hyprx.Rex.Messaging;
using Hyprx.Rex.Tasks;
using Hyprx.Secrets;

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

            if (globalTasks.ContainsKey(first))
            {
                options.Cmd = "task";
            }
            else if (globalJobs.ContainsKey(first))
            {
                options.Cmd = "job";
            }
            else if (globalDeployments.ContainsKey(first))
            {
                options.Cmd = "deploy";
            }
            else
            {
                Console.WriteLine($"Target '{first}' not found in tasks or jobs.");
                return 1;
            }
        }

        var runContext = new RunContext(options.Context, serviceProvider);

        switch (options.Cmd)
        {
            case "task":
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

            case "job":
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

            case "deployment":
            case "deploy":
            case "destroy":
            case "rollback":
                {
                    DeploymentAction action = options.DeploymentAction switch
                    {
                        "deploy" => DeploymentAction.Deploy,
                        "destroy" => DeploymentAction.Destroy,
                        "rollback" => DeploymentAction.Rollback,
                        _ => DeploymentAction.Deploy,
                    };

                    if (options.Targets.Length > 1)
                    {
                        Console.WriteLine("Multiple deployment targets are not currently supported.");
                        return 1;
                    }

                    if (!Deployments.TryGetValue(options.Targets[0], out var deployment))
                    {
                        Console.WriteLine($"Deployment '{options.Targets[0]}' not found.");
                        return 1;
                    }

                    var data = new CodeDeploymentData(deployment.Id);
                    data.Action = action;
                    var ctx = new DeploymentPipelineContext(runContext, deployment, data);
                    var pipeline = serviceProvider.GetService(typeof(DeploymentPipeline)) as DeploymentPipeline;
                    var result = await pipeline!.RunAsync(ctx, cancellationToken);

                    if (result.Error is not null)
                    {
                        return 1;
                    }

                    return 0;
                }

            case "list":
                {
                    if (options.ListTargets.Length == 0)
                        options.ListTargets = ["tasks", "jobs", "deployments"];

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

                    if (la.Contains("deployments") && globalDeployments.Count > 0)
                    {
                        Console.WriteLine("DEPLOYMENTS:");
                        var max = globalDeployments.Keys.Max(k => k.Length);
                        foreach (var kvp in globalDeployments)
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
        bool isTarget = false;
        var targets = new List<string>();
        var additionalArgs = new List<string>();

        var isList = Array.IndexOf(args, "--list") >= 0 || Array.IndexOf(args, "-l") >= 0;

        for (var i = 0; i < args.Length; i++)
        {
            var current = args[i];
            if (current.Length == 0)
            {
                continue;
            }

            if (current[0] is '-' && !isTarget)
            {
                switch (current)
                {
                    case "--task":
                    case "--tasks":
                        if (isList)
                            continue;

                        options.Cmd = "task";
                        break;

                    case "--job":
                    case "--jobs":
                        if (isList)
                            continue;

                        options.Cmd = "job";
                        break;

                    case "--deploy":
                        if (isList)
                            continue;
                        options.Cmd = "deploy";
                        options.DeploymentAction = "deploy";
                        break;

                    case "--destroy":
                        if (isList)
                            continue;
                        options.Cmd = "deploy";
                        options.DeploymentAction = "destroy";
                        break;

                    case "--rollback":
                        if (isList)
                            continue;
                        options.Cmd = "deploy";
                        options.DeploymentAction = "rollback";
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

                        if (Array.IndexOf(args, "--deployments") >= 0 || Array.IndexOf(args, "--deployment") >= 0)
                        {
                            listArgs.Add("deployments");
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

                    case "--":
                        isTarget = true;
                        break;

                    default:
                        Console.WriteLine($"Unknown option '{current}'.");
                        break;
                }
            }
            else if (isTarget && current[0] is '-' or '/')
            {
                isTarget = true;
                var j = i;
                for (; j < args.Length; j++)
                {
                    if (args[j] == "--")
                    {
                        j++;
                        continue;
                    }

                    additionalArgs.Add(args[j]);
                }

                break;
            }
            else
            {
                isTarget = true;
                targets.Add(current);
            }
        }

        options.Targets = targets.Count == 0 ? new[] { "default" } : targets.ToArray();
        options.Args = additionalArgs.ToArray();
    }
}