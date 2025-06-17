# Hyprx.Rex.Console

## Overview

The Hyprx.Rex.Console package enables you to create a cli task runner or
a make-like experience in .NET within console projects or using `dotnet run app.cs` (single files).

## Usage

```csharp
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

## Command Line usage

Commands can either use a project or file.

When using a file, you must have .NET 10 or higher and use the following command:

```bash
dotnet run -v quiet app.cs
```

When using a project use the following:

```bash
dotnet run -v quiet --project ./path/to/project.csproj
```

When running an assembly, use:

```bash
dotnet ./path/to/myapp.dll
```

### list tasks

```bash
dotnet run -v quiet app.cs --list
```

### run a task

```bash
dotnet run -v quiet app.cs  hello
```

```bash
dotnet run -v quiet app.cs --task hello
```

### run a job

```bash
dotnet run -v quiet app.cs job1
```

```bash
dotnet run -v quiet app.cs --job hello
```

### run a deployment

```bash
dotnet run -v quiet app.cs job1
```

```bash
dotnet run -v quiet app.cs --deploy hello
```

### rollback a deployment

```bash
dotnet run -v quiet app.cs --rollback hello
```

### delete a deployment

```bash
dotnet run -v quiet app.cs --destroy hello
```
