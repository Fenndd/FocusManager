---
id: T035
title: Plan content filtering and notification controls
milestone: M8
status: ready
depends_on: []
area: ["tasks/**", "ROADMAP.md", "docs/spec/**", "docs/architecture/**"]
---

# T035 — Plan content filtering and notification controls

## Goal

Produce a bounded, source-linked implementation backlog and identify the decisions needed before coding this feature area.

## Sources

[content.md](../docs/spec/content.md), [notifications.md](../docs/spec/notifications.md). Requirement coverage: CNT-001–CNT-003, NTF-001–NTF-002.

## Scope

In scope: Create separate desktop feasibility tasks for keyword/section filtering and notification suppression, then per-platform implementation tasks after evidence (Android uses T030). Obtain owner answers about visible-text scanning and post-session notification summaries. Preserve the distinction between suppressing other apps notifications (CNT) and FocusManager notifications (NTF). Include permissions, reliability, quick section-rule updates and bypass checks.
Out of scope: production implementation, external resource changes, or deciding unspecified product behavior.

Planning change (T026, owner milestone request on 2026-11-17): moved from M5 to M8 to give this feature area a separate acceptance milestone. Task ID and requirement meaning are unchanged.

## Acceptance Criteria

1. Every capability in scope is assigned to a one-session implementation or research/decision task, with sources, dependencies, precise areas and observable acceptance checks.
2. Open owner questions stay in the owning specification and gate affected implementation tasks; new tasks follow TASK_FORMAT.md.
3. Roadmap and coverage links include all resulting tasks; completing this planning task does not complete the feature or milestone.

## Checks

- Compare task coverage with each linked specification and prerequisite output.
- Validate task metadata, dependency cycles, readiness and area conflicts.
- Review proposed execution checks against actual project tooling; commands not yet created must be marked unavailable.

## Open Questions

None

## Result

Not started. Created by T026 to make missing work or a split explicit. No implementation or checks have been completed.
