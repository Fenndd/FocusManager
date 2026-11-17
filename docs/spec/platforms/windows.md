# Windows platform (WIN)

A primary platform (decision B1), developed in parallel with Android (decision B2).
Desktop architecture — ADR-0007: a C#/.NET agent (background service) + a UI using Tauri 2 and web technologies
(decision C3), shared by Windows and Linux.

## Components

| Component | Project | Role |
|-----------|--------|------|
| UI | Currently `FocusManager.App` (WinUI 3); target — a Tauri application shared with Linux (tasks T017, T018) | Managing sessions and profiles, tray, account sign-in |
| Agent | `FocusManager.Agent` (Worker; currently with a WinForms tray icon; target — a service without UI, task T016) | Event monitoring, blocking, settings storage |
| IPC | `FocusManager.Contracts` | UI ↔ agent messages over the named pipe `FocusManager.Agent.Pipe` (JSON, one line per request/response) |
| OS integrations | `FocusManager.Platform.Windows` | WMI, Explorer COM, Chrome policy registry |

## Requirements

### WIN-001 Agent with administrator privileges
The agent runs with administrator privileges (`requireAdministrator` manifest): monitoring process launches
through WMI (`Win32_ProcessStartTrace`) requires elevated privileges.

Platforms: Windows — done

### WIN-002 IPC access
The agent's named pipe is accessible only to the current user, administrators, and SYSTEM.

Platforms: Windows — done

### WIN-003 Ignored processes
The following are not blocked: processes from other Windows sessions; processes in the Windows directory and `Program Files\WindowsApps`;
processes without a full path; known system processes (`explorer.exe`, `svchost.exe`, `conhost.exe`, …);
FocusManager itself (`focusmanager.agent.exe`, `focusmanager.app.exe`). The full list is in `AppEnforcer`.

Platforms: Windows — done

### WIN-004 Application blocking method
A forbidden process is terminated together with its child process tree.

Platforms: Windows — done

### WIN-005 Website blocking method
For Chromium browsers (SITE-005), `URLBlocklist`/`URLAllowlist` are written to each browser's policy key (`Software\Policies\Google\Chrome`,
`Software\Policies\Microsoft\Edge`, etc.); for Firefox, the
`WebsiteFilter` policy is used (`Software\Policies\Mozilla\Firefox`). Policies are cleared when a session ends.
At the strict protection level, policies are written to `HKLM` (a user without administrator privileges cannot remove them);
at other levels, they are written to `HKCU` (decision I5).

Platforms: Windows — partial (Chrome only and `HKCU` only)

### WIN-006 Storage
Settings, profiles, and statistics are stored locally in SQLite (decision C6). The user does not need to
install anything. Cloud synchronization is through Firebase (see [synchronization](../sync.md)).

Platforms: Windows — partial
Note: **implementation change.** The code currently uses PostgreSQL (since 2026-09-30; previously SQLite
in `%LOCALAPPDATA%\FocusManager\focusmanager.db`). A task to return to SQLite is needed.

### WIN-007 Tray icon
The agent shows a tray icon with a session toggle and an “Exit” item. “Exit” follows the
protection level: at the strict level, exit is unavailable (or made more difficult — a profile option); at other levels, it requires
the same check as ending a session early (PROT-001, decision I5).

Platforms: Windows — partial (currently “Exit” is always available)

### WIN-008 Development launch script
`Start-FocusManager.cmd` builds `FocusManager.Windows.slnf`, restarts the agent with elevation,
and launches the UI.

Platforms: Windows — done

### WIN-009 Minimum version
Windows 10 version 1809 (build 17763) and later, x64 (GEN-004).

Platforms: Windows — done

### WIN-010 Agent as a Windows service
The agent runs as a Windows service so that an ordinary user cannot simply terminate its process;
the UI and tray icon are a Tauri application in the user's session (decisions I5, C3).

Platforms: Windows — planned
> ❓ Question: is the service always required or only for the strict level? Proposal — always
> (one installation scheme), with the protection level determining whether stopping is allowed.
