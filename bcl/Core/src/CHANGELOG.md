# Hyprx.Core Changelog

## 0.0.0-alpha.0

Initial feature set:

- Result type
- Extension methods and members under the Extras namespaces for strings,
  spans, string builder, arrays, tasks, etc.
- The static FileSystem class which provides an fs module similar
  to other std libraries including functions missing posix functions
  like copy, chown, chmod, stat, etc.
- Enhanced logic for working with environments such as expanding
  bash style variables, appending/prepending paths to the environment
  path, etc.
