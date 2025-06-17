# dotnet-rex

This is a cli tool that can run projects or files that leverage Hyprx.Rex.Console.

DotNet 10 or higher must be installed locally to use rex.

The cli tool has help support with --help flag.

## Rexfile

The rex cli will look in the current working directory and look for a file called
"rexfile" with no extension. This file is a configuration file for telling rex
where to look for the file or csproj that contains tasks.

The rexfile setting within the file will tell rex where to find either the
csproj file, the single csharp file, or the assembly to run.

### Using a project

```text
rexfile:./RexKitchen/RexKitchen.csproj
```

### Using a file

```text
rexfile:./path/to/rexfile.cs
```

```text
rexfile:./path/to/app.cs
```

### Using an assembly

```text
rexfile:./path/to/app.dll
```

## Run a task

```bash
rex tasks run hello
```

```bash
rex tasks hello
```

```bash
rex hello
```

Note, if the task has the same name as a job or deployment, you'll need use
the tasks subcommand so that rex knows which one to use.

## Run a job

A job is a collection of tasks to run in sequential order.

```bash
rex jobs run job1
```

```bash
rex jobs job1
```

```bash
rex job1
```

Note, if the task has the same name as a job or deployment, you'll need use
the tasks subcommand so that rex knows which one to use.

## Run a deployment, rollback, or deletion

deploy

```bash
rex deploy deployment-name
```

## Rollback

Rollbacks are only supported if one was configured when defining the deployment.

```bash
rex rollback deployment-name
```

## Destroy

Destroys are only supported if one was configured when defining the deployment.

```bash
rex destroy deployment-name
```

## Defining tasks

You'll need to have project or single csharp file that has a package reference to Hyprx.Rex.Console.

```cs
using static Hyprx.RexConsole;
using static Hyprx.Shell;

Task("default", () => Echo("Hello, World!"));

Task("hello", () => Echo("Hello from RexKitchen!"));

Task("env", () => Echo(Env.Expand("$MY_VALUE")));

Job("job1", (job) =>
{
    // addes the global task "hello" as a dependency to all tasks in this job
    // which allows you to share common tasks across multiple job
    // and run them as a standalone task as well.
    job.AddGlobalTask("hello");
    job.Task("task1", () => Echo("Task 1"));
    job.Task("task2", () => Echo("Task 2"));
});

Deployment("deploy1",
    (ctx) =>
    {
        Echo("Starting deployment...");
        Echo("Deployment finished.");
    })
    .BeforeDeploy((before) =>
    {
        before.Task("hello", () => Echo("Preparing to say hello..."));
    })
    .WithRollback(() =>
    {
        Echo("Rolling back deployment...");
        Echo("Rollback complete.");
    });

return await RunTasksAsync(args);
```
