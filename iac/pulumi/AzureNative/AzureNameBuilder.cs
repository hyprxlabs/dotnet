using System.Text;

using Pulumi.AzureNative.App;

using Pulumi.AzureNative.Migrate;

namespace Pulumi.AzureNative;

public class AzureNameBuilder
{
    private readonly StringBuilder sb;

    public AzureNameBuilder()
    {
        this.sb = new StringBuilder();
    }

    public int Length => this.sb.Length;

    public static string Build(AzureResourcePrefix prefix, string workload, DeployEnv env, AzureRegion region, string instance)
    {
        var builder = new AzureNameBuilder();
        builder.AppendPrefix(prefix);
        builder.AppendWorkload(workload);
        builder.AppendEnv(env);
        builder.AppendRegion(region);
        builder.AppendInstance(instance);

        return builder.Build();
    }

    public static string Build(AzureResourcePrefix prefix, string workload, DeployEnv env, AzureRegion region, int instance)
    {
        var builder = new AzureNameBuilder();
        builder.AppendPrefix(prefix);
        builder.AppendWorkload(workload);
        builder.AppendEnv(env);
        builder.AppendRegion(region);
        builder.AppendInstance(instance);

        return builder.Build();
    }

    public static string Build(AzureResourcePrefix prefix, string workload, DeployEnv env, AzureRegion region)
    {
        var builder = new AzureNameBuilder();
        builder.AppendPrefix(prefix);
        builder.AppendWorkload(workload);
        builder.AppendEnv(env);
        builder.AppendRegion(region);

        return builder.Build();
    }

    public static string Build(AzureResourcePrefix prefix, string workload, DeployEnv env)
    {
        var builder = new AzureNameBuilder();
        builder.AppendPrefix(prefix);
        builder.AppendWorkload(workload);
        builder.AppendEnv(env);

        return builder.Build();
    }

    public static string Build(AzureResourcePrefix prefix, string workload)
    {
        var builder = new AzureNameBuilder();
        builder.AppendPrefix(prefix);
        builder.AppendWorkload(workload);

        return builder.Build();
    }

    public AzureNameBuilder Append(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, nameof(value));
        this.sb.Append(value);
        return this;
    }

    public AzureNameBuilder AppendPrefix(AzureResourcePrefix prefix)
    {
        this.Append(prefix.Prefix);
        this.Append("-");
        return this;
    }

    public AzureNameBuilder AppendEnv(DeployEnv env)
    {
        ArgumentNullException.ThrowIfNull(env, nameof(env));
        this.Append(env.Abbr);
        this.Append("-");
        return this;
    }

    public AzureNameBuilder AppendRegion(AzureRegion region)
    {
        ArgumentNullException.ThrowIfNull(region, nameof(region));
        this.Append(region.Name);
        this.Append("-");
        return this;
    }

    public AzureNameBuilder AppendOrg(string org)
    {
        ArgumentException.ThrowIfNullOrEmpty(org, nameof(org));
        this.Append(org);
        this.Append("-");
        return this;
    }

    public AzureNameBuilder AppendWorkload(string workload)
    {
        ArgumentException.ThrowIfNullOrEmpty(workload, nameof(workload));
        this.Append(workload);
        return this;
    }

    public AzureNameBuilder AppendInstance(string instance)
    {
        ArgumentException.ThrowIfNullOrEmpty(instance, nameof(instance));
        this.Append(instance);
        return this;
    }

    public AzureNameBuilder AppendInstance(int instance)
    {
        this.Append(instance.ToString());
        return this;
    }

    public void Clear()
    {
        this.sb.Clear();
    }

    public string Build()
    {
        if (this.Length == 0)
            return string.Empty;

        var last = char.MinValue;
        last = this.sb[this.Length - 1];
        while (last == '-')
        {
            this.sb.Remove(this.Length - 1, 1);
            if (this.Length == 0)
                break;

            last = this.sb[this.Length - 1];
        }

        return this.sb.ToString();
    }

    public override string ToString()
    {
        return this.Build();
    }
}