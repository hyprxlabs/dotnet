using System.Collections.Concurrent;

namespace Hyprx.DotEnv.Expansion;

public class SopsCliAgeEnvExpander : ISecretVaultExpander
{
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> envFiles = new();

    private readonly SemaphoreSlim semaphore = new(1, 1);

    public string Name => "sops-cli-age-env";

    public string SecretsExpression { get; set; } = "secret";

    public string Protocol { get; set; } = "sops-age-env";

    public string? EnvPath { get; set; } = null;

    public string? SopsConfigPath { get; set; } = null;

    public string? AgeKeyFile { get; set; } = null;

    public string? AgeKey { get; set; } = null;

    public bool CanHandle(IList<string> expressions)
    {
        if (expressions is null || expressions.Count < 3)
        {
            return false;
        }

        if (!expressions[0].EqualsFold(this.SecretsExpression))
            return false;

        if (!string.IsNullOrWhiteSpace(this.EnvPath))
        {
            return expressions[1].StartsWith($"{this.Protocol}:///{this.EnvPath}", StringComparison.OrdinalIgnoreCase);
        }

        return expressions[1].StartsWith($"{this.Protocol}:///", StringComparison.OrdinalIgnoreCase);
    }

    public bool Synchronous => true;

    public ExpansionResult Expand(IList<string> args)
    {
        var (expression, error) = SecretExpression.Parse(args);
        if (error is not null)
        {
            return new ExpansionResult()
            {
                Error = new Exception($"Failed to parse the secret expression: {error}"),
            };
        }

        if (expression is null)
        {
            return new ExpansionResult()
            {
                Error = new ArgumentException("The secret expression is invalid."),
            };
        }

        var (envFile, envFileError) = this.GetEnvFile(expression);
        if (envFileError is not null)
        {
            return new ExpansionResult()
            {
                Error = new Exception($"Failed to get the env file: {envFileError}"),
            };
        }

        if (envFile is null || envFile.Count == 0)
        {
            return new ExpansionResult()
            {
                Error = new ArgumentException("The env file is empty or not found."),
            };
        }

        var query = expression.Uri.Query;
        var name = expression.Name ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            var nv = System.Web.HttpUtility.ParseQueryString(query);
            name = nv["name"] ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return new ExpansionResult()
            {
                Error = new InvalidOperationException("Secret name cannot be empty. Use the --name option or the 'name' query parameter e.g. sops-age-env:///path/to/file?name=secretName."),
            };
        }

        if (!envFile.TryGetValue(name, out var value))
        {
            if (expression.Create)
            {
                // Create the secret
                var (secret, error2) = this.GenerateAndEncrypt(expression, envFile);
                if (error2 is not null)
                {
                    return new ExpansionResult()
                    {
                        Error = new Exception($"Failed to generate and encrypt the secret: {error2.Message}"),
                    };
                }

                value = secret;
            }
            else
            {
                // If the secret is not found and not created, return an error.
                return new ExpansionResult()
                {
                    Error = new KeyNotFoundException($"The secret '{name}' was not found in the env file."),
                };
            }
        }

        return new ExpansionResult()
        {
            Value = value!,
        };
    }

    public async Task<ExpansionResult> ExpandAsync(IList<string> args, CancellationToken cancellationToken = default)
    {
        var (expression, error) = SecretExpression.Parse(args);
        if (error is not null)
        {
            return new ExpansionResult()
            {
                Error = new Exception($"Failed to parse the secret expression: {error}"),
            };
        }

        if (expression is null)
        {
            return new ExpansionResult()
            {
                Error = new ArgumentException("The secret expression is invalid."),
            };
        }

        var (envFile, envFileError) = await this.GetEnvFileAsync(expression, cancellationToken);
        if (envFileError is not null)
        {
            return new ExpansionResult()
            {
                Error = new Exception($"Failed to get the env file: {envFileError}"),
            };
        }

        if (envFile is null || envFile.Count == 0)
        {
            return new ExpansionResult()
            {
                Error = new ArgumentException("The env file is empty or not found."),
            };
        }

        var query = expression.Uri.Query;
        var name = expression.Name ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            var nv = System.Web.HttpUtility.ParseQueryString(query);
            name = nv["name"] ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return new ExpansionResult()
            {
                Error = new InvalidOperationException("Secret name cannot be empty. Use the --name option or the 'name' query parameter e.g. sops-age-env:///path/to/file?name=secretName."),
            };
        }

        if (!envFile.TryGetValue(name, out var value))
        {
            if (expression.Create)
            {
                // Create the secret
                var (secret, error2) = await this.GenerateAndEncryptAsync(expression, envFile, cancellationToken);
                if (error2 is not null)
                {
                    return new ExpansionResult()
                    {
                        Error = new Exception($"Failed to generate and encrypt the secret: {error2.Message}"),
                    };
                }

                value = secret;
            }
            else
            {
                // If the secret is not found and not created, return an error.
                return new ExpansionResult()
                {
                    Error = new KeyNotFoundException($"The secret '{name}' was not found in the env file."),
                };
            }
        }

        return new ExpansionResult()
        {
            Value = value!,
        };
    }

    private (string? Secret, Exception? Error) GenerateAndEncrypt(
        SecretExpression expression,
        Dictionary<string, string> envFile,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return (null, new OperationCanceledException("Operation was canceled."));
        }

        try
        {
            this.semaphore.Wait();
            var password = expression.Generate();
            var path = expression.Uri.LocalPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                return (null, new ArgumentException($"The path cannot be empty {expression.Uri}."));
            }

            var si = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "sops",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var args = new List<string>();
            var env = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var ageKeyFile = this.AgeKeyFile;
            var sopsConfig = this.SopsConfigPath;

            var nv = System.Web.HttpUtility.ParseQueryString(expression.Uri.Query);
            if (nv.Count > 0)
            {
                var value = nv["age-key-file"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    ageKeyFile = value;
                }

                value = nv["sops-config"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    sopsConfig = value;
                }
            }

            var dir = Path.GetDirectoryName(path);
            si.WorkingDirectory = dir!;

            if (string.IsNullOrWhiteSpace(sopsConfig))
            {
                var configFile = Path.Combine(dir!, ".sops.yaml");
                if (File.Exists(configFile))
                {
                    sopsConfig = configFile;
                }
            }

            if (!string.IsNullOrWhiteSpace(ageKeyFile))
            {
                env["SOPS_AGE_KEY_FILE"] = ageKeyFile;
            }

            if (!string.IsNullOrWhiteSpace(sopsConfig))
            {
                args.Add("--config");
                args.Add(sopsConfig);
            }

            var content = DotEnvSerializer.SerializeDictionary(envFile);
            if (string.IsNullOrWhiteSpace(content))
            {
                return (null, new ArgumentException("The content to encrypt is empty."));
            }

            if (env.Count > 0)
            {
                foreach (var (key, value) in env)
                {
                    si.Environment[key] = value;
                }
            }

            var tempDir = Path.GetTempPath();
            var tempFileName = Path.GetFileName(path);
            var tempPath = Path.Combine(tempDir, tempFileName);

            try
            {
                File.Copy(path, tempPath, true);
                File.WriteAllText(path, content);

                args.Add("-i");
                args.Add("-e");
                args.Add(path);

                var process = new System.Diagnostics.Process
                {
                    StartInfo = si,
                };

                process.Start();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    var stderr = process.StandardError.ReadToEnd();
                    if (File.Exists(tempPath))
                    {
                        File.Copy(tempPath, path, true);
                        File.Delete(tempPath);
                    }

                    return (null, new Exception($"Failed to encrypt the file: {stderr}"));
                }

                return (password, null);
            }
            catch (Exception ex)
            {
                try
                {
                    if (File.Exists(tempPath))
                    {
                        File.Copy(tempPath, path, true);
                        File.Delete(tempPath);
                    }
                }
                catch (Exception copyEx)
                {
                    return (null, new Exception($"Failed to restore the original file after encryption failure: {copyEx.Message} at {tempPath}", ex));
                }

                return (null, ex);
            }
        }
        finally
        {
            this.semaphore.Release();
        }
    }

    private async Task<(string? Secret, Exception? Error)> GenerateAndEncryptAsync(
        SecretExpression expression,
        Dictionary<string, string> envFile,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return (null, new OperationCanceledException("Operation was canceled."));
        }

        try
        {
            await this.semaphore.WaitAsync(cancellationToken);
            var password = expression.Generate();
            var path = expression.Uri.LocalPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                return (null, new ArgumentException($"The path cannot be empty {expression.Uri}."));
            }

            var si = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "sops",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var args = new List<string>();
            var env = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var ageKeyFile = this.AgeKeyFile;
            var sopsConfig = this.SopsConfigPath;

            var nv = System.Web.HttpUtility.ParseQueryString(expression.Uri.Query);
            if (nv.Count > 0)
            {
                var value = nv["age-key-file"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    ageKeyFile = value;
                }

                value = nv["sops-config"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    sopsConfig = value;
                }
            }

            var dir = Path.GetDirectoryName(path);
            si.WorkingDirectory = dir!;

            if (string.IsNullOrWhiteSpace(sopsConfig))
            {
                var configFile = Path.Combine(dir!, ".sops.yaml");
                if (File.Exists(configFile))
                {
                    sopsConfig = configFile;
                }
            }

            if (!string.IsNullOrWhiteSpace(ageKeyFile))
            {
                env["SOPS_AGE_KEY_FILE"] = ageKeyFile;
            }

            if (!string.IsNullOrWhiteSpace(sopsConfig))
            {
                args.Add("--config");
                args.Add(sopsConfig);
            }

            var content = DotEnvSerializer.SerializeDictionary(envFile);
            if (string.IsNullOrWhiteSpace(content))
            {
                return (null, new ArgumentException("The content to encrypt is empty."));
            }

            if (env.Count > 0)
            {
                foreach (var (key, value) in env)
                {
                    si.Environment[key] = value;
                }
            }

            var tempDir = Path.GetTempPath();
            var tempFileName = Path.GetFileName(path);
            var tempPath = Path.Combine(tempDir, tempFileName);

            try
            {
                File.Copy(path, tempPath, true);
                await File.WriteAllTextAsync(path, content, cancellationToken);

                args.Add("-i");
                args.Add("-e");
                args.Add(path);

                var process = new System.Diagnostics.Process
                {
                    StartInfo = si,
                };

                process.Start();
                await process.WaitForExitAsync(cancellationToken);
                if (process.ExitCode != 0)
                {
                    var stderr = await process.StandardError.ReadToEndAsync();
                    if (File.Exists(tempPath))
                    {
                        File.Copy(tempPath, path, true);
                        File.Delete(tempPath);
                    }

                    return (null, new Exception($"Failed to encrypt the file: {stderr}"));
                }

                return (password, null);
            }
            catch (Exception ex)
            {
                try
                {
                    if (File.Exists(tempPath))
                    {
                        File.Copy(tempPath, path, true);
                        File.Delete(tempPath);
                    }
                }
                catch (Exception copyEx)
                {
                    return (null, new Exception($"Failed to restore the original file after encryption failure: {copyEx.Message} at {tempPath}", ex));
                }

                return (null, ex);
            }
        }
        finally
        {
            this.semaphore.Release();
        }
    }

    private (Dictionary<string, string>?, Exception?) GetEnvFile(
        SecretExpression expression)
    {
        if (this.envFiles.ContainsKey(expression.Uri.ToString()))
        {
            return (this.envFiles[expression.Uri.ToString()], null);
        }

        this.semaphore.Wait();
        try
        {
            var path = expression.Uri.LocalPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                return (null, new ArgumentException($"The path cannot be empty {expression.Uri}."));
            }

            if (!File.Exists(path))
            {
                return (null, new FileNotFoundException($"The file does not exist: {path}"));
            }

            var si = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "sops",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            var args = new List<string>();
            var env = new Dictionary<string, string>();
            var query = expression.Uri.Query;
            var ageKeyFile = this.AgeKeyFile;
            var sopsConfig = this.SopsConfigPath;

            if (!string.IsNullOrWhiteSpace(query))
            {
                var nv = System.Web.HttpUtility.ParseQueryString(query);
                var value = nv["age-key-file"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    ageKeyFile = value;
                }

                value = nv["sops-config"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    sopsConfig = value;
                }
            }

            if (!string.IsNullOrWhiteSpace(ageKeyFile))
            {
                env["SOPS_AGE_KEY_FILE"] = ageKeyFile;
            }

            if (env.Count > 0)
            {
                foreach (var (key, value) in env)
                {
                    si.Environment[key] = value;
                }
            }

            var dir = Path.GetDirectoryName(path);
            si.WorkingDirectory = dir!;

            if (string.IsNullOrWhiteSpace(sopsConfig))
            {
                var sopsConfigPath = Path.Combine(dir!, ".sops.yaml");
                if (File.Exists(sopsConfigPath))
                {
                    sopsConfig = sopsConfigPath;
                }
            }

            if (!string.IsNullOrWhiteSpace(sopsConfig))
            {
                args.Add("--config");
                args.Add(sopsConfig);
            }

            args.Add("-d");
            args.Add(path);

            var process = new System.Diagnostics.Process
            {
                StartInfo = si,
            };

            process.Start();
            process.WaitForExit();

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            if (process.ExitCode != 0)
            {
                return (null, new Exception($"Failed to decrypt the file: {error}"));
            }

            var content = output;
            if (string.IsNullOrWhiteSpace(content))
            {
                return (null, new ArgumentException("The decrypted content is empty."));
            }

            var envFile = DotEnvSerializer.DeserializeDocument(content);
            if (envFile is null)
            {
                return (null, new ArgumentException("Failed to parse the decrypted content as an env file."));
            }

            var map = envFile.ToDictionary();
            this.envFiles.TryAdd(expression.Uri.ToString(), map);

            return (map, null);
        }
        finally
        {
            this.semaphore.Release();
        }
    }

    private async Task<(Dictionary<string, string>?, Exception?)> GetEnvFileAsync(
        SecretExpression expression,
        CancellationToken cancellationToken = default)
    {
        if (this.envFiles.ContainsKey(expression.Uri.ToString()))
        {
            return (this.envFiles[expression.Uri.ToString()], null);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return (null, new OperationCanceledException("Operation was canceled."));
        }

        await this.semaphore.WaitAsync(cancellationToken);
        try
        {
            var path = expression.Uri.LocalPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                return (null, new ArgumentException($"The path cannot be empty {expression.Uri}."));
            }

            if (!File.Exists(path))
            {
                return (null, new FileNotFoundException($"The file does not exist: {path}"));
            }

            var si = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "sops",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            var args = new List<string>();
            var env = new Dictionary<string, string>();
            var query = expression.Uri.Query;
            var ageKeyFile = this.AgeKeyFile;
            var sopsConfig = this.SopsConfigPath;

            if (!string.IsNullOrWhiteSpace(query))
            {
                var nv = System.Web.HttpUtility.ParseQueryString(query);
                var value = nv["age-key-file"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    ageKeyFile = value;
                }

                value = nv["sops-config"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    sopsConfig = value;
                }
            }

            if (!string.IsNullOrWhiteSpace(ageKeyFile))
            {
                env["SOPS_AGE_KEY_FILE"] = ageKeyFile;
            }

            if (env.Count > 0)
            {
                foreach (var (key, value) in env)
                {
                    si.Environment[key] = value;
                }
            }

            var dir = Path.GetDirectoryName(path);
            si.WorkingDirectory = dir!;

            if (string.IsNullOrWhiteSpace(sopsConfig))
            {
                var sopsConfigPath = Path.Combine(dir!, ".sops.yaml");
                if (File.Exists(sopsConfigPath))
                {
                    sopsConfig = sopsConfigPath;
                }
            }

            if (!string.IsNullOrWhiteSpace(sopsConfig))
            {
                args.Add("--config");
                args.Add(sopsConfig);
            }

            args.Add("-d");
            args.Add(path);

            var process = new System.Diagnostics.Process
            {
                StartInfo = si,
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            if (process.ExitCode != 0)
            {
                return (null, new Exception($"Failed to decrypt the file: {error}"));
            }

            var content = output;
            if (string.IsNullOrWhiteSpace(content))
            {
                return (null, new ArgumentException("The decrypted content is empty."));
            }

            var envFile = DotEnvSerializer.DeserializeDocument(content);
            if (envFile is null)
            {
                return (null, new ArgumentException("Failed to parse the decrypted content as an env file."));
            }

            var map = envFile.ToDictionary();
            this.envFiles.TryAdd(expression.Uri.ToString(), map);

            return (map, null);
        }
        finally
        {
            this.semaphore.Release();
        }
    }
}