# Hyprx DotNet Libraries

A monorepo for various Hyprx DotNet libraries.

Breaking changes may be introduced until version 1.0.

## Layout

- **bcl** Base Class Libraries
- **cmn** Shared files between projects.
- **data** Data related libraries.
- **eng** The build assets folder. similar to build.
- **lib** Dotnet libraries that generally rely on third-party dependencies outside
  of the dotnet framework.
- **rex** Rex, the cli task runner and related libraries.
- **tpl** Dotnet templates.
- **iac** Infrastructure with code.
  - **pulumi** Pulumi iac
  - **terraform** Terraform iac
