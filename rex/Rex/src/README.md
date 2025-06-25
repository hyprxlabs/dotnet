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

If the file is not found, rex will look for a rexfile.cs in the current folder
or look for a subfolder called .rex and then load a file called ./.rex/main.cs
or the first csproj it finds in the .rex folder: ./.rex/*.csproj.

Using the rex file is the most explicit.

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

## List tasks/jobs

```bash
rex list
```

list namespaces will show all namespaces that are defined.

```bash
rex list namespaces
```

list services will shall all namespaces that are defined as a service.

```bash
rex list services
```

## Run a task

```bash
# runs a job or task called hello
rex hello
```

```bash
# runs a job or task called hello
rex run hello
```

This is the most explicit.

```bash
# runs only a task called hello.
rex tasks run hello
```

## Run many tasks

```bash
rex run many "target1"  "target2"
```

```bash
rex tasks "task1" "task2"
```

This is the most explicit.

```bash
rex tasks run many "task1" "task2"
```

Note, if the task has the same name as a job or deployment, you'll need use
the tasks subcommand so that rex knows which one to use.

## Run a job

Runs a single job and any dependencies.

```bash
# runs a job or task alled job1
rex job1
```

```bash
# runs a job or task alled job1
rex run job1
```

```bash
rex jobs run job1
```

## Run many jobs

```bash
rex run many "job1" "job2"
```

```bash
rex jobs "job1" "job2"
```

this is the most explicit

```bash
rex jobs run many "job1" "job2"
```

## Alias Commands

- build
- clean
- test
- restore
- pack
- publish
- up
- down
- rollback

These are shortcuts to running a job or task with the same name. A command like
rex build is a shortcut to calling `rex run build`.  If a job is found the job
will be used and if a job doesn't an exist, but a task does, then the task called
build will be used.

These shortcut tasks also have an additional `service` argument.  
When the service argument is used and the service
argument value does not start with hyphen and the target does not contain a colon (:), the service
binds the target to a namespace.

So if you define a task called mysql:up you can run

```bash
rex up mysql
```

or you can run

```bash
rex mysql:up
```

so if you want to pass an argument in and not have it picked up as a service argument, you must
be explicit or prefix a task with colon (:).

```bash
rex mysql:up arg1
```

```bash
rex :up arg1
```

## Options

- **--context** - The context is useful for switching between contexts in a task. e.g. local vs ci.
- **--verbose** - Logs additional verbose information.
- **--env** - Sets an environment variable. Can be used many times.
- **--env-file** - Sets a dotenv file.  This file is loaded for the given task or job.
- **--secrets-file** - Sets a dotenv file. This file is treated as a file of secrets and is loaded for the given task or job.
   (Secrets are added to the SecretMasker which can be used to scrub output).
- **--timeout** - Sets the timeout for the job or task. If the execution takes too long, the job or task will be cancelled.
- **--file** - Sets the rexfile or csproj file that you want to use rather than looking in the current directory.

## Defining tasks

You'll need to have project or single csharp file that has a package reference to Hyprx.Rex.Console.

```cs
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

// use namespace will create a job or task with the given namespace.
// task up becomes mssql:up
// task down becomes mssql:down
//
// when true is set, the namespace is created like a service or project.
// so you can call
// `rex up mssql`  or  `rex run mssql:up`
//
// the concept of service is to make it easier to target a specific project
// service, or subfolder.

UseNamespace("mssql", true, () =>
{
    Task("up", () => Echo("Hello from MSSQL!"));
    Task("down", () => Echo("Goodbye from MSSQL!"));
});

return await RunTasksAsync(args);
```
