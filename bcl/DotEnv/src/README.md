# Hyprx.DotEnv

## Overview

An implementation of dotenv for parsing and writing .env files.

This library can:

- expand environment variables such as `MYVAR=${HOME}/.config`
- perserves the order of the environment variables defined in the file.
- can parse and write comments.
- can load multiple files and expand the environment variables.
- can handle extra parsing features such as json, yaml, and bacticks.  
- avoids reflection to help with AOT scenarios.

## Usage

```csharp
using Hyprx.DotEnv;

var doc = DotEnvSerializer.DeserializeDocument(
    """
    # COMMENT
    KEY=VALUE

    MULTILINE=" my 
    value
    spans
    multiple
    lines"
    single='single value'
    PF="${USERPROFILE:-notfound}"
    """);

foreach(var node in doc) 
{
    if (node is DotEnvEmptyLine)
        Console.WriteLine("");

    if (node is DotEnvComment comment)
        Console.WriteLine($"#{comment.Value}");

    if (node is DotEnvEntry entry)
        Console.WriteLine($"{entry.Key}=\"{entry.Value}\"");
}

var content = DotEnvSerializer.DeserializeDocument(doc);
Console.WriteLine(content);

```
