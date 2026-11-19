---
id: T049
title: Add the Windows session helper for Explorer interaction
milestone: M2
status: planned
depends_on: ["T048", "T027"]
area: ["src/Windows/FocusManager.SessionHelper/**", "src/Windows/FocusManager.Platform.Windows/**", "src/Windows/FocusManager.Contracts/**", "src/Windows/FocusManager.Agent/**", "tests/Windows/**", "FocusManager.sln", "FocusManager.Windows.slnf", "docs/architecture/README.md", "tasks/T049-*"]
---

# T049 — Add the Windows session helper for Explorer interaction

## Goal

Move Explorer-window monitoring and navigation into a per-session helper process that talks to the agent over a per-session pipe, so folder blocking no longer needs the agent to share the user's desktop (ADR-0009, step S2). The agent stays a console process; it starts the helper itself.

## Sources

[ADR-0009](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md) (Proposed): D1, D3, D9 (helper protocol); hazards H2, H7; checks V7; assumptions A1, A2, A18. [Folder requirements](../docs/spec/folders.md) (FLD-001–FLD-007), [Windows requirements](../docs/spec/platforms/windows.md), [architecture](../docs/architecture/README.md).

## Scope

In scope:
1. New project `src/Windows/FocusManager.SessionHelper` (`asInvoker`, no visible window), referencing `FocusManager.Platform.Windows` and `FocusManager.Contracts`; add it to `FocusManager.sln` and `FocusManager.Windows.slnf` in the Windows folder.
2. Versioned helper protocol (C# only): `FolderOpened(path, windowToken)` from the helper; read current folder, go back, close, navigate and open commands to it; opaque window tokens. Placement in `FocusManager.Contracts` unless the owner asks for schema-first.
3. A session-routed `IExplorerNavigator` in the agent; `FolderEnforcer` keeps its rules, repeat suppression and logging unchanged. `ExplorerInterop` is created only in the helper.
4. Per-session pipe `\\.\pipe\FocusManager.Agent.Helper.<sessionId>` created by the agent: DACL for LocalSystem and the session's logon SID only, remote clients rejected, first-instance flag set, one connection per session, a per-launch generation value, and the client process ID checked against the launched process. The helper exits when its pipe closes.
5. Console-mode launch: the agent starts the helper for the current session; `CreateProcessAsUser` and supervision are added by T052.
6. Add `FocusManager.SessionHelper` to `docs/architecture/README.md` (only after ADR-0009 is accepted) and note that Windows folder blocking needs the helper.
7. Measure the extra round-trip cost of the thin-helper design (assumption A18) with the folder tests.

Out of scope: launching the helper as another user, relaunch with backoff and session-change handling (T052), IPC authorization for the UI (T051), notifications from the helper (ADR-0009 Q6), protecting the helper from termination (ADR-0009 Q10, T034).

## Acceptance Criteria

1. Existing folder-blocking tests (FLD-001–FLD-007 behavior in `FocusManager.Agent.Tests`) pass unchanged through the routed navigator, using a fake helper.
2. Helper protocol tests: unknown message, unsupported version, oversized message and wrong-session routing are rejected; the helper exits on disconnect (V7).
3. With the helper started by the console agent, a forbidden folder is corrected exactly as before, and closing the agent ends the helper (manual, Windows).
4. No `FocusManager.Agent` code references `ExplorerInterop` or Shell COM directly.
5. The Result records A1 and A2 as verified (negative check; A2 by inspection only), not verified or failed, and the A18 measurement.

## Checks

- `dotnet test FocusManager.Windows.slnf` on Windows.
- Manual, Windows: `Start-FocusManager.cmd`, start a session, open a forbidden folder, confirm the correction and the notification behavior; stop the agent and confirm no helper process remains.
- Inspect the solution and filter files and the dependency direction (Shared code must not reference Windows projects).

## Open Questions

ADR-0009 must be accepted (T027). Q6 (notifications without the UI) and Q10 (helper termination) are owner questions that this task does not need answered; it must not decide them.

## Result

Not started. No implementation or checks have been completed for this task.
