# Hyprx.DotEnv.Expansion

## Overview

Expands environment variables and sub commands found in .env files.

The library also enables implementing the ISecretVaultExpander interface where
to enable using vaults like azure keyvault, sops, vault, aws secrets manager. etc.

This library currently requires the use of Hyprx.DotEnv and the `DotEnvDocument`
object type.

## Summary Usage

```csharp
var expander = new ExpansionBuilder()
    .WithExpression()
    .AddSopsCliAgeEnv();


var content = """
USER=$(whoami)
HOME_PATH="$HOME"

# this will sent to the SopsCliAgeEnvExpander to handle converting the command
# and then pulling values from the sops/age encrypted env file.
VAR_FROM_SOPS="$(secret sops-age-env:///path/to/file --name SUPER_SECRET)"
""";

var env = DotEnvSerializer.SerializeDocument(contenxt);
Console.WriteLine(env["USER"]); // $(whoami)
Console.WriteLine(env["HOME_PATH"]); // $HOME

var result = expander.Expand(env);
if (result.IsError)
{
    // handle errors
    foreach(var error in result.Errors)
    {
        Console.WriteLine(error.Message);
    }
}

Console.WriteLine(env["USER"]) // username
Console.WriteLine(env["HOME_PATH"]); // /home/username

```

## Variable Expansion

### Defaults

- `$HOME` is expanded to the home environment variable.
- `${HOME}` is expanded to the home environment variable.
- `${HOME:-/home/userz}` is expanded if found, otherwise uses `/home/userz` for the value one time.
- `${HOME:=/home/userz}` is expanded if found, otherwise sets HOME to `/home/userz` for the remaining alls to $HOME.
- `${HOME:?error message}` is expanded if found, otherwise sets the
  error message to `error message` when the variable is not found.

### UnixArgs

When the `EnableUnixArgs` option is set to `true`, then arguments passed to the program can be used
in order.

`${0}` - is expanded to `Environment.GetCommandLineArgs()[0];`

### Windows Variable Expansion

When the `EnableWindowsEnvironments` option is set to `true`, then windows style variable subsitution
is performed.

`%USERPROFILE%` is expanded to the USERPROFILE environment variable.

## Command Sub Expressions

This feature is not enabled by default.  When the `EnableSubExpressions` option is set to `true`, then
commands within the sub expression syntax `$(expression)` will be interpolated in .ENV values.

When the option is enabled, it does not use shell execution by default, which means you can't
use pipes or other syntax and can only use simple commands.

Something like the following can then be used to populate the secret.

```dotenv
MY_SECRET=$(az keyvault secret show --name "my-secret" --vault-name "my vault" --query value -o tsv)
```

When the `EnableShell` option is set to true, then shell execution is used, which enables
chaining commands using pipes and other things.   The `Shell` option specifies the shell.
Currently only bash and pwsh are supported.
