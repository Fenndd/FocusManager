---
id: T027
title: Design the Windows service and user-session boundary
milestone: M2
status: review
depends_on: []
area: ["docs/architecture/decisions/**", "tasks/T016*", "tasks/T003*", "tasks/T005*", "tasks/T014*", "tasks/T017*", "tasks/T04[7-9]-*", "tasks/T05[0-2]-*", "docs/spec/**", "contracts/**"]
---

# T027 — Design the Windows service and user-session boundary

## Goal

Define a reviewable service/user-session design before moving privileged enforcement into session 0.

## Sources

[Architecture](../docs/architecture/README.md), [task lifecycle](../docs/engineering/TASK_FORMAT.md), [Windows requirements](../docs/spec/platforms/windows.md) (WIN-001, WIN-007, WIN-010), [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md).

## Scope

In scope: inspect current AppEnforcer session filtering, ExplorerInterop COM, IpcPipeFactory ACLs and tray ownership; compare helper placement, identity/authentication, disconnect/reconnect, logoff and policy cleanup. Record a proposed ADR and split T016 further if the selected implementation exceeds one session. Define required contract additions with compatibility checks.
Out of scope: service implementation, choosing new protection behavior, installing a service.

## Acceptance Criteria

1. The ADR identifies service/helper responsibilities, authenticated user identity, IPC permissions and lifecycle/failure handling with alternatives and consequences.
2. T016 has exact boundaries and checks derived from the design; required contract changes are assigned before consumers.
3. ADR acceptance is recorded explicitly; proposed choices are not described as accepted.

## Checks

- Trace the three existing session-0 hazards in source and review proposed handling for an unauthorized user, reconnect, logoff and reboot.
- Inspect ADR status and T016 dependency/readiness; documentation-only checks apply.

## Open Questions

Owner decisions are needed before the ADR can be accepted and before the dependent tasks start. They are ADR-0009 Q1–Q18 (Q17–Q18 were added by T056; see decisions K1 for the answers the owner has already given; [open questions](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md#open-questions-for-the-owner)); the ones that gate a specific task are listed in that task's own Open Questions. The ones that affect an existing requirement also appear as `> ❓ Question:` lines next to WIN-002, WIN-003, WIN-005, WIN-006, WIN-007, WIN-009, WIN-010, SM-002/SM-003, SM-007, NTF-001, GEN-006 and PROT-001/PROT-008. Questions with no requirement to attach to (Q7 UI autostart, Q13 uninstall data) are referenced under WIN-007 and WIN-010.

## Result

Status: review. A proposed ADR and the resulting planning are drafted; nothing is accepted.
The independent [review](../docs/reviews/2026-11-18-uncommitted.md) assigns technical pre-acceptance
clarifications to [T056](T056-clarify-proposed-service-design-after-review.md), which records its result
in its own [Result](T056-clarify-proposed-service-design-after-review.md#result): the safe creation and opening of
the data tree (D6, checks V13) and the console/service tray boundary (D10, checks V14, V15, M15, M16). The owner's
answers of 2026-11-18 ([decisions K1](../docs/spec/decisions.md)) are product decisions, not acceptance of this
ADR, and [T058](T058-reconcile-service-design-with-owner-decisions.md) reconciles the proposal with them. ADR
acceptance is not recorded, and clarifying the proposal does not authorize T047–T052 implementation.

What exists (documentation only):
- [ADR-0009](../docs/architecture/decisions/0009-windows-service-and-user-session-boundary.md), status Proposed. It records hazards H1–H13 traced in source, decisions D1–D10, alternatives, consequences, owner questions Q1–Q18 (Q17–Q18 were added by T056), assumptions A1–A30 and verification checks V1–V15 and M1–M16. If accepted it amends ADR-0007 (access limited to the current user).
- Planning for criterion 2: T016 is now the tray handoff and the acceptance gate over the new children T047 (contract additions, D9), T048 (S1), T049 (S2), T050 (S3), T051 (S4) and T052 (S5), each `planned` with area, dependencies, acceptance criteria and checks derived from the ADR. An explicit ordering and overlap table is in the ADR (Consequences). Planning notes were added to T003 (controller instantiable per OS-user partition), T005 (D9 delivered by T047, not by T005), T014 and T017 (T017 now also depends on T047). `ROADMAP.md`, `STATUS.md` and `REQUIREMENT_COVERAGE.md` are owned by the main agent and are not updated by this record.

Acceptance criteria status:
1. ADR content: drafted, not owner-reviewed. Met only as a proposal.
2. T016 boundaries and checks: planned as above; derived from a proposal that is not yet accepted. D9 contract additions are assigned to T047, ahead of its consumers T017 and T051.
3. ADR acceptance: not recorded. Not met.

Checks actually done (documentation only; nothing was built or run):
- Source traces of hazards H1–H13 and the line references of the ADR re-checked against the working tree on 2026-11-18 (H2, H4, H6, H10 and H11 fixed where they were imprecise).
- Link, task front matter, dependency graph and readiness checks for the new and changed task files, and the ADR status (Proposed), run by the second writer on 2026-11-18 with a throwaway script (all relative links and the ADR anchor resolve, front matter fields and section order are valid, the graph is acyclic, no `ready` task has an unaccepted dependency).
- Two Microsoft pages (`CreateProcessAsUser` and the .NET Windows service guide) were re-read on 2026-11-18 for the privilege, window-station DACL and Event Log statements. The other Microsoft references were not independently re-verified.

Limitations:
- Not verified: all Microsoft documentation claims other than the two above, and assumptions A1–A30, none of which has been validated on a real Windows system. The additions of T056 (D6 rules R1–R8, D10) are likewise untested.
- No code was written or run; no Windows session, reboot, access-isolation or service behavior was tested.
- The task ordering is a proposal for the main agent to integrate; for example, putting T013 before T048 is a choice, not a requirement.

Delegation: the ADR was drafted by a delegated tier-2 executor, reviewed independently, and revised in a second tier-2 round (this record). The main agent owns integration, the decision-record status and acceptance.
