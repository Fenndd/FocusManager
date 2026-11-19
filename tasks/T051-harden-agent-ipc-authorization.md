---
id: T051
title: Harden the agent IPC with token-based caller authorization
milestone: M2
status: planned
depends_on: ["T047", "T048", "T050", "T027"]
area: ["src/Windows/FocusManager.Agent/**", "src/Windows/FocusManager.Platform.Windows/**", "src/Windows/FocusManager.Contracts/**", "tests/Windows/**", "docs/spec/platforms/windows.md", "tasks/T051-*"]
---

# T051 — Harden the agent IPC with token-based caller authorization

## Goal

Make the UI pipe safe and usable when the agent runs as LocalSystem: a restrictive security descriptor, service-side caller identification from the client token, per-user data partitions, bounded requests and sanitized errors (ADR-0009, step S4). The agent stays a console process.

## Sources

[ADR-0009](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md) (Proposed): D4, D5, D9; hazard H4; checks V1, V2, V3, V11, M5; assumptions A9, A11, A13, A15. [Windows requirements](../docs/spec/platforms/windows.md) (WIN-002), [T047](T047-add-contract-versioning-and-agent-info.md) (schema for `GetAgentInfo`, `errorCode`, limits).

## Scope

In scope:
1. Pipe creation (`IpcPipeFactory`): the descriptor of D4 (LocalSystem owner and full access, NETWORK denied, INTERACTIVE read/write without create-instance rights, medium no-write-up label), the first-instance flag, remote clients rejected (native creation if A9 shows .NET does not set it), at least one listening instance. Failure to create the first instance (the name is held by a console agent, or squatted by another process) is returned to the caller as a typed result and never papered over by serving on a non-first instance; the service reaction is in ADR-0009 D10 item 5 (T052 implements it). Add a test in which a second server with the same name fails to become the first instance, and a note in the Result on whether .NET reports it as a distinguishable failure.
2. Per-request flow of D5: bounded read, impersonate, read user SID, session, integrity level, INTERACTIVE and NETWORK membership, AppContainer flag, elevation type and logon ID, revert; classify the caller; derive the data partition (T050) from the caller's SID. Requests never carry identity. Process IDs are used only for diagnostics.
3. Server behavior from T047: `protocolVersion` handling, `GetAgentInfo`, `errorCode` values with sanitized error text, read deadline, maximum request size, connection cap. The WinUI client, which sends no version, keeps working.
4. The authorization table of D5 for the existing commands (`GetStudyModeStatus`, `GetWhitelist`, `SetStudyMode`, `SaveWhitelist`); refusals are logged with rate limiting.
5. WIN-002 wording change in `docs/spec/platforms/windows.md` before the behavior changes, following the owner's answers to ADR-0009 Q2 and Q8.

Out of scope: protection-level checks (PROT-001–PROT-006, M7), the client-side check that the pipe owner is LocalSystem (T052, because it only holds once the service host exists), events and subscriptions (T014), the helper pipe (T049).

## Acceptance Criteria

1. The effective pipe security descriptor has the expected owner and ACEs, no create-instance right for anyone but LocalSystem, a NETWORK deny and a medium label (V1).
2. Authorization decisions are correct for synthetic callers: the user, another user, an administrator, network, low integrity, AppContainer, session 0, failed impersonation (V2).
3. An oversized request, a stalled client and concurrent clients do not block the server; error responses contain a code and no exception text (V3).
4. Opening `\\<computer name>\pipe\FocusManager.Agent.Pipe` from the network path is rejected (M5).
5. The WinUI client without a version still works; `GetAgentInfo` returns the supported protocol range.
6. The Result records A9, A11, A13 and A15 as verified, not verified or failed.

## Checks

- `dotnet test FocusManager.Windows.slnf` on Windows (V1–V3).
- Optional integration test V11: a low-integrity child process cannot open the pipe; a medium-integrity process in the same session can.
- Manual, Windows: M5, and a session start and end from the WinUI app and from the T017 UI if available. Cross-user checks need a second local account.
- Note: running as a console process cannot show service-identity behavior (the pipe owner is the console user); the LocalSystem-specific checks are repeated by T052.

## Open Questions

ADR-0009 must be accepted (T027). Owner questions in ADR-0009: Q2, Q3, Q8 (administrator access to other users' data), Q9 and Q16 (states shown for refusals and incompatibility).

## Result

Not started. No implementation or checks have been completed for this task.
