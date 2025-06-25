#pragma warning disable SA1117, SA1500, SA1116
using Hyprx;

using static Hyprx.RexConsole;
using static Hyprx.Shell;

Task("default", () => Echo("Hello, World!"));

Task("hello", () => Echo("Hello from RexKitchen!"));

Task("env", () => Echo(Env.Expand("$MY_VALUE")));

Task("build", () => Echo("Building..."));

Task("inspect", (ctx) =>
{
    Echo($"Context: {ctx.Name}");
    Echo($"Task: {ctx.Data.Id}");
    Echo($"Env: {string.Join(", ", ctx.Env)}");
    Echo($"Needs: {string.Join(", ", ctx.Data.Needs)}");
    Echo($"Args: {string.Join(", ", ctx.Args)}");
})
.WithNeeds(["hello"]);

Job("job1", (job) =>
{
    // addes the global task "hello" as a dependency to all tasks in this job
    // which allows you to share common tasks across multiple job
    // and run them as a standalone task as well.
    job.AddGlobalTask("hello");
    job.Task("task1", () => Echo("Task 1"));
    job.Task("task2", () => Echo("Task 2"));
});

UseNamespace("mssql", true, () =>
{
    Task("up", () => Echo("Hello from MSSQL!"));
    Task("down", () => Echo("Goodbye from MSSQL!"));
});

return await RunTasksAsync(args);