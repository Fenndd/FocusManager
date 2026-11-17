---
id: T007
title: Migrate to .NET 10 LTS
milestone: M1
status: planned
depends_on: ["T002"]
area: ["global.json", "Directory.Build.props", "tests/Directory.Build.props", "src/**/*.csproj", "tests/**/*.csproj", "scripts/windows/Start-FocusManager.ps1", "docs/architecture/README.md", "AGENTS.md"]
---

# T007 — Migrate to .NET 10 LTS

## Goal

Move the desktop code and development launch flow from .NET 8 to .NET 10 LTS without adding platform or language features.

## Sources

Legacy classification: type `chore`; platform `shared`; priority `high`.

Legacy ADR associations: ADR-0005.

[ADR-0005](../docs/architecture/decisions/0005-native-stack-per-platform.md).

The project uses .NET 8 (`global.json`: SDK 8.0.419). The original planning rationale records .NET 8 support ending on 2026-11-10 and no further security updates; because the agent runs as administrator, migration is urgent. Verify lifecycle and compatible package versions when implementing. T002 comes first to avoid updating Npgsql immediately before its removal.

## Scope

In scope:
1. Change `global.json` to SDK 10.0.x.
2. Update TFMs to `net10.0`, `net10.0-windows`, and `net10.0-windows10.0.19041.0` for App while it exists.
3. Update `Microsoft.Data.Sqlite`, `Microsoft.Extensions.*`, `System.Management`, test packages, and `Microsoft.WindowsAppSDK`.
4. Check Windows build/start through `Start-FocusManager.cmd`; update TFM paths in `scripts/windows/Start-FocusManager.ps1`.
5. Update `net8.0` mentions in `docs/architecture/README.md` and `AGENTS.md`.

Out of scope: adopting new language or platform features.

## Acceptance Criteria

1. `dotnet test FocusManager.Shared.slnf` and `dotnet test FocusManager.Windows.slnf` pass.
2. The launch script starts the Windows version, which blocks applications, folders, and sites as before.

## Checks

- Run `dotnet test FocusManager.Shared.slnf` and, on Windows, `dotnet test FocusManager.Windows.slnf`.
- Run `Start-FocusManager.cmd` on Windows and check application, folder, and site blocking.
- Inspect SDK/TFM settings, package versions, script paths, and documentation mentions.

## Open Questions

None

## Result

Not started. No implementation or checks have been completed for this task.
