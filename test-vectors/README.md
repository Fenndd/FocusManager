# Shared rule test vectors

JSON examples of rule inputs and expected results that every platform implementation runs through its own
tests (ADR-0005). The complete suite and loader belong to T022; this folder currently holds only the minimal
Windows path-compatibility examples added by T054.

## `windows-path-compat.json`

Top-level fields: `version`, `suite`, `description`, `cases`.

| Case field | Meaning |
| --- | --- |
| `id` | Unique case name. |
| `requirement` | Specification requirement the case exercises (`docs/spec/`). |
| `kind` | `folder` (folder open) or `app` (application start). |
| `pathStyle` | Path semantics of every path in the case: `windows` (`\` and `/` separators, drive/UNC roots, case-insensitive) or `unix` (`/` only, case-sensitive). All current cases are `windows`. |
| `allowed` | The whitelist: entries with `path`, and for folders `includeSubfolders`. |
| `input` | The candidate folder path or executable path. |
| `expected` | `allow` or `deny`. |
| `hostContext` | Optional. Present only when a stored value is not fully qualified and so depends on the Windows host (`currentDrive`, and `currentDirectories` mapping a drive such as `D:` to its current directory; a drive not listed has its root). Cases without it must give the same result with pure string path rules on any host OS. |
| `note` | Optional explanation. |

Consumers: `tests/Shared/FocusManager.Core.Tests` (cases without `hostContext`, through `PathRules.Windows`) and
`tests/Windows/FocusManager.Platform.Windows.Tests` (all cases, through `WindowsHostPathRules` with a synthetic host built from `hostContext`).

Expected results are those of the evaluator before the T001 refactor, which resolved paths with `Path.GetFullPath`
(recorded on Windows / .NET 8). A rule change requires a vector change.
