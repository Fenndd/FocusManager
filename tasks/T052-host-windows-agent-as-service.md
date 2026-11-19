---
id: T052
title: Host the Windows agent as a LocalSystem service with session supervision
milestone: M2
status: planned
depends_on: ["T049", "T050", "T051", "T027"]
area: ["src/Windows/FocusManager.Agent/**", "src/Windows/FocusManager.Platform.Windows/**", "src/Windows/FocusManager.SessionHelper/**", "src/Windows/FocusManager.App/Services/AgentClient.cs", "tests/Windows/**", "scripts/windows/**", "Start-FocusManager.cmd", "docs/spec/platforms/windows.md", "tasks/T052-*"]
---

# T052 — Host the Windows agent as a LocalSystem service with session supervision

## Goal

Run the agent under the service control manager as LocalSystem, launch and supervise the session helper, and handle logon, logoff, reconnect and crash recovery (ADR-0009, step S5). In service mode the agent has no tray and creates no window; the WinForms tray stays available in console mode only, until T016 removes it (ADR-0009 D10).

## Sources

[ADR-0009](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md) (Proposed): D2, D3, D4 (client verification), D8, [D10](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md#d10-tray-ownership-in-console-and-service-mode-t056-proposal) (the trace of the current tray startup and the host-mode proposal), hazards H3, H7, H10, H11; checks V8, V14, V15, M1–M12 and M14–M16; assumptions A3, A4, A8, A10, A19 and A27–A30. [Windows requirements](../docs/spec/platforms/windows.md) (WIN-001, WIN-007, WIN-008, WIN-010). [T056 result](T056-clarify-proposed-service-design-after-review.md#result) (the documentation task that added D10; the proposal is untested and does not accept the ADR).

## Scope

In scope:
1. `Microsoft.Extensions.Hosting.WindowsServices` with `AddWindowsService`; LocalSystem, automatic (not delayed) start, dependency on the WMI service, quoted image path in an administrator-only directory, recovery actions including `fFailureActionsOnNonCrashFailures`, non-zero exit on fatal errors; Event Log source and level configuration (H10).
2. Explicit host mode and tray boundary (D10). `Program.cs` computes `AgentHostMode` (`Console` or `Service`) once, from `WindowsServiceHelpers.IsWindowsService()` and the process session ID, through a pure function that tests can call; the tray is enabled only in `Console` mode in a session other than 0 (table in D10). `AgentHostedService` depends on a small tray interface instead of the concrete `TrayHost`: `TrayHost` implements it unchanged (interface in `Tray/`), and `NullAgentTray` (no thread, no window, no events) implements it for service mode. A registration helper that tests can call (working location: a `Hosting/` folder of the Agent project) registers exactly one of them; the console-only registrations live in a separate method marked `NoInlining` that only the `Console` branch calls. In service mode `TrayHost` is never registered or constructed, `TrayNotifier` stays unregistered and `SilentNotifier` stays the `INotifier`. In console mode in a session other than 0 nothing changes: the same registrations, the same order of calls in `StartAsync` and `StopAsync` (`AgentHostedService.cs:71-125` at the time of the trace), the same tray events, `UseWindowsForms` kept. Log the selected mode at start. Required second layer: `TrayHost.Start` refuses to run in session 0, with an injectable session provider and a unit test. A console agent whose pipe server cannot become the first instance (D4, delivered by T051) exits with a non-zero code, and the scripts report an installed service, so that a console agent and the service never both enforce. The service side of the same failure (a console agent or a squatting process holds the pipe name) follows D10 item 5: no serving on a non-first instance, enforcement continues, rate-limited log, retry with backoff.
2a. Test seams (D10 item 7), added as behavior-preserving changes before V14 and V15 are written: interfaces for the monitors and enforcers that `AgentHostedService` uses (or an injectable composition), an injectable pipe name or IPC-loop factory, an injectable registry root and policy path for `ChromePolicyRegistry` (reuse the T048 injected root if it exists), and the tray interface. No V14 or V15 test starts the real IPC loop, the WMI watcher or `ExplorerMonitor`, touches `HKCU` or opens the real pipe name. Order: extract the interfaces mechanically; write V15 as a characterization test against the unchanged behavior and see it pass; only then add host mode, `AddWindowsService` and the registration helper.
3. Session-change handling through a lifetime derived from `WindowsServiceLifetime` (A10) and enumeration of existing sessions at start, with idempotent handling (D8).
4. Helper launch with `WTSQueryUserToken`, the user's environment block and `CreateProcessAsUser` into `winsta0\default`, no handle inheritance; supervision with exponential backoff; reporting `helperUnavailable`; never relaunching into a logging-off session. If the window-station or desktop DACL does not already allow the helper (A4), stop and report; do not edit it.
5. Reduced privilege declaration (`SERVICE_REQUIRED_PRIVILEGES_INFO`): measure actual use, record the final list with evidence per entry, and verify with M14.
6. Client-side verification that the pipe owner is LocalSystem in `AgentClient` (and the contract for the T017 Rust core); a mismatch counts as "agent unavailable".
7. Install, uninstall, restart and status scripts in `scripts/windows/`, update `Start-FocusManager.cmd` and the elevated launcher (H11), uninstall cleanup of ledger entries (loaded hives directly; unloaded hives reported until T041 chooses a method). The install script runs elevated and creates the service-owned data tree through the T050 code (ADR-0009 D6 R1) before the service first starts; a refusal under R2 fails the install with a message.
8. `docs/spec/platforms/windows.md` updates for WIN-001, WIN-008 and WIN-010 status and wording before the behavior changes.

Out of scope: removing the tray (T016: `TrayHost`, `TrayNotifier`, `UseWindowsForms`, the interface, the seam and `NullAgentTray` if they have no further purpose, the handoff to the Tauri tray, the single-icon acceptance M13 and the tray Exit semantics), installer and Store packaging (T041), protection against stopping the service or killing the helper (T034, ADR-0009 Q10), `HKLM` policies (T034). Between T052 and T016 a service-mode agent has no tray by design; the WinUI app or the Tauri UI is the control surface.

## Acceptance Criteria

1. The service starts at boot and survives user logoff; killing its process as an administrator leads to an SCM restart, restored state and no duplicate helpers (M6, M9).
2. After a reboot during a session, blocking resumes after logon without user action, and the UI and helper are not terminated by the agent (M7).
3. Standard-user and administrator (non-elevated UI) sessions both get application blocking, helper-based folder correction and site policy under their own `HKEY_USERS\<SID>` (M2, M3); a second user is isolated (M4, pending Q3).
4. Ending the helper leads to a relaunch with the degraded state visible meanwhile (M8); lock, unlock, switching and remote desktop where available keep enforcement and let the UI reconnect (M10).
5. `sc qc`, `sc qfailure`, `sc qfailureflag` and `icacls` show the configuration of M1; no FocusManager window or `explorer.exe` runs in session 0 (M11, M15).
6. The uninstall script leaves no FocusManager policy values in loaded hives and deletes the service (M12).
7. With the reduced privilege set declared, M1–M3 and M8 still pass (M14); the Result lists the final privilege set.
8. Lifecycle decision tests for synthetic session events (logon, logoff, disconnect, reconnect, duplicates, enumeration at start) pass (V8).
9. Service mode starts no tray: the host-mode decision table and the registration tests pass (V14), including that `TrayHost` is neither registered nor constructed in `Service` mode, `TrayNotifier` is not registered, re-adding the `TrayHost` registration makes the test fail, and `TrayHost.Start` does nothing in session 0; M15 shows no window and no tray icon for the service process in session 0, with the probe's positive control (it finds the tray's window in console mode) recorded, and its module list is the evidence for whether `System.Windows.Forms` is loaded (A29).
10. Console mode is unchanged in sessions other than 0: V15 (a characterization test written first against the unchanged behavior and kept green) and M16 carry this claim, and M16 shows the tray icon, the Study Mode toggle and Exit working as before, in an isolated session. No existing test references `AgentHostedService` or `TrayHost`, so the existing Agent tests passing is not evidence; that is stated in the Result. A console agent in session 0 no longer gets a tray (deliberate, D10).
11. The Result records A3, A4, A8, A10, A19 and A27–A30 as verified, not verified or failed, and the `whoami /priv` output of the service (M14).

## Checks

- `dotnet test FocusManager.Windows.slnf` on Windows (V8, V14, V15).
- Manual checks M1–M12 and M14–M16 from ADR-0009 on a Windows virtual machine with one standard and one administrator account, using the scripts created by this task (they do not exist yet). Record the Windows build and accounts. M16 and every console-mode run happen in an isolated session or VM and never against the owner's live data (`%LOCALAPPDATA%\FocusManager`), agent, pipe or registry. A successful build or unit test is not evidence of session-0 behavior, reboot recovery or access isolation.
- Record `whoami /priv` of the service process in M14. Review the privilege list, the quoted image path and the install directory ACL with an independent reviewer, because this is privileged code.

## Open Questions

ADR-0009 must be accepted (T027). Owner questions in ADR-0009: Q1 (service always or only for the strict level), Q5, Q7 (UI autostart), Q10, Q13, Q16 (stopping the service). Owner answers of 2026-11-18 ([decisions K1](../docs/spec/decisions.md)) supersede conflicting proposal text, including the Q1 premise and the D8 stop and snapshot wording; [T058](T058-reconcile-service-design-with-owner-decisions.md) reconciles it before this task starts. The host-mode design of D10 raises no new product question. The elevated install step that creates the data tree (ADR-0009 D6 R1, Q17) is a T052 script item once T050's code exists.

## Result

Not started. No implementation or checks have been completed for this task. The host-mode and tray boundary (scope item 2, criteria 9 and 10) were specified by the documentation task [T056](T056-clarify-proposed-service-design-after-review.md#result); the specification is untested and does not accept ADR-0009.
