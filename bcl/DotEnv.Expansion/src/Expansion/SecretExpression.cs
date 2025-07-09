using System.Globalization;
using System.Text;

using Hyprx.Secrets;

namespace Hyprx.DotEnv.Expansion;

public class SecretExpression
{
    public static string CommandName { get; set; } = "secret";

    public Uri Uri { get; set; } = new Uri("https://example.com");

    public string? Name { get; set; } = null;

    public bool Create { get; set; } = false;

    public int Size { get; set; } = 16;

    public bool Upper { get; set; } = true;

    public bool Lower { get; set; } = true;

    public bool Digits { get; set; } = true;

    public string Special { get; set; } = "@#~`_-[]|:^";

    public string? Chars { get; set; } = null;

    public DateTime? ExpiresAt { get; set; } = null;

    public List<string> Tokens { get; set; } = [];

    private enum Quote
    {
        None = 0,

        Single = 1,

        Double = 2,
    }

    public static List<string> ParseArgs(string args)
    {
        if (string.IsNullOrEmpty(args))
            return [];

        var token = new StringBuilder();
        var quote = Quote.None;
        var tokens = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            var c = args[i];

            if (quote != Quote.None)
            {
                switch (quote)
                {
                    case Quote.Single:
                        if (c == '\'')
                        {
                            quote = Quote.None;
                            if (token.Length > 0)
                            {
                                tokens.Add(token.ToString());
                                token.Clear();
                            }
                        }
                        else
                        {
                            token.Append(c);
                        }

                        continue;

                    case Quote.Double:
                        if (c == '\"')
                        {
                            quote = Quote.Double;
                            if (token.Length > 0)
                            {
                                tokens.Add(token.ToString());
                                token.Clear();
                            }
                        }
                        else
                        {
                            token.Append(c);
                        }

                        continue;
                }

                token.Append(c);
                continue;
            }

            if (c == ' ')
            {
                // handle backtick (`) and backslash (\) to notate a new line and different argument.
                var remaining = args.Length - 1 - i;
                if (remaining > 2)
                {
                    var j = args[i + 1];
                    var k = args[i + 2];

                    if ((j == '\\' || j == '`') && k == '\n')
                    {
                        i += 2;
                        if (token.Length > 0)
                        {
                            tokens.Add(token.ToString());
                        }

                        token.Clear();
                        continue;
                    }

                    if (remaining > 3)
                    {
                        var l = args[i + 3];
                        if (k == '\r' && l == '\n')
                        {
                            i += 3;
                            if (token.Length > 0)
                            {
                                tokens.Add(token.ToString());
                            }

                            token.Clear();
                            continue;
                        }
                    }
                }

                if (token.Length > 0)
                {
                    tokens.Add(token.ToString());
                    token.Clear();
                }

                continue;
            }

            if (token.Length == 0)
            {
                switch (c)
                {
                    case '\'':
                        quote = Quote.Single;
                        continue;

                    case '\"':
                        quote = Quote.Double;
                        continue;
                }
            }

            token.Append(c);
        }

        if (token.Length > 0)
            tokens.Add(token.ToString());

        token.Clear();

        return tokens;
    }

    public static (SecretExpression? Result, string? Error) Parse(IList<string> args, string commandName = "secret", string? workingDirectory = null, string? defaultUri = null)
    {
        var parts = args;
        if (parts.Count == 0)
        {
            return (null, "Expression cannot be empty.");
        }

        if (parts[0].EqualsFold(CommandName) || parts[0].EqualsFold(commandName))
        {
            parts.RemoveAt(0);
        }

        var first = parts.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(first))
        {
            return (null, "Expression cannot be empty.");
        }

        if (first.EqualsFold("default"))
         {
            if (defaultUri is null)
            {
                return (null, "Default URI is not set.");
            }

            first = defaultUri;
         }

        var cwd = workingDirectory ?? Environment.CurrentDirectory;

        if (first.Contains("$CWD"))
        {
            first = first.Replace("$CWD", cwd);
        }

        if (first.Contains("$PWD"))
        {
            first = first.Replace("$PWD", cwd);
        }

        if (Uri.TryCreate(first, UriKind.Absolute, out var uri))
        {
            parts.RemoveAt(0);
        }
        else
        {
            return (null, $"Invalid URL: {parts[0]}");
        }

        int size = 16;
        bool upper = true;
        bool lower = true;
        bool digits = true;
        string? chars = null;
        string? name = null;
        string? special = "@#~`_-[]|:^";
        bool create = false;
        DateTime? expiresAt = null;

        var ceiling = parts.Count;
        for (var i = 0; i < parts.Count; i++)
        {
            var part = parts[i];
            if (part.Length == 0 || part.Length == 1)
            {
                continue;
            }

            if (part[0] == '-' && part[1] != '-')
            {
                foreach (var c in part[1..])
                {
                    if (c == 'u')
                    {
                        upper = true;
                    }
                    else if (c == 'l')
                    {
                        lower = true;
                    }
                    else if (c == 'd')
                    {
                        digits = true;
                    }
                    else if (c == 's')
                    {
                        special = parts[++i];
                    }
                    else if (c == 'C')
                    {
                        create = true;
                    }
                }

                continue;
            }

            var j = i + 1;

            switch (part)
            {
                case "--size":
                    if (j < ceiling && int.TryParse(parts[j], out var parsedSize))
                    {
                        size = parsedSize;
                        i++;
                        continue;
                    }
                    else
                    {
                        return (null, "Invalid size parameter. Expected an integer value.");
                    }

                case "-n":
                case "--name":
                    if (j < ceiling)
                    {
                        name = parts[j];
                        i++;
                        continue;
                    }
                    else
                    {
                        return (null, "Invalid name parameter. Expected a string value.");
                    }

                case "--no-upper":
                    upper = false;
                    break;

                case "--no-lower":
                    lower = false;
                    break;

                case "--no-digits":
                    digits = false;
                    break;

                case "--no-special":
                    special = null;
                    break;

                case "--upper":
                    if (j < ceiling && parts[j].Length > 0 && parts[j][0] != '-')
                    {
                        upper = parts[j].EqualsFold("true");
                        i++;
                        continue;
                    }

                    upper = true;
                    break;
                case "--lower":
                    if (j < ceiling && parts[j].Length > 0 && parts[j][0] != '-')
                    {
                        lower = parts[j].EqualsFold("true");
                        i++;
                        continue;
                    }

                    lower = true;
                    break;
                case "--digits":
                    if (j < ceiling && parts[j].Length > 0 && parts[j][0] != '-')
                    {
                        digits = parts[j].EqualsFold("true");
                        i++;
                        continue;
                    }

                    digits = true;
                    break;
                case "--chars":
                    if (j < ceiling)
                    {
                        chars = parts[j];
                        i++;
                        continue;
                    }
                    else
                    {
                        return (null, "Invalid chars parameter. Expected a string value.");
                    }

                case "--special":
                    if (j < ceiling && parts[j].Length > 0 && parts[j][0] != '-')
                    {
                        special = parts[j];

                        if (special.EqualsFold("false"))
                        {
                            special = null;
                        }
                        else if (special.EqualsFold("true"))
                        {
                            special = "@#_-[];:|-~^";
                        }

                        i++;
                        continue;
                    }

                    break;

                case "--create":
                    if (j < ceiling && parts[j].Length > 0 && parts[j][0] != '-')
                    {
                        create = parts[j].EqualsFold("true");
                        i++;
                        continue;
                    }

                    create = true;
                    break;

                case "--expires-at":
                    if (j < ceiling && DateTime.TryParse(parts[j], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var expiresAtValue))
                    {
                        expiresAt = expiresAtValue.ToUniversalTime();
                        i++;
                        continue;
                    }
                    else
                    {
                        return (null, "Invalid expires-at parameter. Expected a valid date-time value.");
                    }
            }
        }

        return (new SecretExpression
        {
            Uri = uri,
            Create = create,
            Size = size,
            Upper = upper,
            Name = name,
            Lower = lower,
            Digits = digits,
            Special = special ?? "@#_-[];:|-~^",
            Chars = chars,
            ExpiresAt = expiresAt,
            Tokens = [.. parts],
        }, null);
    }

    public string Generate()
    {
        var sg = new SecretGenerator();
        if (!string.IsNullOrWhiteSpace(this.Chars))
        {
            sg.SetValidator(_ => true)
                .Add(this.Chars);
        }
        else
        {
            if (!this.Upper || !this.Lower || !this.Digits || string.IsNullOrWhiteSpace(this.Special))
            {
                sg.SetValidator((set) =>
                {
                    var testUpper = this.Upper;
                    var testLower = this.Lower;
                    var testDigits = this.Digits;
                    var testSpecial = !string.IsNullOrWhiteSpace(this.Special);

                    var hasUpper = set.Any(c => char.IsUpper(c));
                    var hasLower = set.Any(c => char.IsLower(c));
                    var hasDigit = set.Any(c => char.IsDigit(c));
                    var hasSpecial = set.Any(c => this.Special.Contains(c));

                    if (testUpper && !hasUpper)
                    {
                        return false;
                    }

                    if (testLower && !hasLower)
                    {
                        return false;
                    }

                    if (testDigits && !hasDigit)
                    {
                        return false;
                    }

                    if (testSpecial && !hasSpecial)
                    {
                        return false;
                    }

                    return true;
                });
            }

            if (this.Upper)
            {
                sg.Add(SecretCharacterSets.LatinAlphaUpperCase);
            }

            if (this.Lower)
            {
                sg.Add(SecretCharacterSets.LatinAlphaLowerCase);
            }

            if (this.Digits)
            {
                sg.Add(SecretCharacterSets.Digits);
            }

            if (!string.IsNullOrWhiteSpace(this.Special))
            {
                sg.Add(this.Special);
            }
        }

        if (this.Size <= 0)
        {
            this.Size = 16;
        }

        return sg.GenerateAsString(this.Size);
    }
}
