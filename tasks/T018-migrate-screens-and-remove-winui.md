---
id: T018
title: Migrate screens to Tauri and remove the WinUI application
milestone: M2
status: planned
depends_on: ["T017", "T016", "T012", "T022"]
area: ["src/Desktop/**", "src/Windows/FocusManager.App/**", "FocusManager.sln", "FocusManager.Windows.slnf", "scripts/windows/**", "Start-FocusManager.cmd", "README.md", "AGENTS.md", "docs/spec/platforms/windows.md"]
---

# T018 — Migrate screens to Tauri and remove the WinUI application

## Goal

Replace existing whitelist and warning screens with Tauri equivalents, then remove the obsolete WinUI application and its build dependencies.

## Sources

Legacy classification: type `feature`; platform `windows`; priority `high`.

Requirement identifiers: `APP-006`, `FLD-001`, `FLD-003`, `SITE-003`, `SM-006`.

Legacy ADR associations: ADR-0007.

[Application input](../docs/spec/apps.md), [folder rules](../docs/spec/folders.md), [site input](../docs/spec/sites.md), [study mode warnings](../docs/spec/study-mode.md), [profiles](../docs/spec/profiles.md), and [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md).

After T017, migrate `src/Windows/FocusManager.App/Views/`: applications, folders with the subfolder flag, sites, and empty-category warnings. T004 was cancelled in favor of this task.

## Scope

In scope:
1. Tauri application/folder/site lists: add through file/folder selection or pattern input, remove, and edit the “include subfolders” flag.
2. Validate and normalize input by SITE-003 / APP-006; UI tests consume common `test-vectors/` cases (T022) wherever a matching rule exists.
3. Show empty-category warnings using the new SM-006 / PRF-004 semantics.
4. Remove `src/Windows/FocusManager.App` from the repository, `FocusManager.sln`, and `FocusManager.Windows.slnf`. Update launch scripts, `README.md`, `AGENTS.md`, and `docs/spec/platforms/windows.md`.

Out of scope: profiles, triggers, and statistics (M5/M6/M9).

Planning change (T026, 2026-11-17): Do not remove WinUI until the service/tray migration is verified and shared input vectors exist.

## Acceptance Criteria

1. Manual Windows checks confirm all former UI scenarios work in Tauri.
2. The WinUI project is absent; the Windows build no longer requires WindowsAppSDK.

## Checks

- Run desktop tests/build using T017 README commands and shared input test vectors where available.
- Run `dotnet test FocusManager.Windows.slnf` on Windows and verify the build has no WindowsAppSDK dependency.
- Use the updated launch script and manually check add/remove for applications, folders/subfolders, sites, validation, and empty-category warnings.
- Inspect solution/filter references and updated documentation.

## Open Questions

None

## Result

Not started. No implementation or checks have been completed for this task.
