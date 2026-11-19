---
id: T016
title: Run the Windows agent as a service without its own UI
milestone: M2
status: planned
depends_on: ["T027", "T003", "T017", "T047", "T048", "T049", "T050", "T051", "T052"]
area: ["src/Windows/FocusManager.Agent/Tray/**", "src/Windows/FocusManager.Agent/Notifications/**", "src/Windows/FocusManager.Agent/FocusManager.Agent.csproj", "src/Windows/FocusManager.Agent/Program.cs", "src/Windows/FocusManager.Agent/Hosting/**", "src/Windows/FocusManager.Agent/Services/AgentHostedService.cs", "tests/Windows/FocusManager.Agent.Tests/**", "docs/spec/platforms/windows.md", "tasks/T016-*"]
---

# T016 — Run the Windows agent as a service without its own UI

## Goal

Complete the move of the Windows agent to a UI-free Windows service: remove the agent's tray after the Tauri tray exists, and accept the whole service chain end to end. Most implementation is done by the child tasks T047–T052; this task owns only the tray handoff (ADR-0009 step S6) and the final acceptance gate.

## Sources

Legacy classification: type `refactor`; platform `windows`; priority `high`.

Requirement identifiers: `WIN-001`, `WIN-007`, `WIN-010`.

Legacy ADR associations: ADR-0007.

[Windows requirements](../docs/spec/platforms/windows.md), [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md), and [ADR-0009](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md) (Proposed; the design from T027).

The current agent is an elevated console process with a WinForms tray (`src/Windows/FocusManager.Agent/Tray/TrayHost.cs`). The service runs as LocalSystem in session 0, where a tray, notification display and Explorer interaction cannot work (hazards H1–H5 in the ADR). The children split the work as follows:

| Child | ADR step | Result |
| --- | --- | --- |
| [T047](T047-add-contract-versioning-and-agent-info.md) | D9 | Contract versioning, `GetAgentInfo`, error codes |
| [T048](T048-make-windows-enforcement-session-aware.md) | S1 | Session-aware application and policy targeting, ledger port |
| [T049](T049-add-windows-session-helper.md) | S2 | Session helper for Explorer interaction |
| [T050](T050-move-windows-storage-to-service-owned-partitions.md) | S3 | Service-owned per-user storage, durable ledger |
| [T051](T051-harden-agent-ipc-authorization.md) | S4 | Pipe security descriptor, caller authorization |
| [T052](T052-host-windows-agent-as-service.md) | S5 | Service host, helper launch and supervision, scripts |

Planning change (T026, 2026-11-17): Separate service/user-session design into T027 and finish session extraction before moving the host. T016 cannot start until its design is accepted.

Planning change (T027, 2026-11-18, proposal pending ADR-0009 acceptance): the former single scope item "implement the accepted ADR" is replaced by the child tasks above; T016 keeps the tray handoff and the acceptance gate. If the owner rejects or changes ADR-0009, the children and this task are revised together.

## Scope

In scope:
1. After T017 provides the Tauri tray and the owner has answered ADR-0009 Q4 (what tray Exit does in the service model), remove WinForms (`UseWindowsForms`), `TrayHost`, `TrayNotifier` and the tray coupling in `AgentHostedService` from the agent, and remove what T052 added only to keep the tray out of session 0 when nothing else uses it: the tray interface, `NullAgentTray`, the host-mode seam and the console-only registration method in `src/Windows/FocusManager.Agent/Hosting/` (ADR-0009 D10 item 6; the interface is in `Tray/`, which is already in this task's area). Keep the host-mode decision if the service still needs it.
2. Update the WIN-007 and WIN-010 text and status in `docs/spec/platforms/windows.md` as the owner's answers require.
3. Run the end-to-end acceptance pass over the finished chain on a Windows virtual machine, repeating the manual checks that matter to the original acceptance criteria (ADR-0009 M1–M14), and re-run M15 (no window, no tray icon and no `System.Windows.Forms` module in the service process) and M16 (console mode, if console mode still exists) on the final build.

Out of scope: preventing service termination at strict protection levels (M7, T034); installer and Store packaging (T041); anything the child tasks own.

## Acceptance Criteria

1. T047–T052 are accepted (`done`) and ADR-0009 is accepted, with acceptance recorded.
2. The agent runs as a service and survives user logoff and reboot (M6, M7, M9 re-run on the final build).
3. Manual Windows checks confirm unchanged application/folder blocking (M2, M3).
4. The agent has no tray icon and no WinForms reference; Tauri owns the tray (M11, M13), and the tray interface, `NullAgentTray` and the host-mode seam are removed or kept with a recorded reason. M15 is re-run on the final build; M16 is re-run, or recorded as no longer applicable if console mode was removed.
5. The end-to-end check that the T017 UI obtains `GetAgentInfo` and typed errors from the real service, which T017 could only verify against a fake, passes and is recorded.

## Checks

- Review the final implementation against ADR-0009 and the three session-0 constraints (session comparison, Explorer COM, pipe identity) before closing.
- Run `dotnet test FocusManager.Windows.slnf` on Windows.
- Use the service scripts created by T052 (they do not exist until T052 is done); log off and reboot; check service survival, interactive-user IPC, Explorer interaction, and application/folder blocking.
- Verify that only the Tauri UI owns the tray icon (M13) and that no tray icon or `explorer.exe` runs in session 0 (M11).
- Inspect the agent project for `UseWindowsForms`, the removed tray classes, the tray interface and `NullAgentTray`.
- Re-run M15 and M16 (ADR-0009) on the final build, in an isolated session or VM.

## Open Questions

Needs from the owner: ADR-0009 Q4 (tray Exit semantics, WIN-007). The other owner questions are carried by the child tasks (Q1 and Q16 in T052; Q2, Q3, Q8 in T051; Q5 and Q12 in T048).

## Result

Not started. No implementation or checks have been completed for this task.
