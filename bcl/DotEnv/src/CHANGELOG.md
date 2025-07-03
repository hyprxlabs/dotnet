# Hyprx.DotEnv Changelog

## 0.0.0-alpha.2

Moved variable expansion to new library and remove dependency
on Hyprx.Core.

## 0.0.0-alpha.0

An implementation of dotenv for parsing and writing .env files.

This library can:

- expand environment variables such as `MYVAR=${HOME}/.config`
- perserves the order of the environment variables defined in the file.
- can parse and write comments.
- can load multiple files and expand the environment variables.
- can handle extra parsing features such as json, yaml, and bacticks.  
- avoids reflection to help with AOT scenarios.
