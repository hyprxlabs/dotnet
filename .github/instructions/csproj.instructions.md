---
description: 'Guidelines for building C# applications'
applyTo: '**/*.csproj'
---

# C# Project File (.csproj) Guidelines

- Use 2 spaces for indentation.
- Do not use tabs.
- For `<PackageReference>` elements, do not include the `Version` attribute
  as they are managed in the `Directory.Packages.props` file.