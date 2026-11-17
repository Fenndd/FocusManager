---
id: T036
title: Plan local statistics and progress views
milestone: M9
status: planned
depends_on: ["T021"]
area: ["tasks/**", "ROADMAP.md", "docs/spec/**", "docs/architecture/**"]
---

# T036 — Plan local statistics and progress views

## Goal

Produce a bounded, source-linked implementation backlog and identify the decisions needed before coding this feature area.

## Sources

[stats.md](../docs/spec/stats.md), [product.md](../docs/spec/product.md). Requirement coverage: STAT-001–STAT-005, GEN-005.

## Scope

In scope: Create separate tasks for platform activity collection, local session/block-event records, aggregation and desktop/Android charts/history/progress UI. Resolve undefined measurement/retention and achievement/exam-mode behavior with the owner. Preserve offline storage, keep technical telemetry separate, and leave cloud merging to T038. Daily streaks are specified; do not invent achievement rules.
Out of scope: production implementation, external resource changes, or deciding unspecified product behavior.

Planning change (T026, owner milestone request on 2026-11-17): moved from M5 to M9 to give this feature area a separate acceptance milestone. Task ID and requirement meaning are unchanged.

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
