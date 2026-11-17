---
id: T034
title: Plan protection levels and bypass handling
milestone: M7
status: planned
depends_on: ["T021"]
area: ["tasks/**", "ROADMAP.md", "docs/spec/**", "docs/architecture/**"]
---

# T034 — Plan protection levels and bypass handling

## Goal

Produce a bounded, source-linked implementation backlog and identify the decisions needed before coding this feature area.

## Sources

[protection.md](../docs/spec/protection.md), [platforms/windows.md](../docs/spec/platforms/windows.md), [platforms/linux.md](../docs/spec/platforms/linux.md), [platforms/android.md](../docs/spec/platforms/android.md). Requirement coverage: PROT-001–PROT-010, WIN-007, WIN-010.

## Scope

In scope: Create separate tasks for normal/friction/code exit checks, active-session weakening rules, strict mode and preconfigured emergency exit, platform uninstall/settings feasibility and capability warnings. Separate shared policy from each OS integration/UI. Obtain PROT-002 friction and PROT-005 defaults, code handling and any missing timer/reboot semantics from the owner before their implementation. Keep partner features deferred.
Out of scope: production implementation, external resource changes, or deciding unspecified product behavior.

Planning change (T026, owner milestone request on 2026-11-17): moved from M5 to M7 to give this feature area a separate acceptance milestone. Task ID and requirement meaning are unchanged.

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
