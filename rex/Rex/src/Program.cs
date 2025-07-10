// See https://aka.ms/new-console-template for more information
#pragma warning disable SA1516
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Hyprx.DotEnv;

using Hyprx.DotEnv.Documents;
using Hyprx.DotEnv.Expansion;

using Hyprx.Exec;

using Hyprx.Rex;

using Hyprx.Rex.Commands;

var cwd = Environment.GetEnvironmentVariable("REX_PWD");
if (!string.IsNullOrEmpty(cwd))
{
    Environment.CurrentDirectory = cwd;
}

var listNamespacesCommand = new SubCommand("namespaces", "List all namespaces in the rexfile or project.")
{
    Options.File,
    Options.Verbose,
};

listNamespacesCommand.Aliases.Add("ns");
listNamespacesCommand.SetAction(Handlers.ListNamespacesAction());

var listServicesCommand = new SubCommand("services", "List all services in the rexfile or project.")
{
    Options.File,
    Options.Verbose,
};

listServicesCommand.SetAction(Handlers.ListServicesAction());

// rex list
var listAllCommand = new SubCommand("list", "List all available tasks and jobs in the rexfile or project.")
{
    Options.File,
    Options.Verbose,
    listNamespacesCommand,
    listServicesCommand,
};

listAllCommand.SetAction(Handlers.ListAllAction());

// rex tasks list
var taskListCommand = new SubCommand("list", "List available tasks.")
{
    Options.File,
    Options.Verbose,
};

taskListCommand.SetAction(Handlers.ListTasksAction());

// rex tasks run many build clean test
var taskRunManyCommand = new SubCommand("many", "Run all tasks provided in the targets argument.")
{
    Options.Context,
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
    Options.Targets,
};

taskRunManyCommand.TreatUnmatchedTokensAsErrors = false;
taskRunManyCommand.SetAction(Handlers.GenerateTargetsAction(() =>
{
    return new Hyprx.Exec.CommandArgs
     {
         "--tasks",
     };
}));

var taskRunCommand = new SubCommand("run", "Run a single task. A single task may include additional options and arguments.")
{
    Options.Context,
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
    Options.Target,
    Options.Service,
    taskRunManyCommand,
};

taskRunCommand.TreatUnmatchedTokensAsErrors = false;
taskRunCommand.SetAction(Handlers.GenerateTargetAction(() =>
{
    return new Hyprx.Exec.CommandArgs
     {
         "--task",
     };
}));

// rex tasks run "build"
var tasksCommand = new SubCommand("tasks", "Manage tasks. The tasks subcommand is also a shortcut for 'rex tasks run many <target1> <target2> ...'")
{
    Options.Context,
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
    taskRunCommand,
    taskListCommand,
};

tasksCommand.TreatUnmatchedTokensAsErrors = false;
tasksCommand.SetAction(Handlers.GenerateTargetsAction(() =>
{
    return new Hyprx.Exec.CommandArgs
     {
         "--tasks",
     };
}));

// rex jobs run many "build" "clean" "test"
var jobRunAllCommand = new SubCommand("many", "Run all jobs provided in the targets argument.")
{
    Options.Context,
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
    Options.Targets,
};

jobRunAllCommand.SetAction(Handlers.GenerateTargetsAction(() =>
{
    return new Hyprx.Exec.CommandArgs
     {
         "--jobs",
     };
}));

var jobRunCommand = new SubCommand("run", "Run a specific job.")
{
    Options.Context,
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
    Options.Target,
    Options.Service,
    jobRunAllCommand,
};

jobRunCommand.TreatUnmatchedTokensAsErrors = false;
jobRunCommand.SetAction(Handlers.GenerateTargetAction(() =>
{
    return new Hyprx.Exec.CommandArgs
     {
         "--job",
     };
}));

var jobListCommand = new SubCommand("list", "List all available jobs.")
{
    Options.File,
    Options.Verbose,
};

jobListCommand.SetAction(Handlers.ListJobsAction());

var jobsCommand = new SubCommand("jobs", "Manage jobs. By default this will run multiple jobs")
{
    Options.Context,
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
    Options.Targets,
    jobRunCommand,
    jobListCommand,
};

jobsCommand.TreatUnmatchedTokensAsErrors = false;
jobsCommand.SetAction(Handlers.GenerateTargetsAction(() =>
{
    return new Hyprx.Exec.CommandArgs
     {
         "--jobs",
     };
}));

// rex run many "build" "clean" "test"
var targetManyCommand = new SubCommand("many", "Run multiple targets. If the first target is a job, it will run all jobs. If the first target is a task, it will run all tasks.")
{
    Options.Context,
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
    Options.Targets,
};

targetManyCommand.TreatUnmatchedTokensAsErrors = false;
targetManyCommand.SetAction(Handlers.GenerateTargetsAction(() =>
{
    return new Hyprx.Exec.CommandArgs
     {
         "--auto",
         "--many",
     };
}));

var targetCommand = new SubCommand("run", "Run a target, task or job.")
{
    Options.Context,
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
    Options.Target,
    Options.Service,
    targetManyCommand,
};

targetCommand.TreatUnmatchedTokensAsErrors = false;
targetCommand.SetAction(Handlers.GenerateTargetAction(() =>
{
    return new Hyprx.Exec.CommandArgs
     {
         "--auto",
     };
}));

var buildCommand = new SubCommand("build", "Invokes the build job or task. This is a shortcut for 'rex run build` ")
{
    Options.Service,
    Options.Context,
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
};
buildCommand.SetAction(Handlers.GenerateAutoAction("build"));

var cleanCommand = new SubCommand("clean", "Invokes the clean job or task. This is a shortcut for 'rex run clean` ")
{
    Options.Service,
    Options.Context,
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
};
cleanCommand.SetAction(Handlers.GenerateAutoAction("clean"));

var testCommand = new SubCommand("test", "Runs tests. This is a shortcut for 'rex run test` ")
{
    Options.Service,
    Options.Context,
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
};

testCommand.TreatUnmatchedTokensAsErrors = false;
testCommand.SetAction(Handlers.GenerateAutoAction("test"));

var pack = new SubCommand("pack", "Packages a library or application. This is a shortcut for 'rex run pack` ")
{
    Options.Service,
    Options.File,
    Options.Context,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
};

pack.TreatUnmatchedTokensAsErrors = false;
pack.SetAction(Handlers.GenerateAutoAction("pack"));

var publishCommand = new SubCommand("publish", "Invokes the publish job or task. This is a shortcut for 'rex run publish` ")
{
    Options.Service,
    Options.Context,
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
};
publishCommand.TreatUnmatchedTokensAsErrors = false;
publishCommand.SetAction(Handlers.GenerateAutoAction("publish"));

var upCommand = new SubCommand("up", "Deploys or spins up infrastructure. This is a shortcut for 'rex run up` ")
{
    Options.Service,
    Options.Context,
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
};

upCommand.Aliases.Add("deploy");
upCommand.TreatUnmatchedTokensAsErrors = false;
upCommand.SetAction(Handlers.GenerateAutoAction("up"));

var downCommand = new SubCommand("down", "Spins down infrastructure or removes an application from deployment. This is a shortcut for 'rex run down` ")
{
    Options.Service,
    Options.File,
    Options.Context,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
};
downCommand.TreatUnmatchedTokensAsErrors = false;
downCommand.SetAction(Handlers.GenerateAutoAction("down"));

var rollback = new SubCommand("rollback", "Rolls back the last deployment. This is a shortcut for 'rex run rollback` ")
{
    Options.Service,
    Options.Context,
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
};
rollback.TreatUnmatchedTokensAsErrors = false;
rollback.SetAction(Handlers.GenerateAutoAction("rollback"));

var envArgs = new Argument<string[]>("env")
{
    Arity = ArgumentArity.ZeroOrMore,
    Description = "Environment variables to set in the form KEY=VALUE or path to envFile.",
};

var runArg = new Argument<string>("file")
{
    Arity = ArgumentArity.ExactlyOne,
    Description = "Files to run.",
};

var rootCommand = new RootCommand("rex is a task runner")
{
    Options.File,
    Options.Context,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
    Options.Target,
    Options.Service,
    listAllCommand,
    tasksCommand,
    jobsCommand,
    targetCommand,
    buildCommand,
    cleanCommand,
    testCommand,
    pack,
    publishCommand,
    upCommand,
    downCommand,
    rollback,
};

rootCommand.TreatUnmatchedTokensAsErrors = false;
rootCommand.SetAction(Handlers.GenerateTargetAction(() =>
{
    return new Hyprx.Exec.CommandArgs
     {
         "--auto",
     };
}));

var result = rootCommand.Parse(args);

await result.InvokeAsync();