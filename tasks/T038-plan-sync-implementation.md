---
id: T038
title: Plan account and synchronization implementation
milestone: M10
status: planned
depends_on: ["T023", "T037"]
area: ["tasks/**", "ROADMAP.md", "docs/spec/**", "docs/architecture/**"]
---

# T038 — Plan account and synchronization implementation

## Goal

Produce a bounded, source-linked implementation backlog and identify the decisions needed before coding this feature area.

## Sources

[sync.md](../docs/spec/sync.md), [stats.md](../docs/spec/stats.md), [monetization.md](../docs/spec/monetization.md). Requirement coverage: SYNC-001–SYNC-006, STAT-005, PAY-005.

## Scope

In scope: Create separate tasks for desktop system-browser authentication and Android sign-in, local-first profile/settings sync, platform-specific lists, statistics merge, account backup/restore, and cross-device sessions. Include user isolation, logout/account switch, conflicts, offline/reconnect, duplication and deletion tests; preserve account-free blocking and free basic sync. Use T037 contracts rather than inventing conflict policies.
Out of scope: production implementation, external resource changes, or deciding unspecified product behavior.

Planning change (T026, owner milestone request on 2026-11-17): moved from M6 to M10 to give this feature area a separate acceptance milestone. Task ID and requirement meaning are unchanged.

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
