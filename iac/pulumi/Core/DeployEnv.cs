
namespace Pulumi;

public readonly struct DeployEnv
{

    public DeployEnv(string name, string formatted, string abbr, string shortName)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Formatted = formatted ?? throw new ArgumentNullException(nameof(formatted));
        Abbr = abbr ?? throw new ArgumentNullException(nameof(abbr));
        ShortName = shortName ?? throw new ArgumentNullException(nameof(shortName));
    }

    public static DeployEnv Production { get; } = new DeployEnv("production", "Production", "prod", "p");

    public static DeployEnv Staging { get; } = new DeployEnv("staging", "Staging", "stg", "s");

    public static DeployEnv Development { get; } = new DeployEnv("development", "Development", "dev", "d");

    public static DeployEnv Testing { get; } = new DeployEnv("testing", "Testing", "test", "t");

    public static DeployEnv QualityAssurance { get; } = new DeployEnv("quality-assurance", "Quality Assurance", "qa", "q");

    public static DeployEnv UserAcceptanceTesting { get; } = new DeployEnv("user-acceptance-testing", "User Acceptance Testing", "uat", "u");

    public static DeployEnv PenetrationTesting { get; } = new DeployEnv("penetration-testing", "Penetration Testing", "pent", "pt");

    public string Name { get; }

    public string Formatted { get; }

    public string Abbr { get; }

    public string ShortName { get; }
}