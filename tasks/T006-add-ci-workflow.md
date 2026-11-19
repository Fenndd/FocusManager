---
id: T006
title: Add GitHub Actions CI
milestone: M1
status: planned
depends_on: ["T001", "T002", "T007"]
area: [".github/workflows/ci.yml", "tests/Windows/**", "AGENTS.md"]
---

# T006 — Add GitHub Actions CI

## Goal

Run shared and Windows checks in CI so that agents working on Linux also receive Windows validation.

## Sources

Legacy classification: type `chore`; platform `all`; priority `medium`.

Legacy ADR associations: ADR-0003, ADR-0007.

[ADR-0003](../docs/architecture/decisions/0003-source-tree-by-platform.md) and [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md).

There is currently no CI. Agents frequently work in Linux containers and cannot verify Windows-specific code locally.

## Scope

In scope:
1. Create `.github/workflows/ci.yml`: job `shared` on `ubuntu-latest` runs `dotnet test FocusManager.Shared.slnf`; job `windows` on `windows-latest` builds and tests `FocusManager.Windows.slnf`. Use `microsoft/setup-msbuild` for WinUI if needed until WinUI is removed.
2. Mark tests requiring a real registry environment, Explorer COM, or administrator rights with a trait such as `[Trait("Category", "Integration")]`; exclude with a CI filter only if necessary. Do not disable or delete these tests.
3. State in `AGENTS.md` that CI provides final Windows-code validation.
4. Allow later jobs for Tauri (T017), the Linux agent (T020), and Android (T009); those tasks add their own jobs.

Out of scope: release publication and signing.

Planning change (T026, 2026-11-17): CI follows the SDK migration as well as storage/path fixes so the first required green jobs validate the intended SDK.

## Acceptance Criteria

1. Both jobs are green on the main branch.
2. The SDK version is read from `global.json`.

## Checks

Owner scheduling hold (2026-11-18): the Linux-host job/checks described here are deferred until explicit
owner resumption. Preserve their planned definition; do not create/run a Linux job as a workaround
for the hold. Windows work can be scoped separately when this task becomes eligible, with deferred
coverage reported explicitly. No existing CI success or full acceptance is established by this note.

- Inspect workflow syntax, runner selection, SDK selection, and any test-category filters.
- Run `dotnet test FocusManager.Shared.slnf` and run `dotnet test FocusManager.Windows.slnf` on Windows.
- After an authorized push, inspect both GitHub Actions jobs on the main branch; record CI verification as unavailable until such a run exists.

## Open Questions

None

## Result

Not started. No implementation or checks have been completed for this task.
