---
id: T001
title: Make Core path rules independent of the host OS
milestone: M1
status: ready
depends_on: []
area: ["src/Shared/FocusManager.Core/Rules/**", "src/Shared/FocusManager.Core/Abstractions/**", "src/Windows/FocusManager.Agent/Program.cs", "tests/Shared/FocusManager.Core.Tests/**", "AGENTS.md"]
---

# T001 — Make Core path rules independent of the host OS

## Goal

Make path semantics explicit in the shared desktop rule evaluator so that Windows and Unix cases pass on any host without changing Windows behavior.

## Sources

Legacy classification: type `refactor`; platform `shared`; priority `high`.

Requirement identifiers: `APP-002`, `FLD-002`, `FLD-003`.

Legacy ADR associations: ADR-0005.

[Application rules](../docs/spec/apps.md), [folder rules](../docs/spec/folders.md), and [ADR-0005](../docs/architecture/decisions/0005-native-stack-per-platform.md).

`src/Shared/FocusManager.Core/Rules/RuleEvaluator.cs` uses `Path.GetFullPath`, `Path.DirectorySeparatorChar`, and `StringComparison.OrdinalIgnoreCase`, so its semantics depend on the OS executing the code. Three Windows-path tests fail on Linux: `EvaluateApplicationStart_Allows_ByFileNameFallback_WhenFullPathUnavailable`, `EvaluateFolderOpen_Allows_SameFolderPath_IgnoringTrailingSeparator`, and `EvaluateFolderOpen_Allows_ChildFolder_WhenSubfoldersAllowed`. Core is shared by Windows and Linux agents (ADR-0005, ADR-0007); Linux uses case-sensitive paths and `/`.

## Scope

In scope:
1. Introduce explicit platform path semantics in Core, for example `PathStyle` / `IPathRules`: separators, case sensitivity, and normalization, without consulting the host OS through `System.IO.Path`.
2. Inject those semantics into `RuleEvaluator`; register Windows semantics in the Windows agent and preserve its behavior.
3. Parameterize `RuleEvaluatorTests` for Windows and Unix styles so both sets pass on any OS.
4. Remove the known-failures note from `AGENTS.md` once the failures are fixed.

Out of scope:
- Changes to rule semantics (APP-003, APP-004, SITE-002) belong to T012.
- Similar Windows-only `NormalizePath` methods in `AppEnforcer` / `FolderEnforcer` may remain unchanged.

## Acceptance Criteria

1. `dotnet test FocusManager.Shared.slnf` reports no failures on Linux.
2. All existing Windows-style cases pass without changing Windows behavior.
3. `RuleEvaluator` contains neither `Path.GetFullPath` nor `Path.DirectorySeparatorChar`.
4. Registration in `src/Windows/FocusManager.Agent/Program.cs` uses explicit Windows semantics.
5. The known-failures note is removed from `AGENTS.md`.

## Checks

- Run `dotnet test FocusManager.Shared.slnf` on Linux and inspect both path-style case sets.
- Run `dotnet test FocusManager.Windows.slnf` on Windows for the registration change.
- Inspect `RuleEvaluator`, the agent registration, and `AGENTS.md` against criteria 3–5; record any unavailable OS checks.

## Open Questions

None

## Result

Not started. No implementation or checks have been completed for this task.
