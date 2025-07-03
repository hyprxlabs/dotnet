namespace Hyprx.DotEnv.Expansion;

public class ExpansionBuilder
{
    private readonly ExpansionOptions options;

    public ExpansionBuilder(ExpansionOptions? options = null)
    {
        this.options = options ?? new ExpansionOptions();
    }

    public ExpansionBuilder WithUnixArgs(bool enable = true)
    {
        this.options.EnableUnixArgs = enable;
        return this;
    }

    public ExpansionBuilder WithWindowsExpansion(bool enable = true)
    {
        this.options.EnableWindowsVariables = enable;
        return this;
    }

    public ExpansionBuilder WithExpressions(bool enable = true)
    {
        this.options.EnableSubExpressions = enable;
        return this;
    }

    public ExpansionBuilder WithShell(bool enable = true, string shell = "bash")
    {
        this.options.EnableShell = enable;
        this.options.Shell = shell;
        return this;
    }

    /// <summary>
    /// The is the expression used to represent a cli that expands secrets. The default is
    /// <c>secret</c>. This expression is used to expand secrets from a secret vault.
    /// The expression can be used in the following way:
    /// <c>MYVAR=$(secret sops-age-env:///path/to/secret --name super-secret)</c>.
    /// </summary>
    /// <param name="expression">The expression to use for expanding secrets.</param>
    /// <returns>The expansion builder.</returns>
    /// <exception cref="ArgumentException">Thrown when expression is null, empty, or whitespace.</exception>
    public ExpansionBuilder WithSecretsExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression cannot be null or whitespace.", nameof(expression));

        this.options.SecretsExpression = expression;
        return this;
    }

    public ExpansionBuilder AddSecretVaultExpander(ISecretVaultExpander expander)
    {
        if (expander == null)
            throw new ArgumentNullException(nameof(expander));

        this.options.SecretVaultExpanders.Add(expander);
        return this;
    }

    public ExpansionBuilder AddAzureCliKeyVault(Action<AzCliKeyVaultExpander>? configure = null)
    {
        var expander = new AzCliKeyVaultExpander();
        configure?.Invoke(expander);
        this.AddSecretVaultExpander(expander);
        return this;
    }

    public ExpansionBuilder AddSopsCliAgeEnv(Action<SopsCliAgeEnvExpander>? configure = null)
    {
        var expander = new SopsCliAgeEnvExpander();
        configure?.Invoke(expander);
        this.AddSecretVaultExpander(expander);
        return this;
    }

    public ExpansionBuilder WithSecretVaultExpanders(IEnumerable<ISecretVaultExpander> expanders)
    {
        if (expanders == null)
            throw new ArgumentNullException(nameof(expanders));

        this.options.SecretVaultExpanders.Clear();

        foreach (var expander in expanders)
        {
            this.AddSecretVaultExpander(expander);
        }

        return this;
    }

    public DotEnvExpander Build()
    {
        foreach (var expander in this.options.SecretVaultExpanders)
        {
             expander.SecretsExpression = this.options.SecretsExpression;
        }

        return new DotEnvExpander(this.options);
    }
}