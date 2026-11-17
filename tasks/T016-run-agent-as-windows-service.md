---
id: T016
title: Run the Windows agent as a service without its own UI
milestone: M2
status: planned
depends_on: ["T027", "T003", "T017"]
area: ["src/Windows/FocusManager.Agent/**", "src/Windows/FocusManager.Platform.Windows/**", "src/Desktop/**", "tests/Windows/**", "scripts/windows/**", "Start-FocusManager.cmd", "docs/architecture/decisions/**"]
---

# T016 — Run the Windows agent as a service without its own UI

## Goal

Move the Windows agent to a UI-free Windows service while preserving application/folder enforcement and moving the tray to Tauri.

## Sources

Legacy classification: type `refactor`; platform `windows`; priority `high`.

Requirement identifiers: `WIN-001`, `WIN-007`, `WIN-010`.

Legacy ADR associations: ADR-0007.

[Windows requirements](../docs/spec/platforms/windows.md) and [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md).

The current agent is an elevated console process with a WinForms tray (`src/Windows/FocusManager.Agent/Tray/TrayHost.cs`). Design session boundaries before coding: the service runs as SYSTEM in session 0, but `AppEnforcer` compares process sessions to the agent session; `ExplorerInterop` uses Explorer-window COM unavailable from the service; and `IpcPipeFactory` grants access to the “current user,” which would become SYSTEM.

## Scope

In scope:
1. Implement the accepted T027 ADR describing what runs in the service (process monitoring, policies, storage), what runs in the user session (Explorer interaction in Tauri or a separate helper), and their connection.
2. Use `Microsoft.Extensions.Hosting.WindowsServices`; remove WinForms and `TrayHost` from the agent.
3. Correct interactive-user session detection and IPC pipe access for that user.
4. Add install/uninstall/restart development scripts in `scripts/windows/` and update `Start-FocusManager.cmd`.

Out of scope: preventing service termination at strict protection levels belongs to M7.

Planning change (T026, 2026-11-17): Separate service/user-session design into T027 and finish session extraction before moving the host. T016 cannot start until its design is accepted.

## Acceptance Criteria

1. The agent runs as a service and survives user logoff and reboot.
2. Manual Windows checks confirm unchanged application/folder blocking.
3. The agent has no tray icon; Tauri owns the tray.

## Checks

- Review the service/user-session design against the three session-0 constraints before implementation.
- Run `dotnet test FocusManager.Windows.slnf` on Windows.
- Use the service scripts created by this task (commands do not exist yet), log off/reboot, and check service survival, interactive-user IPC, Explorer interaction, and application/folder blocking.
- Verify that only the Tauri UI owns the tray icon.

## Open Questions

None

## Result

Not started. No implementation or checks have been completed for this task.
