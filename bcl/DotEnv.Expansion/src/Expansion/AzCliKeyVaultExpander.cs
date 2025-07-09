using System.Diagnostics;

using Hyprx.Secrets;

namespace Hyprx.DotEnv.Expansion;

public class AzCliKeyVaultExpander : ISecretVaultExpander
{
    public string Name => "az-cli-keyvault";

    public bool Synchronous => true;

    public string Protocol { get; set; } = "akv";

    public string SecretsExpression { get; set; } = "secret";

    public string? ClientSecret { get; set; } = null;

    public string? KeyVaultName { get; set; } = null;

    public bool IsDefault { get; set; } = false;

    protected string? DefaultUri
    {
        get
        {
            if (!this.IsDefault)
                return null;

            if (string.IsNullOrWhiteSpace(this.KeyVaultName))
            {
                return null;
            }

            return $"{this.Protocol}://{this.KeyVaultName}";
        }
    }

    public bool CanHandle(IList<string> expressions)
    {
        if (expressions.Count < 3)
            return false;

        if (!expressions[0].EqualsFold("secret"))
            return false;

        var url = expressions[1];

        if (this.IsDefault && url.EqualsFold("default") && !string.IsNullOrWhiteSpace(this.KeyVaultName))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(this.KeyVaultName))
        {
            return url.StartsWith($"{this.Protocol}://{this.KeyVaultName}", StringComparison.OrdinalIgnoreCase) ||
                    url.StartsWith($"azure-key-vault://{this.KeyVaultName}", StringComparison.OrdinalIgnoreCase);
        }

        return url.StartsWith($"{this.Protocol}://", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith($"azure-key-vault://", StringComparison.OrdinalIgnoreCase);
    }

    public ExpansionResult Expand(IList<string> args)
    {
        var (result, error) = SecretExpression.Parse(args, this.SecretsExpression, null, this.DefaultUri);
        if (error != null)
        {
            return new ExpansionResult
            {
                Error = new Exception($"Failed to parse secret expression '{string.Join(" ", args)}': {error}"),
                Position = -1,
            };
        }

        if (result is null)
        {
            return new ExpansionResult
            {
                Error = new InvalidOperationException($"Failed to parse secret expression '{string.Join(" ", args)}'."),
                Position = -1,
            };
        }

        var domain = result.Uri.Host;
        var path = result.Uri.AbsolutePath.TrimStart('/');
        if (path.Length == 0)
        {
            path = result.Name ?? string.Empty;
            if (path.Length == 0)
            {
                return new ExpansionResult
                {
                    Error = new InvalidOperationException("Secret name cannot be empty. Either use the --name flag or for akv add the name as a path e.g. akv://vault/secret-name."),
                    Position = -1,
                };
            }
        }

        var version = string.Empty;
        var key = path;
        key = Convert(key);
        if (path.Contains('/'))
        {
            var parts = path.Split('/');
            if (parts.Length > 1)
            {
                version = parts[0];
                key = string.Join('/', parts.Skip(1));
            }
        }

        var list = new List<string>()
        {
            "keyvault",
            "secret",
            "show",
            "--vault-name", domain,
            "--name", key,
            "-o", "tsv",
            "--query", "value",
        };

        var si = new ProcessStartInfo()
        {
            FileName = "az",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (var arg in list)
        {
            si.ArgumentList.Add(arg);
        }

        var process = new Process()
        {
            StartInfo = si,
        };

        process.Start();

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        var text = stdout.Trim();

        if (process.ExitCode == 0)
        {
            return new ExpansionResult
            {
                Value = text,
                Position = -1,
            };
        }

        if (text.Contains("NotFound") || stderr.Contains("NotFound"))
        {
            if (result.Create)
            {
                var sg = new SecretGenerator();

                if (!string.IsNullOrWhiteSpace(result.Chars))
                {
                    sg.Add(result.Chars);
                }
                else
                {
                    if (result.Upper)
                        sg.Add(SecretCharacterSets.LatinAlphaUpperCase);
                    if (result.Lower)
                        sg.Add(SecretCharacterSets.LatinAlphaLowerCase);
                    if (result.Digits)
                        sg.Add(SecretCharacterSets.Digits);

                    if (!string.IsNullOrEmpty(result.Special))
                        sg.Add(result.Special);
                }

                var secretValue = sg.GenerateAsString(result.Size);

                var args0 = new List<string>()
                            {
                                "keyvault",
                                "secret",
                                "set",
                                "--name", key,
                                "--value", secretValue,
                                "--vault-name", domain,
                            };

                if (result.ExpiresAt.HasValue)
                {
                    args0.Add("--expires");
                    args0.Add(result.ExpiresAt.Value.ToString("o"));
                }

                var si2 = new ProcessStartInfo()
                {
                    FileName = "az",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                foreach (var arg in args0)
                {
                    si2.ArgumentList.Add(arg);
                }

                var process2 = new Process()
                {
                    StartInfo = si2,
                };

                process2.Start();
                process2.WaitForExit();
                var stdout2 = process2.StandardOutput.ReadToEnd();
                var stderr2 = process2.StandardError.ReadToEnd();

                if (process2.ExitCode != 0)
                {
                    return new ExpansionResult
                    {
                        Error = new Exception($"Failed to set secret '{key}' in vault '{domain}'. \n stdout: {stdout2} \n stderr: {stderr2}"),
                    };
                }

                return new ExpansionResult
                {
                    Value = secretValue,
                    Position = -1,
                };
            }
        }

        return new ExpansionResult
        {
            Error = new Exception($"Failed to expand secret '{string.Join(" ", args)}'. \n stdout:{text} \n stderr: {stderr}"),
            Position = -1,
        };
    }

    public async Task<ExpansionResult> ExpandAsync(IList<string> args, CancellationToken cancellationToken = default)
    {
        var (result, error) = SecretExpression.Parse(args, this.SecretsExpression, null, this.DefaultUri);
        if (error != null)
        {
            return new ExpansionResult
            {
                Error = new Exception($"Failed to parse secret expression '{string.Join(" ", args)}': {error}"),
                Position = -1,
            };
        }

        if (result is null)
        {
            return new ExpansionResult
            {
                Error = new InvalidOperationException($"Failed to parse secret expression '{string.Join(" ", args)}'."),
                Position = -1,
            };
        }

        var domain = result.Uri.Host;
        var path = result.Uri.AbsolutePath.TrimStart('/');
        if (path.Length == 0)
        {
            path = result.Name ?? string.Empty;
            if (path.Length == 0)
            {
                return new ExpansionResult
                {
                    Error = new InvalidOperationException("Secret name cannot be empty. Either use the --name flag or for akv add the name as a path e.g. akv://vault/secret-name."),
                    Position = -1,
                };
            }
        }

        var version = string.Empty;
        var key = path;
        if (path.Contains('/'))
        {
            var parts = path.Split('/');
            if (parts.Length > 1)
            {
                version = parts[0];
                key = string.Join('/', parts.Skip(1));
            }
        }

        key = Convert(key);

        var list = new List<string>()
        {
            "keyvault",
            "secret",
            "show",
            "--vault-name", domain,
            "--name", key,
            "-o", "tsv",
            "--query", "value",
        };

        var si = new ProcessStartInfo()
        {
            FileName = "az",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (var arg in list)
        {
            si.ArgumentList.Add(arg);
        }

        var process = new Process()
        {
            StartInfo = si,
        };

        process.Start();

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(cancellationToken);

        var text = stdout.Trim();

        if (process.ExitCode == 0)
        {
            return new ExpansionResult
            {
                Value = text,
                Position = -1,
            };
        }

        if (text.Contains("NotFound") || stderr.Contains("NotFound"))
        {
            if (result.Create)
            {
                var sg = new SecretGenerator();

                if (!string.IsNullOrWhiteSpace(result.Chars))
                {
                    sg.Add(result.Chars);
                }
                else
                {
                    if (result.Upper)
                        sg.Add(SecretCharacterSets.LatinAlphaUpperCase);
                    if (result.Lower)
                        sg.Add(SecretCharacterSets.LatinAlphaLowerCase);
                    if (result.Digits)
                        sg.Add(SecretCharacterSets.Digits);

                    if (!string.IsNullOrEmpty(result.Special))
                        sg.Add(result.Special);
                }

                var secretValue = sg.GenerateAsString(result.Size);

                var argList = new List<string>()
                            {
                                "keyvault",
                                "secret",
                                "set",
                                "--name", key,
                                "--value", secretValue,
                                "--vault-name", domain,
                            };

                if (result.ExpiresAt.HasValue)
                {
                    argList.Add("--expires");
                    argList.Add(result.ExpiresAt.Value.ToString("o"));
                }

                var si2 = new ProcessStartInfo()
                {
                    FileName = "az",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                foreach (var arg in argList)
                {
                    si2.ArgumentList.Add(arg);
                }

                var process2 = new Process()
                {
                    StartInfo = si2,
                };

                process2.Start();
                await process2.WaitForExitAsync(cancellationToken);
                var stdout2 = await process2.StandardOutput.ReadToEndAsync();
                var stderr2 = await process2.StandardError.ReadToEndAsync();

                if (process2.ExitCode != 0)
                {
                    return new ExpansionResult
                    {
                        Error = new Exception($"Failed to set secret '{key}' in vault '{domain}'. \n stdout: {stdout2} \n stderr: {stderr2}"),
                    };
                }

                return new ExpansionResult
                {
                    Value = secretValue,
                    Position = -1,
                };
            }
        }

        return new ExpansionResult
        {
            Error = new Exception($"Failed to expand secret '{string.Join(" ", args)}'. \n stdout:{text} \n stderr: {stderr}"),
            Position = -1,
        };
    }

    private static string Convert(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        var builder = StringBuilderCache.Acquire();
        foreach (var c in name)
        {
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(c);
            }
            else if (c is '-' or '_' or '.' or '/' or ':' or '\\')
            {
                builder.Append('-');
            }
            else
            {
                continue;
            }
        }

        return builder.ToString();
    }
}