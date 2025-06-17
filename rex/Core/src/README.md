# Hyprx.Rex.Core

## Overview

Core provides the key primitives for tasks, jobs, and deployments for running
rex code tasks.

Task authors may reference this library, however, most users should import
Hyprx.Rex.Console instead.

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
