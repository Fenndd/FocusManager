---
id: T061
title: Record profile offline billing and default decisions
milestone: M0
status: review
depends_on: []
area: ["AGENTS.md", "docs/spec/**", "docs/engineering/REQUIREMENT_COVERAGE.md", "tasks/T021-*", "tasks/T023-*", "tasks/T024-*", "tasks/T028-*", "tasks/T030-*", "tasks/T031-*", "tasks/T032-*", "tasks/T034-*", "tasks/T035-*", "tasks/T037-*", "tasks/T038-*", "tasks/T039-*", "tasks/T040-*", "tasks/T058-*", "tasks/T059-*", "tasks/T060-*", "tasks/T061-*"]
---

# T061 — Record profile offline billing and default decisions

## Goal

Record the owner's new answers about profiles, default exceptions, protection, independent device sessions,
offline behavior, budget, billing channels, consent and blocked notifications. Select a justified Android/
Windows default exception set within the owner's explicit delegation and keep implementation separate.

## Sources

- Owner answers 1–15 in chat on 2026-11-18, and prior clarification that Firebase Blaze is acceptable when needed.
- [Owner decisions](../docs/spec/decisions.md), [task lifecycle](../docs/engineering/TASK_FORMAT.md), [current handoff](../STATUS.md).

## Scope

In scope: dated confirmed decisions and deliberate supersession notes; relevant specification and planning
task updates; default exception categories/names justified by platform sources; scoped roadmap/status/coverage
integration. Preserve existing implementation, review evidence and other agents' unfinished work.

Out of scope: implementation, running/replacing active T054–T056 work, service/database/policy changes,
cloud billing/deployment, prices or free/paid tier selection, purchases, iOS/Linux development, commit/push,
or accepting implementation tasks/ADRs.

## Acceptance Criteria

1. Owner answers are reflected in canonical decisions and owning specifications, with no seeded profiles or cross-device start/stop behavior left as an active requirement.
2. Default exception lists, their dedicated UI and removability are concrete; any critical-system boundary and waiting-screen interpretation are based on owner clarification rather than an invented restriction.
3. Daily emergency defaults, friction waiting, startup selection, latest-change sync, offline local functions,
   EUR 5/month preference ceiling, chosen billing channels and owner-controlled tier/pricing hold are recorded accurately.
4. Consent timing, deferred popular-item collection/on-screen analysis, blocked-notification viewing during
   active sessions and optional switching cleanup are assigned to appropriate planning/implementation owners.
5. Task/roadmap/dependency/link/coverage checks and independent consistency review pass. No unimplemented
   behavior is marked done, and the Linux hold and existing acceptance gates remain intact.

## Checks

- Inspect requirement/decision/task consistency and scoped diff; verify relevant platform details from primary sources.
- Validate task fields/status/roadmap, acyclic dependencies, requirement coverage and local links; run scoped `git diff --check`.
- No .NET, device, browser, Linux or live account execution is required for this documentation work.

## Open Questions

None for recording these answers. The owner confirmed a separately displayed protected OS-component group
and waiting only while the FocusManager waiting screen is visible. Device mapping, visibility detection and
other implementation details remain with the owning development/planning tasks, not this documentation task.

## Result

Confirmed answers and delegated ordinary defaults recorded on 2026-11-18. Both requested clarifications
were subsequently answered and integrated; documentation work is complete and enters `review` for owner
acceptance. The former blocked state is resolved. This does not accept any implementation task or ADR.

Recorded L1–L6 and updated profiles/apps/protection/session/account/cloud/billing/privacy/content/platform
specifications and their planning tasks. Removed seeded profiles and active cross-device start/stop scope;
retained removed SYNC-004 for traceability. Added PRF-007 for last-used/explicit startup profile selection.
Selected removable Android Contacts/SMS/Clock/Calculator and Windows Explorer/Clock/Calculator under the
owner's delegation, with the Default Exceptions view and explicit installed-app mapping/runtime caveats.
Prepared a local Blocked notifications tab usable during sessions, and retained owner holds for commercial
choices, visible-text analysis, popular blocked-item collection and Linux.

Verified:
- 61 task records and roadmap rows agree; required section order, dependency targets and readiness were
  checked; the dependency graph is acyclic. T028 is ready for examples/model handoff, not accepted.
- All 108 requirement definitions/retained removed identifiers across 18 specification files are covered.
- 373 local file links resolve; scoped Markdown trailing-whitespace and `git diff --check` checks passed.
- Independent tier-2 read-only review found no blocking owner-answer mapping issue; two stale coverage
  descriptions were corrected and rechecked.
- Default selection used primary Android role/dialer and Windows critical-service references to distinguish
  ordinary conveniences from technical OS needs. No actual platform behavior is established by these docs.

Delegation: tier 2 GPT-6.1 Sol / medium prepared exclusive profile/protection and sync/billing documents,
provided read-only default-selection evidence, and independently reviewed consistency. The main agent
recorded canonical decisions, ordinary defaults, content/privacy and coordinated task/status/coverage changes.

Not performed: application/code changes, .NET/device/browser/Linux execution, database deletion, cloud
plan/resource changes, purchase/payment activation, commit or push. Existing T054/T055/T056 implementation,
task results and ADR edits were preserved and not accepted by this task.

Historical clarification blockers, explicitly resolved by the owner's follow-up on 2026-11-18:
1. Whether critical OS components are a separately displayed protected technical group, or also removable
   with explicit platform-limit reporting. Existing APP-005/AND-003 remain in force until clarified; no new
   locked ordinary exception is introduced.
2. Whether friction waiting requires the visible FocusManager waiting screen, or merely an illuminated
   device display while another app may be open. Pause/resume criteria must follow that answer.

Resolution: critical OS components are shown separately as Protected System Components and are not removable
ordinary app exceptions. Exit waiting progresses only while the FocusManager waiting screen is visible;
hiding/minimizing it, locking the device or turning its display off pauses progress, and visible return
resumes accumulated waiting. PROT-005 references this exit-wait rule. L1/L6, APP-005/APP-007, AND-003,
PROT-002, T032/T034, roadmap and STATUS are aligned; the current Open Questions section is None.

Follow-up verification: the main agent rechecked all 61 task/roadmap states and dependency consistency,
108 requirement identifiers/coverage, 373 local links, scoped whitespace and the clarification diff. The
earlier independent package review remains recorded above; this small answer addendum was checked by the
main agent, without claiming a second independent review or executing implementation tests.

Next action: owner acceptance of T061 remains separate. Eligible development tasks consume the recorded
decisions through their normal gates; no additional product answer is needed to complete this handoff.
