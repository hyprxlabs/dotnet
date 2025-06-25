// See https://aka.ms/new-console-template for more information
#pragma warning disable SA1516
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;

using Hyprx.Rex.Commands;

var cwd = Environment.GetEnvironmentVariable("REX_PWD");
if (!string.IsNullOrEmpty(cwd))
{
    Environment.CurrentDirectory = cwd;
}

// rex list
var listAllCommand = new Command("list", "List all available tasks and jobs in the rexfile or project.")
{
    Options.File,
    Options.Verbose,
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
var taskRunManyCommand = new Command("many", "Run all tasks provided in the targets argument.")
{
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

var taskRunCommand = new Command("run", "Run a single task. A single task may include additional options and arguments.")
{
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
    Options.Target,
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
var tasksCommand = new Command("tasks", "Manage tasks. The tasks subcommand is also a shortcut for 'rex tasks run many <target1> <target2> ...'")
{
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
var jobRunAllCommand = new Command("many", "Run all jobs provided in the targets argument.")
{
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

var jobRunCommand = new Command("run", "Run a specific job.")
{
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
    Options.Target,
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

var jobListCommand = new Command("list", "List all available jobs.")
{
    Options.File,
    Options.Verbose,
};

jobListCommand.SetAction(Handlers.ListJobsAction());

var jobsCommand = new Command("jobs", "Manage jobs. By default this will run multiple jobs")
{
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
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
var runManyCommand = new Command("many", "Run multiple targets. If the first target is a job, it will run all jobs. If the first target is a task, it will run all tasks.")
{
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
    Options.Targets,
};

runManyCommand.TreatUnmatchedTokensAsErrors = false;
runManyCommand.SetAction(Handlers.GenerateTargetsAction(() =>
{
    return new Hyprx.Exec.CommandArgs
     {
         "--auto",
         "--many",
     };
}));

var runCommand = new Command("run", "Run a target, task or job.")
{
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
    Options.Target,
    runManyCommand,
};

runCommand.TreatUnmatchedTokensAsErrors = false;
runCommand.SetAction(Handlers.GenerateTargetAction(() =>
{
    return new Hyprx.Exec.CommandArgs
     {
         "--auto",
     };
}));

var buildCommand = new Command("build", "Invokes the build job or task. This is a shortcut for 'rex run build` ")
{
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
};
buildCommand.SetAction(Handlers.GenerateAutoAction("--build"));

var cleanCommand = new Command("clean", "Invokes the clean job or task. This is a shortcut for 'rex run clean` ")
{
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
};
cleanCommand.SetAction(Handlers.GenerateAutoAction("--clean"));

var testCommand = new Command("test", "Runs tests. This is a shortcut for 'rex run test` ")
{
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
};

testCommand.TreatUnmatchedTokensAsErrors = false;
testCommand.SetAction(Handlers.GenerateAutoAction("--test"));

var pack = new Command("pack", "Packages a library or application. This is a shortcut for 'rex run pack` ")
{
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
};

pack.TreatUnmatchedTokensAsErrors = false;
pack.SetAction(Handlers.GenerateAutoAction("--pack"));

var publishCommand = new Command("publish", "Invokes the publish job or task. This is a shortcut for 'rex run publish` ")
{
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
};
publishCommand.TreatUnmatchedTokensAsErrors = false;
publishCommand.SetAction(Handlers.GenerateAutoAction("--publish"));

var upCommand = new Command("up", "Deploys or spins up infrastructure. This is a shortcut for 'rex run up` ")
{
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
};
upCommand.TreatUnmatchedTokensAsErrors = false;
upCommand.SetAction(Handlers.GenerateAutoAction("--up"));

var downCommand = new Command("down", "Spins down infrastructure or removes an application from deployment. This is a shortcut for 'rex run down` ")
{
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
};
downCommand.TreatUnmatchedTokensAsErrors = false;
downCommand.SetAction(Handlers.GenerateAutoAction("--down"));

var rollback = new Command("rollback", "Rolls back the last deployment. This is a shortcut for 'rex run rollback` ")
{
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
};
rollback.TreatUnmatchedTokensAsErrors = false;
rollback.SetAction(Handlers.GenerateAutoAction("--rollback"));

var rootCommand = new RootCommand("rex is a task runner")
{
    Options.File,
    Options.Timeout,
    Options.Env,
    Options.EnvFiles,
    Options.SecretFiles,
    Options.Verbose,
    listAllCommand,
    taskRunCommand,
    taskRunManyCommand,
    tasksCommand,
    jobRunCommand,
    jobRunAllCommand,
    jobsCommand,
    jobListCommand,
    runCommand,
    buildCommand,
    cleanCommand,
};

var result = rootCommand.Parse(args);

await result.InvokeAsync();