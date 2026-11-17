---
id: T014
title: Enable configurable blocking notifications
milestone: M2
status: planned
depends_on: ["T016", "T017"]
area: ["contracts/**", "src/Windows/FocusManager.Agent/**", "src/Desktop/**", "tests/Windows/**", "docs/spec/notifications.md"]
---

# T014 — Enable configurable blocking notifications

## Goal

Show blocking notifications by default through the desktop UI, with a persisted off switch and repeat suppression.

## Sources

Legacy classification: type `change`; platform `windows`; priority `medium`.

Requirement identifiers: `NTF-001`, `NTF-002`.

Legacy ADR associations: ADR-0007.

[Notification requirements](../docs/spec/notifications.md) and [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md).

NTF-001 / decision E8 enables notifications by default and lets users disable them in settings. The agent currently registers `SilentNotifier` and has no setting. As tray/UI move to Tauri (T016, T017), the UI shows notifications and the agent emits IPC blocking events.

## Scope

In scope:
1. Add an agent-to-UI “blocked” event with a contract in `contracts/`.
2. Show system notifications in the UI; persist a local “Blocking notifications” setting, enabled by default.
3. Suppress repeats (NTF-002) for applications and sites as well as folders.

Out of scope: Linux and Android delivery tasks are created through T035; T020 provides host/IPC only.

## Acceptance Criteria

1. Application and folder blocking produce notifications; turning off the setting disables them.
2. Windows NTF-001 and NTF-002 statuses are `done`.

## Checks

- Run `dotnet test FocusManager.Windows.slnf` on Windows for agent event and repeat-suppression behavior.
- Run the desktop test command documented by T017; that command is not established yet.
- Manually block an application and folder, change the setting, restart the UI, and verify persistence and repeat suppression for applications, sites, and folders.
- Inspect Windows notification requirement statuses.

## Open Questions

None

## Result

Not started. No implementation or checks have been completed for this task.
