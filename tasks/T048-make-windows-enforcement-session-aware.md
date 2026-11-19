---
id: T048
title: Make Windows enforcement session-aware with a policy ledger
milestone: M2
status: planned
depends_on: ["T003", "T013", "T027"]
area: ["src/Windows/FocusManager.Agent/**", "src/Windows/FocusManager.Platform.Windows/**", "src/Shared/FocusManager.Core/**", "tests/Windows/**", "tests/Shared/**", "docs/spec/platforms/windows.md", "tasks/T048-*"]
---

# T048 — Make Windows enforcement session-aware with a policy ledger

## Goal

Make application blocking and browser-policy handling independent of the agent's own session and user, so that the agent can later run as a LocalSystem service (ADR-0009, step S1). The agent stays a console process and its observable behavior for a single user does not change.

## Sources

[ADR-0009](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md) (Proposed): D5 (session owner, on-demand resolution), D7 (application filter, policy target, ledger, self-exemption), D8 ordering constraint 8; hazards H1, H5, H8, H9, H13; checks V4, V5, V6, V12. [Windows requirements](../docs/spec/platforms/windows.md) (WIN-003, WIN-004, WIN-005), [study mode](../docs/spec/study-mode.md) (SM-002–SM-004).

Prerequisites: T003 provides the shared session controller and ports; T013 provides the data-driven browser list that the ledger records.

## Scope

In scope:
1. A session-owner resolver behind an interface (no OS calls in `src/Shared`): session ID to owner SID, cached per session ID and logon ID, with invalidation on logon and logoff. An unknown session ID is resolved on demand before a process is classified (D5; proposal), so a process event that arrives before the logon notification is not skipped.
2. Application filter: use the process-start event's `SessionID`, `Sid` and `ParentProcessID`; evaluate with the session owner's rules; replace the comparison with the agent's own cached session (H1). In console mode the single user's behavior is unchanged.
3. Retry with backoff and reported state when process monitoring cannot start (H9).
4. Browser-policy target: the owner's hive through `RegOpenCurrentUser` while impersonating, or `HKEY_USERS\<SID>`, depending on the results for assumptions A5 and A6; never `Registry.CurrentUser` from the service. The policy ledger as a port with apply-and-clear ordering (record intent, write, mark applied; delete values, remove record) and reconciliation. The durable implementation belongs to T050; this task tests against a fake durable store.
5. Self-exemption of FocusManager's own processes by verified full path, but only after the owner answers ADR-0009 Q12; until then the file-name exemption stays and this item is not started.
6. WIN-003 wording change for Q12, made in `docs/spec/platforms/windows.md` before the behavior changes.

Out of scope: partitions and durable storage (T050), IPC authorization (T051), the helper (T049), the service host (T052), `HKLM` policies (T034), the browser list itself (T013), the answer to what to do when an owner stays unresolved (ADR-0009 Q5).

## Acceptance Criteria

1. With a fake owner resolver, application filtering selects rules by the event's `SessionID`; session 0 and a session without a user are not enforced (V4).
2. A synthetic process event for an uncached session ID, delivered before any logon notification, is resolved on demand and evaluated with the owner's rules; a resolver failure is retried, does not terminate the process, and reports `unavailable` (V12; pending Q5).
3. Against an injected registry root, apply and clear follow the ledger order, and reconciliation after a simulated crash at each step converges to the intended state (V5).
4. A failing monitor start is retried and reported, not silently skipped (V6).
5. The Result records the outcomes for assumptions A5, A6, A7 and A20 (each verified, not verified or failed).
6. On Windows in console mode, application, folder and site blocking behave as before for one user (manual check with `Start-FocusManager.cmd`).

## Checks

- `dotnet test FocusManager.Shared.slnf` for the shared parts.
- `dotnet test FocusManager.Windows.slnf` on Windows for V4, V5, V6, V12 and the unchanged existing tests.
- Manual, Windows: start a session with `Start-FocusManager.cmd`; launch a forbidden application, open a forbidden folder and a forbidden site; end the session and confirm the policy values are gone.
- Inspect that no `src/Shared` file calls an OS API and that `Registry.CurrentUser` is no longer used for policies.
- A5 and A6 need a Windows account setup (standard user, and a user-created registry link); record the Windows build and accounts.

## Open Questions

ADR-0009 must be accepted (T027). Owner questions in ADR-0009: Q5 (behavior for unresolved owners, applications started at logon), Q12 (exemption by verified path). The self-exemption item waits for Q12.

## Result

Not started. No implementation or checks have been completed for this task.
