---
id: T033
title: Plan launch methods and resilient session timers
milestone: M6
status: planned
depends_on: ["T021"]
area: ["tasks/**", "ROADMAP.md", "docs/spec/**", "docs/architecture/**"]
---

# T033 — Plan launch methods and resilient session timers

## Goal

Produce a bounded, source-linked implementation backlog and identify the decisions needed before coding this feature area.

## Sources

[triggers.md](../docs/spec/triggers.md), [study-mode.md](../docs/spec/study-mode.md), [protection.md](../docs/spec/protection.md). Requirement coverage: TRG-001–TRG-006, PROT-010.

## Scope

In scope: Create separate tasks for timed/manual sessions, schedules, time budgets, launch-count limits, Wi-Fi and Android geofences, and combining triggers. Split shared runtime, desktop adapters/UI and Android implementation. Define and obtain decisions for reset/timezone/DST/reboot semantics where absent; use adjustable-clock tests and preserve PROT-010.
Out of scope: production implementation, external resource changes, or deciding unspecified product behavior.

Planning change (T026, owner milestone request on 2026-11-17): moved from M5 to M6 to give this feature area a separate acceptance milestone. Task ID and requirement meaning are unchanged.

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
