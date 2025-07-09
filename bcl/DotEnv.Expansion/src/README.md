# Hyprx.DotEnv.Expansion

## Overview

Expands environment variables and provides command substitution found in .env files.

This library currently requires the use of Hyprx.DotEnv and the `DotEnvDocument`
object type.

By default, the DotEnvExpander only expands environment variables using bash
variable expansion.

You may opt into using command substitution, command substitution with bash or pwsh, or
enabling subsitution with implementations of ISecretVaultExpander when the `$(secret [OPTIONS])`
is found in a subexpression.

## Summary Usage

```csharp
// AddSopsCliAgeEnv enables pseudo secret command that tie to expanders that implement ISecretValueExpander, 
// this one adds an expander than can handle sop, age, & env files.

// WithCommandSubstitution() enables calling commands using $() syntax.  
//   by default this only handles single commands, if you need something that handles
//   pipes then use  .WithEnableShell(true, "bash") to call it as a bash sub expression.

var expander = new ExpansionBuilder()
    .AddSopsCliAgeEnv() 
    .WithCommandSubstitution()
    .Build();


var content = """
USER=$(whoami)
HOME_PATH="$HOME"

# this will sent to the SopsCliAgeEnvExpander to handle converting the command
# and then pulling values from the sops/age encrypted env file.
VAR_FROM_SOPS="$(secret sops-age-env:///path/to/.env --name SUPER_SECRET)"
VAR2_FROM_SOPS=$(secret sops-age-env:///$CWD/../relative/path/.env --name OTHER_SECRET)
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

When the `EnableWindowsVariables` option is set to `true`, then windows style variable subsitution
is performed.

`%USERPROFILE%` is expanded to the USERPROFILE environment variable.

## Command Substitution

This feature is not enabled by default.  When the `EnableCommandSubstitution` option is set to `true`, then
commands within the sub expression syntax `$(expression)` will be interpolated in .ENV values.

When the option is enabled, it does not use shell execution by default, which means you can't
use pipes or other syntax and can only use simple commands.

Something like the following can then be used to populate the secret.

```dotenv
MY_SECRET=$(az keyvault secret show --name "my-secret" --vault-name "my vault" --query value -o tsv)
```

When the `EnableShell` option is set to true, then shell execution is used, which enables
chaining commands using pipes and other things.   The `USeShell` option specifies the shell.
Currently only bash and pwsh are supported.

## Secret Substitution

Currently two implementations of ISecretVaultExpander ship with this library:

- **SopsEnvExpander** which can expand variables from sops/age encrypted dotenv file.
- **AzCliKeyVaultExander** which can expand variables from the azure cli for azure keyvault.

The benefits of using implement ISecretVaultExpanders is that enable creating
secrets that do not exist and provide a uniform experience to pull a secret by primarily
do it using a URI format.

The expander is responsible normalizing the secret name.

### Shared Options & Flags

- `--name` The name of the secret.
- `--create` Create the secret if it does not exist.
- `--size [size]` The size of the secret that should be generated if missing.
- `--no-upper` Omit uppercase characters for secret generation.
- `--no-lower` Omit lowercase characters for secret generation.
- `--no-digits` Omit digits 0-9 characters for secret generation.
- `--no-special` Omit special characters for secret generation.
- `--special [special]`  The special characters to use for secret generation.
- `--expires-at [time]` Sets the expiration date if the vault supports it. Should be in a format
  like `2024-12-31T23:59:59Z`

### Sops DotEnv Expander

NOTE: this currently only supports encrypt/decrypting dotenv files
using AGE.

syntax:

```bash
$(secret sops-env:///absolute/path/to/.env --name VAR_NAME)
$(secret sops-env:///$PWD/../relative/.env --name VAR_NAME)
$(secret sops-env:///absolute/path/to/.env --name MISSING_SECRET --create --size 20)
```

Inside of a .env file it would look like:

```dotenv
SECRET_ONE=$(secret sops-env:///absolute/path/to/.env --name VAR_NAME)
SECRET_TWO=$(secret sops-env:///$PWD/../relative/.env --name VAR_NAME)
SECRET_THREE=$(secret sops-env:///absolute/path/to/.env --name MISSING_SECRET --create --size 20)
```

The use of the $PWD or $CWD variable is required for relative paths as URIs still need a root path
to resolve the relative path.

The expander takes additional arguments:

- `--sops-config`  the path to the sops configuration file.
- `--age-recipients`  The age public keys delimited by semicolon.
- `--age-key-file`  The path to the private age key file to use.
- `--age-key` The name of the environemnt variable that holds the age private key.

### Azure KeyVault Expander

NOTE: this expander expects you to be logged into the azure cli before calling it.

syntax:

```bash
$(secret akv://vault-name --name var-name)
$(secret akv://vault-name --name missing-secret --create --size 20)
```

Inside of a .env file it would look like

```dotenv
SECRET_ONE=$(secret akv://vault-name --name var-name)
SECRET_TWO=$(secret akv://vault-name --name missing-secret --create --size 20)
```

## Notes

The command substitution is primary for the use case of automation outside of
desktop and web apps such as scripts, clis, and devops pipelines.
