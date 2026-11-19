---
id: T001
title: Make Core path rules independent of the host OS
milestone: M1
status: review
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

Owner scheduling change (2026-11-18): Linux development and Linux-host validation are on hold until
the owner explicitly resumes them. Criterion 1 and the Linux command below remain unverified/deferred,
not passed; do not provision a Linux environment to close this handoff. This does not waive the original
criterion or establish acceptance. Windows remediation and its verification are [T054](T054-fix-windows-path-compatibility-after-review.md).

- Run `dotnet test FocusManager.Shared.slnf` on Linux and inspect both path-style case sets.
- Run `dotnet test FocusManager.Windows.slnf` on Windows for the registration change.
- Inspect `RuleEvaluator`, the agent registration, and `AGENTS.md` against criteria 3–5; record any unavailable OS checks.

## Open Questions

None

## Result

Implementation submitted by a tier-2 executor. Independent review on 2026-11-18 requires changes;
owner acceptance is pending. See the [review](../docs/reviews/2026-11-18-uncommitted.md) and
[T054](T054-fix-windows-path-compatibility-after-review.md). The prior result below records the submitted
implementation and its historical checks; it does not override the subsequent findings.

Changes:
- `src/Shared/FocusManager.Core/Abstractions/IPathRules.cs`: new port (`Normalize`, `GetFileName`, `AreEqual`, `IsChild`).
- `src/Shared/FocusManager.Core/Rules/PathRules.cs`: pure-string `PathRules.Windows` (`\` and `/` separators, drive/UNC roots, case-insensitive) and `PathRules.Unix` (`/` only, case-sensitive). No `System.IO.Path`, no current directory.
- `RuleEvaluator` takes `IPathRules`; its private `NormalizePath`/`IsChildPath` and the `Path.*` calls are removed. The temporary parameterless constructor (Windows default) was deleted in the review follow-up: every call site (`AppEnforcerTests`, `FolderEnforcerTests` in `tests/Windows/FocusManager.Agent.Tests`) now passes `PathRules.Windows` explicitly, so path semantics are always explicit. A grep for `RuleEvaluator()` across `src/` and `tests/` finds no remaining parameterless construction.
- `src/Windows/FocusManager.Agent/Program.cs`: registers `IPathRules` as `PathRules.Windows` and builds `RuleEvaluator` from it.
- `tests/Shared/FocusManager.Core.Tests`: `RuleEvaluatorTests` parameterized over Windows and Unix styles via `PathStyleFixture` (paths built from literals, not `System.IO.Path`), plus Windows-only and Unix-only cases and new `PathRulesTests`.
- `AGENTS.md`: known-failures note removed.

Windows behavior notes from the submitted implementation (superseded where contradicted by the 2026-11-18 review):
the file-name fallback is affected by colon handling, and the drive-root representation change is observable
through values persisted by the UI. Both are confirmed regressions assigned to T054. An empty allowed parent
never matches as a parent in `IsChild`, preserving the previous effective result. Other recorded cases:
- (a) Relative paths are normalized but not resolved against the current directory (the old code resolved them through `Path.GetFullPath`).
- (b) The old normalizer maps `C:\Study.` to `C:\Study`, while `PathRules.Windows` keeps the dot. The review confirmed a changed decision on .NET 8; preserving the existing behavior belongs to T054, not a silent deferral to T012. Do not generalize the trailing-space rules beyond observed BCL behavior.
- (c) The root representation (`D:` versus `D:\`) changes folder decisions for values written by `FoldersWhitelistPage.NormalizeFolderPath`; this is observable and belongs to T054.
- (d) An empty candidate path equals an empty allowed path in `AreEqual`. This quirk pre-dates this task and belongs to T012.

Checks (Windows host, final state after the constructor removal):
- `dotnet test FocusManager.Shared.slnf`: verified, Core.Tests 88 passed, Persistence.Sqlite.Tests 35 passed, 0 failed.
- `dotnet test FocusManager.Windows.slnf` (PowerShell): verified, Core.Tests 88, Platform.Windows.Tests 6, Persistence.Sqlite.Tests 35, Agent.Tests 26 passed, 0 failed.
- The three tests that failed on non-Windows hosts before this task (`EvaluateApplicationStart_Allows_ByFileNameFallback_WhenFullPathUnavailable`, `EvaluateFolderOpen_Allows_SameFolderPath_IgnoringTrailingSeparator`, `EvaluateFolderOpen_Allows_ChildFolder_WhenSubfoldersAllowed`) now pass on Windows in both the Windows and the Unix path styles.
- Grep `RuleEvaluator.cs` for `Path.GetFullPath` / `Path.DirectorySeparatorChar`: verified absent. Grep for `new RuleEvaluator()` in `src/` and `tests/`: verified none left.
- Linux run of `dotnet test FocusManager.Shared.slnf` (criterion 1): not verified, no Linux host available. Unix-style cases use literal `/` paths and a pure-string implementation, which suggests host independence but does not prove it.

2026-11-19 pointer: the Windows compatibility remediation for the review's P1/P2 findings is recorded in [T054](T054-fix-windows-path-compatibility-after-review.md) (awaiting review and acceptance). This result is not rewritten; T001 remains pending owner acceptance and AC1 stays deferred under the Linux hold.
