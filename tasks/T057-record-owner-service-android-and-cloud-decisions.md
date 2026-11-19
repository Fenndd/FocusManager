---
id: T057
title: Record owner service Android and cloud decisions
milestone: M0
status: review
depends_on: []
area: ["docs/spec/decisions.md", "docs/spec/study-mode.md", "docs/spec/platforms/windows.md", "docs/spec/platforms/android.md", "docs/spec/protection.md", "docs/spec/notifications.md", "docs/spec/sync.md", "docs/engineering/REQUIREMENT_COVERAGE.md", "AGENTS.md", "tasks/T008-*", "tasks/T011-*", "tasks/T034-*", "tasks/T037-*", "tasks/T057-*", "tasks/T058-*", "tasks/T059-*", "tasks/T060-*"]
---

# T057 — Record owner service Android and cloud decisions

## Goal

Record the owner's 2026-11-18 answers without presenting unimplemented behavior as complete, and provide the follow-up work needed to reconcile the proposed service design and cost-constrained cloud planning.

## Sources

- Owner chat answers 1–16, Android answers 1–6, Opera mechanism question and cloud cost constraint on 2026-11-18.
- Owner follow-up: one service for all protection levels; restrictions must also be cleared after a crash by a separate mechanism.
- [Task lifecycle](../docs/engineering/TASK_FORMAT.md), [owner decisions](../docs/spec/decisions.md), [current handoff](../STATUS.md).

## Scope

In scope: confirmed product decisions, explicit supersession/clarifications of earlier decisions, associated specification/task notes, bounded design/research tasks, and an evidence-based answer about Opera and cloud costs.

Out of scope: implementation, service installation, deleting any database, cloud/billing changes, ADR acceptance, Linux work, or modifying the concurrent T054/T055/T056 implementation/design work. The main agent integrates only bounded additive status/roadmap updates.

## Acceptance Criteria

1. Confirmed answers are recorded with concrete behavior and links; remaining questions are accurately distinguished from accepted answers.
2. Windows recovery, crash cleanup, user/session isolation, startup toggles and update/uninstall behavior are not conflated; existing conflicts have an explicit reconciliation owner.
3. Android approvals preserve policy/prototype gates, and the cloud decision permits needed services subject to zero/minimal cost without authorizing billing or promising free operation.
4. Linux hold and all unaccepted task/ADR states are preserved; task/link/dependency/documentation checks pass.

## Checks

- Inspect specification/decision/task consistency, relative links, task IDs, roadmap states and dependency graph.
- Verify technical Opera and Firebase statements against primary documentation; distinguish missing evidence from verified limitations.
- Inspect the scoped diff and `git diff --check`; do not run application or Linux checks for this documentation task.

## Open Questions

No further answers are required to record the confirmed decisions. The proposed service architecture, crash cleanup mechanism, same-user multiple-session site-policy isolation, Opera mechanism and exact cloud spending ceiling remain design/owner gates before their implementation or paid activation.

## Result

Recorded on 2026-11-18; documentation result awaits owner acceptance (`review`). The owner's product
answers are confirmed decisions, distinct from acceptance of this documentation task or implementation.

Changes: appended K1–K4 with deliberate historical D1/I5 supersession notes; updated Windows/session/
protection/notification and Android/cloud specifications without promoting unimplemented behavior to done.
Recorded PROT-012, planned T058/T059 and ready research-only T060. Updated T008/T011/T034/T037, coverage,
AGENTS and bounded status/roadmap notes. The concurrent T054/T055/T056 source, task records, evidence and
Proposed ADR were not edited by this task; T058 serializes later ADR reconciliation after T056.

Verified:
- 60 task IDs and roadmap entries agree; required front matter/section order and dependency references
  are valid, graph is acyclic, and ready tasks have no unaccepted dependencies.
- 107 unique requirement definitions across 18 specification files are present in the coverage map;
  new PROT-012 is assigned to T034/T059.
- 333 scoped relative local links resolve, and scoped Markdown trailing-whitespace checks passed.
- Scoped `git diff --check` passed; Git's CRLF notices are not check failures.
- Independent tier-2 read-only review found owner-answer mapping faithful and no substantive defect.
  Its source-coverage polish for T034/PROT-012 was integrated and rechecked.
- Primary Opera documentation confirms conditional Chromium-extension compatibility; it does not
  establish managed URL-policy compatibility. No current Opera runtime policy result is claimed.
- Primary Firebase pricing/FAQ documents distinguish Spark/no billing, Blaze requirements/included
  quotas, and budget alerts versus service-specific controls. No universal hard budget or free-at-scale
  guarantee is inferred. T060 still owns an actual scenario cost estimate.

Delegation: tier 2 GPT-6.1 Sol / medium prepared Android and cloud/Opera documents in exclusive paths;
another tier-2 agent independently reviewed mapping/consistency. The main agent recorded canonical
decisions and Windows specifications and integrated/checked the shared indexes.

Not performed: implementation, .NET/device/browser/Linux runs, cloud/billing/resource changes, database
deletion, service installation, commit or push. Actual service/crash/session behavior and Opera capability
remain unverified. These documentation changes do not accept ADR-0009 or the running review-closeout tasks.
