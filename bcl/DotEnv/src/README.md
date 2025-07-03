# Hyprx.DotEnv

## Overview

An implementation of dotenv for parsing and writing .env files.

This library can:

- perserve the order of the environment variables defined in the file.
- can parse and write comments.
- can load multiple files and expand the environment variables.
- can handle extra parsing features such as json, yaml, and bacticks.  
- avoids reflection to help with AOT scenarios.

For variable and command subsitution use the "Hyprx.DotEnv.Expansion" library
which can perform variable and command subusitution.

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

var content = DotEnvSerializer.SerializeDocument(doc);
Console.WriteLine(content);

// 
// # NODE VARS
// NODE_ENV=production
var doc2 = new DotEnvDocument();
doc2.AddEmptyLine();
doc2.AddComment("NODE VARS");
doc2.Add("NODE_ENV", "production");

var content2 = DotEnvSerializer.SerializeDocument(doc);
Console.WriteLine(content2);

```

## Escaping

Escaping only works when using double quotes.

- `\b` backspace
- `\n` newline
- `\r` carriage return
- `\f` form feed
- `\v` vertical tab
- `\t` tab
- '\"` escape double quote
- '\'` escape single quote
- '\uFFFF` escape unicode characters.

## Defaults

By default, only basic dotenv parsing is performed. There are options to enable using
bacticks, json, and yaml delimiters for multiline value parsing to make things easier.

### AllowJson

The `AllowJson` option will allow using `{}` to notate the start and end of json in an
env file.

```dotenv
MY_JSON={
    "one": 1,
    "person": "bob"
    "nested": {
        "person": "alice"
    }
}
```

The above value will be capture as a multiline string.

### AllowYaml

The `AllowYaml` option will allow using `---` to notate the start and end of yaml
in an env file.

```dotenv
MY_YAML=---
one: 1
person: bob
nested:
  person: alice
---
NEXT_SECRET="test"
```

### AllowBackticks

The `AllowBackticks` options will allow using `\`` to noate the start and end
of a multiline value without using quotes.

```dotenv
MYVALUE=`this
will be "captured"`
```
