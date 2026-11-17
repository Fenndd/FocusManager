---
id: T040
title: Plan trials billing and shared entitlements
milestone: M11
status: planned
depends_on: ["T024", "T037"]
area: ["tasks/**", "ROADMAP.md", "docs/spec/**", "docs/architecture/**"]
---

# T040 — Plan trials billing and shared entitlements

## Goal

Produce a bounded, source-linked implementation backlog and identify the decisions needed before coding this feature area.

## Sources

[monetization.md](../docs/spec/monetization.md), [sync.md](../docs/spec/sync.md). Requirement coverage: PAY-001–PAY-006.

## Scope

In scope: Create separate tasks for the owner-approved free/paid feature matrix and prices/trial eligibility, server-side entitlement/account binding, Google Play billing, website/Microsoft Store/Linux payment paths selected in T024, and platform paywalls/restoration/offline access. Include renewal, cancellation, refund, expiry, replay/duplicate delivery, account switch and purchase recovery checks. Do not choose payment providers, tier limits or legal form for the owner.
Out of scope: production implementation, external resource changes, or deciding unspecified product behavior.

Planning change (T026, owner milestone request on 2026-11-17): moved from M6 to M11 to give this feature area a separate acceptance milestone. Task ID and requirement meaning are unchanged.

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
