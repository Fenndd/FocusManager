---
id: T058
title: Reconcile the service design with confirmed owner decisions
milestone: M2
status: planned
depends_on: ["T056"]
area: ["docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md", "docs/architecture/decisions/README.md", "tasks/T003-*", "tasks/T014-*", "tasks/T016-*", "tasks/T027-*", "tasks/T047-*", "tasks/T048-*", "tasks/T049-*", "tasks/T050-*", "tasks/T051-*", "tasks/T052-*", "tasks/T058-*", "docs/spec/study-mode.md", "docs/spec/platforms/windows.md", "docs/spec/protection.md"]
---

# T058 — Reconcile the service design with confirmed owner decisions

## Goal

Produce one coherent proposed service design implementing the owner's confirmed K1 behavior, without overwriting the concurrent T056 technical clarification or silently accepting the architecture.

## Sources

- [Owner decisions K1](../docs/spec/decisions.md#k1-windows-service-and-session-behavior), including the two follow-up answers on all-level service and crash cleanup.
- [Windows requirements](../docs/spec/platforms/windows.md), [sessions](../docs/spec/study-mode.md), [protection](../docs/spec/protection.md).
- [ADR-0009](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md), [T056](T056-clarify-proposed-service-design-after-review.md), [current handoff](../STATUS.md).

## Scope

In scope:
1. Integrate K1 answers into ADR questions/decisions, retaining unanswered residual subquestions and newer Q17/Q18 accurately. One service serves all levels; no cross-user administration; UI startup and starting a new blocking session are separate settings.
2. Replace the proposed recovery snapshot exemption with an initial/recovery sweep of already-running forbidden processes under active rules; preserve system/essential exceptions and verified process/session identity. Define safe behavior when identity is not yet available, without guessing another user's rules.
3. Design removal of ongoing restrictions on controlled stop and also crash/forced termination by an independent mechanism. Define ownership, detection latency, durable cleanup ledger, failure states and cleanup/restart ordering; old cleanup must not erase recovered rules or another session's restrictions. Do not claim zero-delay cleanup or resurrect terminated applications.
4. Prove/qualify enforcement isolation to the associated Windows OS session. Distinguish stored user data from runtime session state. HKLM machine policies are excluded where they affect other sessions; HKCU/HKEY_USERS per SID also needs same-user multi-session validation. If existing browser policies cannot meet the boundary, present alternatives and a concrete owner/design gate, not a silent weaker guarantee.
5. Reconcile UI-only closing, protected session ending, per-session disabling and stopping a machine-wide host: one user's ordinary exit cannot silently stop another user's valid enforcement or expose their data.
6. Define settings retention/full uninstall cleanup, update migration with the limited test-data exception, verified own-component exemption, background notifications and per-category state. Preserve exact Windows minimum until packaging evidence selects a change.
7. Update the affected T003/T014/T016/T027/T047–T052 task boundaries/dependencies/checks; create bounded child implementation/prototype tasks before coding any new crash-cleanup/autostart/recovery work. Record a new ADR if the selected independent mechanism constitutes a separate significant architecture decision.

Out of scope: service/helper/watchdog implementation, machine installation/ACL changes, deleting databases, deploying cloud, accepting ADR-0009, account-switch restriction implementation (T059), or Linux work.

T056 is currently assigned elsewhere; wait for its accepted result or explicit scoped owner continuation and serialize these overlapping edits. The owner accepted product behavior, not the whole earlier architecture or its implementation task results.

## Acceptance Criteria

1. An answer-to-design/check map covers all K1 decisions and residual questions; stale contradictory proposal text is removed or explicitly historical.
2. Crash cleanup and active-rule recovery have a concrete protocol with race/failure checks, including host crash, forced termination, recovery overlap and two users/sessions. Unverified mechanisms are labeled as such and have bounded proof tasks.
3. Browser-policy session isolation and global-service stop semantics have discriminating checks or an explicit unresolved design blocker; no unsupported parity claim is made.
4. Linked tasks reflect the revised protocol and owner decisions; dependencies are acyclic and implementation readiness does not bypass acceptance gates.
5. Independent read-only architecture review evaluates the proposal and its evidence. ADR remains Proposed pending explicit owner acceptance; production behavior is not claimed from documentation.

## Checks

- Trace current lifecycle, snapshot, policy writer and IPC consumers against K1; inspect task/ADR/specification consistency.
- Use primary Windows/browser documentation for feasibility; record exact sources and unverified assumptions. Any active prototype requires its own ready task and isolated Windows scope.
- Inspect local links, task schema/dependencies and scoped diff; run `git diff --check`.
- No .NET/Linux run or external modification is required by this design task.

## Open Questions

The desired behavior is confirmed, but the safe independent cleanup mechanism, session-local browser method, process-owner failure handling and global-host stopping semantics require design evidence. Do not invent defaults where the source does not settle them. The exact Store minimum and ADR-0009's newer Q17/Q18 remain gated by their own evidence/owner answers.

## Result

Not started. Planned after T056 to prevent overlapping edits; no blanket continuation or architecture acceptance is inferred from the owner's product answers.
