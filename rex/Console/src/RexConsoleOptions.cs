using Hyprx.Collections.Generic;
using Hyprx.Rex.Collections;
using Hyprx.Rex.Deployments;
using Hyprx.Rex.Jobs;
using Hyprx.Rex.Tasks;
using Hyprx.Secrets;

namespace Hyprx;

public class RexConsoleOptions
{
    public string[] Targets { get; set; } = [];

    public bool List { get; set; } = false;

    public bool DryRun { get; set; } = false;

    public bool Verbose { get; set; } = false;

    public bool Debug { get; set; } = false;

    public string Cmd { get; set; } = "auto";

    public string[] ListTargets { get; set; } = [];

    public string DeploymentAction { get; set; } = "deploy";

    public int Timeout { get; set; } = 0;

    public List<string> EnvFiles { get; set; } = new();

    public Dictionary<string, string> Env { get; set; } = new();

    public List<string> SecretFiles { get; set; } = new();

    public string Context { get; set; } = "default";

    public string[] Args { get; set; } = [];
}

public class RexConsoleSettings
{
    public IServiceProvider? Services { get; set; }

    public TaskMap Tasks { get; set; } = TaskMap.Global;

    public JobMap Jobs { get; set; } = JobMap.Global;

    public DeploymentMap Deployments { get; set; } = DeploymentMap.Default;

    public StringMap Secrets { get; set; } = new();

    public ISecretMasker? SecretMasker { get; set; }
}