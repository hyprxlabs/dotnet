using System.Diagnostics;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

using Hyprx.DotEnv.Documents;
using Hyprx.DotEnv.Expansion;

namespace Hyprx.DotEnv.Expansion;

public class DotEnvExpander
{
    private readonly ExpansionOptions options;

    public DotEnvExpander(ExpansionOptions? options = null)
    {
        this.options = options ?? new ExpansionOptions();
    }

    public async Task<ExpansionSummary> ExpandAsync(
        DotEnvDocument document,
        CancellationToken cancellationToken = default)
    {
        var summary = new ExpansionSummary(document);

        var l = document.Count;

        for (var i = 0; i < l; i++)
        {
            var node = document[i];
            if (node is DotEnvEntry entry)
            {
                var result = await ExpandValueAsync(document, i, this.options, cancellationToken);
                if (result.IsOk)
                {
                    entry.SetRawValue(result.Value);
                    continue;
                }

                summary.Errors.Add(new DotEnvExpansionError
                {
                    Message = result.Error?.Message ?? "Unknown error during expansion.",
                    Exception = result.Error,
                    Index = i,
                    Key = entry.Name,
                    Position = result.Position,
                });
            }
        }

        return summary;
    }

    public ExpansionSummary Expand(
        DotEnvDocument document)
    {
        var summary = new ExpansionSummary(document);

        var l = document.Count;
        for (var i = 0; i < l; i++)
        {
            var node = document[i];
            if (node is DotEnvEntry entry)
            {
                var result = ExpandValue(document, i, this.options);
                if (result.IsOk)
                {
                    entry.SetRawValue(result.Value);
                    continue;
                }

                summary.Errors.Add(new DotEnvExpansionError
                {
                    Message = result.Error?.Message ?? "Unknown error during expansion.",
                    Exception = result.Error,
                    Index = i,
                    Key = entry.Name,
                    Position = result.Position,
                });
            }
        }

        return summary;
    }

    private enum TokenKind
    {
        None,
        Windows,
        BashVariable,
        BashInterpolation,
        Expression,
    }

    private static async Task<ExpansionResult> ExpandValueAsync(
        DotEnvDocument doc,
        int index,
        ExpansionOptions o,
        CancellationToken cancellationToken = default)
    {
        var item = doc[index];
        if (item is not DotEnvEntry entry)
        {
            return new ExpansionResult
            {
                Error = new Exception($"Invalid entry at index {index}. Expected a DotEnvEntry, but found {item.GetType().Name}."),
                Position = 0,
            };
        }

        var template = entry.Value;
        if (string.IsNullOrEmpty(template))
        {
            return new ExpansionResult
            {
                Value = string.Empty,
                Position = 0,
            };
        }

        string? GetValue(string name)
        {
            foreach (var node in doc)
            {
                if (node is DotEnvEntry entry && entry.Name == name)
                {
                    return entry.Value;
                }
            }

            if (o.Variables.TryGetValue(name, out var value))
            {
                return value;
            }

            return Environment.GetEnvironmentVariable(name);
        }

        void SetValue(string name, string value)
        {
            for (var i = 0; i < doc.Count; i++)
            {
                if (doc[i] is DotEnvEntry entry && entry.Name == name)
                {
                    doc[i] = new DotEnvEntry(name, value);
                    return;
                }
            }

            doc.Add(new DotEnvEntry(name, value));
        }

        var tokenBuilder = StringBuilderCache.Acquire();
        var output = StringBuilderCache.Acquire();
        var kind = TokenKind.None;
        var remaining = template.Length;
        var lastTokenStartIndex = 0;
        for (var i = 0; i < template.Length; i++)
        {
            remaining--;
            var c = template[i];
            if (kind == TokenKind.None)
            {
                if (o.EnableWindowsVariables && c is '%')
                {
                    lastTokenStartIndex = i;
                    kind = TokenKind.Windows;
                    continue;
                }

                var z = i + 1;
                var next = char.MinValue;
                if (z < template.Length)
                    next = template[z];

                // escape the $ character.
                if (c is '\\' && next is '$')
                {
                    output.Append(next);
                    i++;
                    continue;
                }

                if (c is '$')
                {
                    // can't be a variable if there is no next character.
                    if (next is '{' && remaining > 3)
                    {
                        lastTokenStartIndex = i;
                        kind = TokenKind.BashInterpolation;
                        i++;
                        remaining--;
                        continue;
                    }

                    if (o.EnableSubExpressions && next is '(' && remaining > 3)
                    {
                        lastTokenStartIndex = i;
                        kind = TokenKind.Expression;
                        i++;
                        remaining--;
                        continue;
                    }

                    // only a variable if the next character is a letter.
                    if (remaining > 0 && char.IsLetterOrDigit(next))
                    {
                        lastTokenStartIndex = i;
                        kind = TokenKind.BashVariable;
                        continue;
                    }
                }

                output.Append(c);
                continue;
            }

            if (kind == TokenKind.Windows && c is '%')
            {
                if (tokenBuilder.Length == 0)
                {
                    // consecutive %, so just append both characters.
                    output.Append('%', 2);
                    continue;
                }

                var key = tokenBuilder.ToString();
                var replacement = GetValue(key);
                if (replacement is not null && replacement.Length > 0)
                    output.Append(replacement);
                tokenBuilder.Clear();
                kind = TokenKind.None;
                continue;
            }

            if (kind == TokenKind.Expression && c is ')')
            {
                if (tokenBuilder.Length == 0)
                {
                    // with bash '$()' is a bad substitution.
                    return new ExpansionResult
                    {
                        Error = new Exception("$() is a bad substitution. Expression not provided."),
                        Position = i,
                    };
                }

                var expression = tokenBuilder.ToString();
                tokenBuilder.Clear();
                kind = TokenKind.None;

                var args = SecretExpression.ParseArgs(expression);
                if (args.Count > 0 && args[0] == "secret")
                {
                    if (o.SecretVaultExpanders.Count == 0)
                    {
                        return new ExpansionResult
                        {
                            Value = template,
                            Position = i,
                            Error = new Exception("No secret vault expanders configured."),
                        };
                    }

                    foreach (var expander in o.SecretVaultExpanders)
                        {
                            try
                            {
                                if (!expander.CanHandle(args))
                                    continue;

                                var result = await expander.ExpandAsync(args, cancellationToken);
                                if (result.IsOk)
                                {
                                    output.Append(result.Value);
                                    break;
                                }
                                else
                                {
                                    return new ExpansionResult
                                    {
                                        Error = result.Error,
                                        Position = i,
                                    };
                                }
                            }
                            catch (Exception ex)
                            {
                                return new ExpansionResult
                                {
                                    Error = ex,
                                    Position = i,
                                };
                            }
                        }
                }

                if (o.EnableShell)
                {
                    var shell = o.Shell;
                    if (string.IsNullOrEmpty(shell))
                    {
                        shell = "bash";
                    }

                    var si = new ProcessStartInfo
                    {
                        FileName = shell,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    };

                    if (shell.EqualsFold("pwsh") || shell.EqualsFold("powershell"))
                    {
                        si.ArgumentList.Add("-NoLogo");
                        si.ArgumentList.Add("-NoProfile");
                        si.ArgumentList.Add("-NonInteractive");
                        si.ArgumentList.Add("-ExecutionPolicy");
                        si.ArgumentList.Add("Bypass");
                        si.ArgumentList.Add("-Command");
                        si.ArgumentList.Add(expression);
                    }
                    else
                    {
                        // bash "-noprofile", "--norc", "-e", "-o", "pipefail" -c command
                        si.ArgumentList.Add("-noprofile");
                        si.ArgumentList.Add("--norc");
                        si.ArgumentList.Add("-e");
                        si.ArgumentList.Add("-o");
                        si.ArgumentList.Add("pipefail");
                        si.ArgumentList.Add("-c");
                        si.ArgumentList.Add(expression);
                    }

                    var process = new Process
                    {
                        StartInfo = si,
                    };

                    process.Start();
                    await process.WaitForExitAsync(cancellationToken);
                    var stdout = await process.StandardOutput.ReadToEndAsync();
                    output.Append(stdout.Trim());
                }
                else
                {

                    var exe = args[0];
                    args.RemoveAt(0);

                    var si = new ProcessStartInfo
                    {
                        FileName = exe,
                    };

                    foreach (var arg in args)
                    {
                        si.ArgumentList.Add(arg);
                    }

                    si.UseShellExecute = false;
                    si.RedirectStandardOutput = true;
                    si.RedirectStandardError = true;
                    si.CreateNoWindow = true;

                    var process = new Process
                    {
                        StartInfo = si,
                    };

                    process.Start();
                    await process.WaitForExitAsync(cancellationToken);
                    var stdout = await process.StandardOutput.ReadToEndAsync();
                    output.Append(stdout.Trim());
                }
            }

            if (kind == TokenKind.BashInterpolation && c is '}')
            {
                if (tokenBuilder.Length == 0)
                {
                    // with bash '${}' is a bad substitution.
                    return new ExpansionResult
                    {
                        Error = new Exception("${} is a bad substitution. Variable name not provided."),
                        Position = lastTokenStartIndex,
                    };
                }

                var substitution = tokenBuilder.ToString();
                string key = substitution;
                string defaultValue = string.Empty;
                string? message = null;
                if (substitution.Contains(":-"))
                {
                    var parts = substitution.Split([':', '-'], StringSplitOptions.RemoveEmptyEntries);
                    key = parts[0];
                    defaultValue = parts[1];
                }
                else if (substitution.Contains(":="))
                {
                    var parts = substitution.Split([':', '='], StringSplitOptions.RemoveEmptyEntries);
                    key = parts[0];
                    defaultValue = parts[1];
                    var v = GetValue(key);
                    if (v is null)
                        SetValue(key, defaultValue);
                }
                else if (substitution.Contains(":?"))
                {
                    var parts = substitution.Split([':', '?'], StringSplitOptions.RemoveEmptyEntries);
                    key = parts[0];
                    message = parts[1];
                }
                else if (substitution.Contains(":"))
                {
                    var parts = substitution.Split(':', StringSplitOptions.RemoveEmptyEntries);
                    key = parts[0];
                    defaultValue = parts[1];
                }

                if (key.Length == 0)
                {
                    message = "Bad substitution, empty variable name.";
                }

                if (!IsValidBashVariable(key.AsSpan()))
                {
                    message = $"Bad substitution, invalid variable name {key}.";
                }

                var replacement = GetValue(key);
                if (replacement is not null)
                {
                    output.Append(replacement);
                }
                else if (message is not null)
                {
                    return new ExpansionResult
                    {
                        Error = new Exception(message),
                        Position = lastTokenStartIndex,
                    };
                }
                else if (o.Variables.TryGetValue(key, out var variableValue) && variableValue.Length > 0)
                {
                    output.Append(variableValue);
                }
                else if (defaultValue.Length > 0)
                {
                    output.Append(defaultValue);
                }
                else
                {
                    return new ExpansionResult
                    {
                        Error = new Exception($"Bad substitution, variable {key} is not set."),
                        Position = lastTokenStartIndex,
                    };
                }

                tokenBuilder.Clear();
                kind = TokenKind.None;
                continue;
            }

            if (kind == TokenKind.BashVariable && (!(char.IsLetterOrDigit(c) || c is '_') || remaining == 0))
            {
                // '\' is used to escape the next character, so don't append it.
                // its used to escape a name like $HOME\\_TEST where _TEST is not
                // part of the variable name.
                bool append = c is not '\\';

                if (remaining == 0 && (char.IsLetterOrDigit(c) || c is '_'))
                {
                    append = false;
                    tokenBuilder.Append(c);
                }

                // rewind one character. Let the previous block handle $ for the next variable
                if (c is '$')
                {
                    append = false;
                    i--;
                }

                var key = tokenBuilder.ToString();
                if (key.Length == 0)
                {
                    return new ExpansionResult
                    {
                        Error = new Exception("Bad substitution, empty variable name."),
                        Position = lastTokenStartIndex,
                    };
                }

                if (o.EnableUnixArgs && int.TryParse(key, out var argIndex))
                {
                    if (argIndex < 0 || argIndex >= Environment.GetCommandLineArgs().Length)
                    {
                        return new ExpansionResult
                        {
                            Error = new Exception($"Bad substitution, invalid index {argIndex}."),
                            Position = i,
                        };
                    }

                    output.Append(Environment.GetCommandLineArgs()[index]);
                    if (append)
                        output.Append(c);

                    tokenBuilder.Clear();
                    kind = TokenKind.None;
                    continue;
                }

                if (!IsValidBashVariable(key.AsSpan()))
                {
                    return new ExpansionResult
                    {
                        Error = new Exception($"Bad substitution, invalid variable name {key}."),
                        Position = i,
                    };
                }

                var replacement = GetValue(key);
                if (replacement is not null && replacement.Length > 0)
                    output.Append(replacement);

                if (replacement is null)
                {
                    return new ExpansionResult
                    {
                        Error = new Exception($"Bad substitution, variable {key} is not set."),
                        Position = lastTokenStartIndex,
                    };
                }

                if (append)
                    output.Append(c);

                tokenBuilder.Clear();
                kind = TokenKind.None;
                continue;
            }

            tokenBuilder.Append(c);
            if (remaining == 0)
            {
                if (kind is TokenKind.Windows)
                {
                    return new ExpansionResult
                    {
                        Error = new Exception("Bad substitution, missing closing token '%'."),
                        Position = lastTokenStartIndex,
                    };
                }

                if (kind is TokenKind.BashInterpolation)
                {
                    return new ExpansionResult
                    {
                        Error = new Exception("Bad substitution, missing closing token '}'."),
                        Position = lastTokenStartIndex,
                    };
                }

                if (kind is TokenKind.Expression)
                {
                    return new ExpansionResult
                    {
                        Error = new Exception("Bad substitution, missing closing token ')'."),
                        Position = lastTokenStartIndex,
                    };
                }
            }
        }

        var value = StringBuilderCache.GetStringAndRelease(output);
        return new ExpansionResult
        {
            Value = value,
            Position = 0,
        };
    }

    private static ExpansionResult ExpandValue(
        DotEnvDocument doc,
        int index,
        ExpansionOptions o)
    {
        var item = doc[index];
        if (item is not DotEnvEntry entry)
        {
            return new ExpansionResult
            {
                Error = new Exception($"Invalid entry at index {index}. Expected a DotEnvEntry, but found {item.GetType().Name}."),
                Position = 0,
            };
        }

        var template = entry.Value;
        if (string.IsNullOrEmpty(template))
        {
            return new ExpansionResult
            {
                Value = string.Empty,
                Position = 0,
            };
        }

        string? GetValue(string name)
        {
            foreach (var node in doc)
            {
                if (node is DotEnvEntry entry && entry.Name == name)
                {
                    return entry.Value;
                }
            }

            if (o.Variables.TryGetValue(name, out var value))
            {
                return value;
            }

            return Environment.GetEnvironmentVariable(name);
        }

        void SetValue(string name, string value)
        {
            for (var i = 0; i < doc.Count; i++)
            {
                if (doc[i] is DotEnvEntry entry && entry.Name == name)
                {
                    doc[i] = new DotEnvEntry(name, value);
                    return;
                }
            }

            doc.Add(new DotEnvEntry(name, value));
        }

        var tokenBuilder = StringBuilderCache.Acquire();
        var output = StringBuilderCache.Acquire();
        var kind = TokenKind.None;
        var remaining = template.Length;
        var lastTokenStartIndex = 0;
        for (var i = 0; i < template.Length; i++)
        {
            remaining--;
            var c = template[i];
            if (kind == TokenKind.None)
            {
                if (o.EnableWindowsVariables && c is '%')
                {
                    lastTokenStartIndex = i;
                    kind = TokenKind.Windows;
                    continue;
                }

                var z = i + 1;
                var next = char.MinValue;
                if (z < template.Length)
                    next = template[z];

                // escape the $ character.
                if (c is '\\' && next is '$')
                {
                    output.Append(next);
                    i++;
                    continue;
                }

                if (c is '$')
                {
                    // can't be a variable if there is no next character.
                    if (next is '{' && remaining > 3)
                    {
                        lastTokenStartIndex = i;
                        kind = TokenKind.BashInterpolation;
                        i++;
                        remaining--;
                        continue;
                    }

                    if (o.EnableSubExpressions && next is '(' && remaining > 3)
                    {
                        lastTokenStartIndex = i;
                        kind = TokenKind.Expression;
                        i++;
                        remaining--;
                        continue;
                    }

                    // only a variable if the next character is a letter.
                    if (remaining > 0 && char.IsLetterOrDigit(next))
                    {
                        lastTokenStartIndex = i;
                        kind = TokenKind.BashVariable;
                        continue;
                    }
                }

                output.Append(c);
                continue;
            }

            if (kind == TokenKind.Windows && c is '%')
            {
                if (tokenBuilder.Length == 0)
                {
                    // consecutive %, so just append both characters.
                    output.Append('%', 2);
                    continue;
                }

                var key = tokenBuilder.ToString();
                var replacement = GetValue(key);
                if (replacement is not null && replacement.Length > 0)
                    output.Append(replacement);
                tokenBuilder.Clear();
                kind = TokenKind.None;
                continue;
            }

            if (kind == TokenKind.Expression && c is ')')
            {
                if (tokenBuilder.Length == 0)
                {
                    // with bash '$()' is a bad substitution.
                    return new ExpansionResult
                    {
                        Error = new Exception("$() is a bad substitution. Expression not provided."),
                        Position = i,
                    };
                }

                var expression = tokenBuilder.ToString();
                tokenBuilder.Clear();
                kind = TokenKind.None;

                var args = SecretExpression.ParseArgs(expression);
                var vaultExpanderHandled = false;
                if (args.Count > 0 && args[0] == "secret")
                {
                    if (o.SecretVaultExpanders.Count == 0)
                    {
                        return new ExpansionResult
                        {
                            Value = template,
                            Error = new Exception("No secret vault expanders configured."),
                            Position = i,
                        };
                    }

                    foreach (var expander in o.SecretVaultExpanders)
                    {
                        try
                        {
                            if (!expander.CanHandle(args))
                                continue;

                            var result = expander.Expand(args);
                            if (result.IsOk)
                            {
                                output.Append(result.Value);
                                vaultExpanderHandled = true;
                                break;
                            }
                            else
                            {
                                return new ExpansionResult
                                {
                                    Error = result.Error,
                                    Position = i,
                                };
                            }
                        }
                        catch (Exception ex)
                        {
                            return new ExpansionResult
                            {
                                Error = ex,
                                Position = i,
                            };
                        }
                    }
                }

                if (vaultExpanderHandled)
                {
                    kind = TokenKind.None;
                    tokenBuilder.Clear();
                    continue;
                }

                if (o.EnableShell)
                {
                    var shell = o.Shell;
                    if (string.IsNullOrEmpty(shell))
                    {
                        shell = "bash";
                    }

                    var si = new ProcessStartInfo
                    {
                        FileName = shell,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    };

                    if (shell.EqualsFold("pwsh") || shell.EqualsFold("powershell"))
                    {
                        si.ArgumentList.Add("-NoLogo");
                        si.ArgumentList.Add("-NoProfile");
                        si.ArgumentList.Add("-NonInteractive");
                        si.ArgumentList.Add("-ExecutionPolicy");
                        si.ArgumentList.Add("Bypass");
                        si.ArgumentList.Add("-Command");
                        si.ArgumentList.Add(expression);
                    }
                    else
                    {
                        // bash "-noprofile", "--norc", "-e", "-o", "pipefail" -c command
                        si.ArgumentList.Add("-noprofile");
                        si.ArgumentList.Add("--norc");
                        si.ArgumentList.Add("-e");
                        si.ArgumentList.Add("-o");
                        si.ArgumentList.Add("pipefail");
                        si.ArgumentList.Add("-c");
                        si.ArgumentList.Add(expression);
                    }

                    var process = new Process
                    {
                        StartInfo = si,
                    };

                    process.Start();
                    process.WaitForExit();
                    var stdout = process.StandardOutput.ReadToEnd().Trim();
                    output.Append(stdout);
                }
                else
                {
                    var exe = args[0];
                    args.RemoveAt(0);

                    var si = new ProcessStartInfo
                    {
                        FileName = exe,
                    };

                    si.UseShellExecute = o.EnableShell;
                    if (!o.EnableShell)
                    {
                        foreach (var arg in args)
                        {
                            si.ArgumentList.Add(arg);
                        }
                    }
                    else
                    {
                        si.Arguments = expression;
                    }

                    si.RedirectStandardOutput = true;
                    si.RedirectStandardError = true;
                    si.CreateNoWindow = true;

                    var process = new Process
                    {
                        StartInfo = si,
                    };

                    process.Start();
                    process.WaitForExit();
                    var stdout = process.StandardOutput.ReadToEnd().Trim();
                    output.Append(stdout);
                }

                kind = TokenKind.None;
                tokenBuilder.Clear();
                continue;
            }

            if (kind == TokenKind.BashInterpolation && c is '}')
            {
                if (tokenBuilder.Length == 0)
                {
                    // with bash '${}' is a bad substitution.
                    return new ExpansionResult
                    {
                        Error = new Exception("${} is a bad substitution. Variable name not provided."),
                        Position = lastTokenStartIndex,
                    };
                }

                var substitution = tokenBuilder.ToString();
                string key = substitution;
                string defaultValue = string.Empty;
                string? message = null;
                if (substitution.Contains(":-"))
                {
                    var parts = substitution.Split([':', '-'], StringSplitOptions.RemoveEmptyEntries);
                    key = parts[0];
                    defaultValue = parts[1];
                }
                else if (substitution.Contains(":="))
                {
                    var parts = substitution.Split([':', '='], StringSplitOptions.RemoveEmptyEntries);
                    key = parts[0];
                    defaultValue = parts[1];
                    var v = GetValue(key);
                    if (v is null)
                        SetValue(key, defaultValue);
                }
                else if (substitution.Contains(":?"))
                {
                    var parts = substitution.Split([':', '?'], StringSplitOptions.RemoveEmptyEntries);
                    key = parts[0];
                    message = parts[1];
                }
                else if (substitution.Contains(":"))
                {
                    var parts = substitution.Split(':', StringSplitOptions.RemoveEmptyEntries);
                    key = parts[0];
                    defaultValue = parts[1];
                }

                if (key.Length == 0)
                {
                    message = "Bad substitution, empty variable name.";
                }

                if (!IsValidBashVariable(key.AsSpan()))
                {
                    message = $"Bad substitution, invalid variable name {key}.";
                }

                var replacement = GetValue(key);
                if (replacement is not null)
                {
                    output.Append(replacement);
                }
                else if (message is not null)
                {
                    return new ExpansionResult
                    {
                        Error = new Exception(message),
                        Position = lastTokenStartIndex,
                    };
                }
                else if (o.Variables.TryGetValue(key, out var variableValue) && variableValue.Length > 0)
                {
                    output.Append(variableValue);
                }
                else if (defaultValue.Length > 0)
                {
                    output.Append(defaultValue);
                }
                else
                {
                    return new ExpansionResult
                    {
                        Error = new Exception($"Bad substitution, variable {key} is not set."),
                        Position = lastTokenStartIndex,
                    };
                }

                tokenBuilder.Clear();
                kind = TokenKind.None;
                continue;
            }

            if (kind == TokenKind.BashVariable && (!(char.IsLetterOrDigit(c) || c is '_') || remaining == 0))
            {
                // '\' is used to escape the next character, so don't append it.
                // its used to escape a name like $HOME\\_TEST where _TEST is not
                // part of the variable name.
                bool append = c is not '\\';

                if (remaining == 0 && (char.IsLetterOrDigit(c) || c is '_'))
                {
                    append = false;
                    tokenBuilder.Append(c);
                }

                // rewind one character. Let the previous block handle $ for the next variable
                if (c is '$')
                {
                    append = false;
                    i--;
                }

                var key = tokenBuilder.ToString();
                if (key.Length == 0)
                {
                    return new ExpansionResult
                    {
                        Error = new Exception("Bad substitution, empty variable name."),
                        Position = lastTokenStartIndex,
                    };
                }

                if (o.EnableUnixArgs && int.TryParse(key, out var argIndex))
                {
                    if (argIndex < 0 || argIndex >= Environment.GetCommandLineArgs().Length)
                    {
                        return new ExpansionResult
                        {
                            Error = new Exception($"Bad substitution, invalid index {argIndex}."),
                            Position = i,
                        };
                    }

                    output.Append(Environment.GetCommandLineArgs()[index]);
                    if (append)
                        output.Append(c);

                    tokenBuilder.Clear();
                    kind = TokenKind.None;
                    continue;
                }

                if (!IsValidBashVariable(key.AsSpan()))
                {
                    return new ExpansionResult
                    {
                        Error = new Exception($"Bad substitution, invalid variable name {key}."),
                        Position = i,
                    };
                }

                var replacement = GetValue(key);
                if (replacement is not null && replacement.Length > 0)
                    output.Append(replacement);

                if (replacement is null)
                {
                    return new ExpansionResult
                    {
                        Error = new Exception($"Bad substitution, variable {key} is not set."),
                        Position = lastTokenStartIndex,
                    };
                }

                if (append)
                    output.Append(c);

                tokenBuilder.Clear();
                kind = TokenKind.None;
                continue;
            }

            tokenBuilder.Append(c);
            if (remaining == 0)
            {
                if (kind is TokenKind.Windows)
                {
                    return new ExpansionResult
                    {
                        Error = new Exception("Bad substitution, missing closing token '%'."),
                        Position = lastTokenStartIndex,
                    };
                }

                if (kind is TokenKind.BashInterpolation)
                {
                    return new ExpansionResult
                    {
                        Error = new Exception("Bad substitution, missing closing token '}'."),
                        Position = lastTokenStartIndex,
                    };
                }

                if (kind is TokenKind.Expression)
                {
                    return new ExpansionResult
                    {
                        Error = new Exception("Bad substitution, missing closing token ')'."),
                        Position = lastTokenStartIndex,
                    };
                }
            }
        }

        var value = StringBuilderCache.GetStringAndRelease(output);
        return new ExpansionResult
        {
            Value = value,
            Position = 0,
        };
    }

    private static bool IsValidBashVariable(ReadOnlySpan<char> input)
    {
        for (var i = 0; i < input.Length; i++)
        {
            if (i == 0 && !char.IsLetter(input[i]))
                return false;

            if (!char.IsLetterOrDigit(input[i]) && input[i] is not '_')
                return false;
        }

        return true;
    }
}