---
id: T003
title: Extract session orchestration from the Windows agent into Shared
milestone: M2
status: planned
depends_on: ["T001"]
area: ["src/Shared/FocusManager.Core/**", "src/Shared/FocusManager.Application/**", "src/Windows/FocusManager.Agent/**", "tests/Shared/**", "docs/architecture/decisions/**", "FocusManager.sln", "*.slnf"]
---

# T003 — Extract session orchestration from the Windows agent into Shared

## Goal

Put desktop session orchestration in shared code so that Windows and Linux agents use one implementation without changing Windows behavior.

## Sources

Legacy classification: type `refactor`; platform `shared`; priority `high`.

Requirement identifiers: `SM-001`, `SM-002`, `SM-003`, `SM-004`, `SM-005`.

Legacy ADR associations: ADR-0005, ADR-0007.

[Study mode](../docs/spec/study-mode.md), [ADR-0005](../docs/architecture/decisions/0005-native-stack-per-platform.md), and [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md).

`src/Windows/FocusManager.Agent/Services/AgentHostedService.cs` currently combines the Windows named-pipe IPC server, the WinForms tray (moves to Tauri in T016), and session logic: on/off state, snapshots, monitor start/stop ordering, site policy application, snapshot-based event filtering, and rule evaluation. The session logic is common to both desktop agents.

## Scope

In scope:
1. Define required ports in `FocusManager.Core/Abstractions`, for example `IAppLaunchMonitor`, `IAppBlocker`, `ISiteFilter` (apply/clear), `ISessionSnapshotProvider`, and `IFolderMonitor`. Avoid Windows concepts such as HWND or the registry; process identifiers are allowed.
2. Implement a `SessionController` or equivalent in `src/Shared`: state, session start/end, recovery after restart, and routing events to rules and blockers. If size warrants a `FocusManager.Application` project, add an ADR.
3. Adapt existing Windows `ProcessStartMonitor` / `AppEnforcer` / `SiteEnforcer` / `ExplorerMonitor` / `FolderEnforcer` implementations. Leave IPC and delegation in `AgentHostedService`.
4. Add shared unit tests with fake ports.

Out of scope:
- Profiles, triggers, and protection levels (new model T021, features M5–M9).
- Changes to the IPC protocol.

## Acceptance Criteria

1. `AgentHostedService` contains no session state, snapshot, or enable/disable sequencing logic.
2. Controller tests cover SM-002, SM-003, SM-004, and SM-005 and pass on Linux.
3. Manual checks on Windows confirm unchanged Windows behavior.

## Checks

- Run `dotnet test FocusManager.Shared.slnf` on Linux, including controller cases for SM-002–SM-005.
- Run `dotnet test FocusManager.Windows.slnf` on Windows.
- Use `Start-FocusManager.cmd` on Windows to check start/end, snapshot behavior, and restart recovery; inspect `AgentHostedService` for remaining orchestration logic.

## Open Questions

None

## Result

Not started. No implementation or checks have been completed for this task.
