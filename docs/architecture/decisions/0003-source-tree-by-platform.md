# ADR-0003: Split Sources into Shared / Windows / Android

- Status: Accepted; extended by ADR-0007 (target layout with Linux, Tauri, and Android in Kotlin). Storage: ADR-0006
- Date: 2026-09-30

## Context

Previously all projects sat directly in `src/`, and `FocusManager.Infrastructure` mixed cross-platform
storage (SQLite) with Windows integrations (WMI, File Explorer COM, Chrome registry). With Android coming,
it must be clear which code is shared and which is platform-specific, and platforms must build independently.

## Decision

- `src/Shared`, `src/Windows`, `src/Android`; `tests/` mirrors the same layout.
- `FocusManager.Infrastructure` was split into:
  - `src/Shared/FocusManager.Persistence.Sqlite` (namespace `FocusManager.Persistence.Sqlite`);
  - `src/Windows/FocusManager.Platform.Windows` (namespace `FocusManager.Platform.Windows`, TFM `net8.0-windows`).
- `FocusManager.Contracts` moved to `src/Windows`: it provides IPC between the Windows UI and Windows agent;
  Android does not need IPC (the UI and service are in one application). If Android needs the DTOs
  (for example, to import/export settings), move them to `Shared` through a separate decision.
- The names `FocusManager.App` / `FocusManager.Agent` are preserved: executable names and scripts depend on them.
- Solution filters `FocusManager.Shared.slnf`, `FocusManager.Windows.slnf` (and later `FocusManager.Android.slnf`)
  allow a platform to build without other platforms' SDKs/workloads.
- Shared project settings moved to `Directory.Build.props`, and shared test packages to `tests/Directory.Build.props`.
- Developer scripts live in `scripts/<platform>/`.

## Consequences

- The move did not change behavior; semantic refactorings (moving Windows defaults out of storage,
  moving orchestration to Shared) were recorded as tasks `T001`–`T005`.
- Build paths changed: `src/Windows/FocusManager.Agent/bin/...`; scripts were updated.
