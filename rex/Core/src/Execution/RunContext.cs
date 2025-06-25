using Hyprx.Rex.Collections;

namespace Hyprx.Rex.Execution;

public class RunContext
{
    public RunContext(string name, IServiceProvider serviceProvider, string[]? args = null, string? cwd = null)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

        this.Name = name;
        this.Services = serviceProvider;
        this.Args = args ?? [];
        this.Cwd = cwd;

        foreach (var (key, value) in Hyprx.Env.Vars)
        {
            this.Env[key] = value;
        }
    }

    protected RunContext(RunContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        this.Name = context.Name;
        this.Env = new StringMap(context.Env);
        this.Secrets = new StringMap(context.Secrets);
        this.Outputs = new Outputs(context.Outputs);
        this.Services = context.Services;
        this.Cwd = context.Cwd;
        this.Args = context.Args;
    }

    public string Name { get; protected set; }

    public StringMap Env { get; protected set; } = new();

    public StringMap Secrets { get; protected set; } = new();

    public IServiceProvider Services { get; protected set; }

    public Outputs Outputs { get; protected set; } = new Outputs();

    public string? Cwd { get; protected set; }

    public string[] Args { get; protected set; } = [];
}