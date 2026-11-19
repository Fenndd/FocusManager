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

[monetization.md](../docs/spec/monetization.md), [sync.md](../docs/spec/sync.md), [L3 owner hold](../docs/spec/decisions.md#l3-billing-channels-and-owner-controlled-commercial-decisions). Requirement coverage: PAY-001–PAY-006.

## Scope

In scope: Plan flexible, secure entitlement/account-binding and payment interfaces with placeholders
for held commercial values. Plan Google Play billing and desktop Stripe using T024's feasibility
findings; preserve the deferred App Store boundary and Windows Microsoft Store distribution target
without assuming Microsoft billing. Any compatibility conflict returns to the owner. Include renewal,
cancellation, refund, expiry, replay/duplicate delivery, account switch and purchase recovery checks.
Separate technical infrastructure from prices, free/paid features and limits, trial eligibility and
offline paid-entitlement policy; these owner decisions stay held until the owner explicitly requests
them. All local functions remain offline-capable, without per-action cloud checks. No invented grace
period, tier matrix, all-free policy or alternative payment provider is permitted.
Out of scope: production implementation, external resource changes, choosing legal form or held
commercial values, Linux-specific work under J1 and iOS implementation. Neutral interface planning
may be separately tasked if authorized; this task grants no permission to code ahead of T024/T037.

Planning change (T026, owner milestone request on 2026-11-17): moved from M6 to M11 to give this feature area a separate acceptance milestone. Task ID and requirement meaning are unchanged.

Planning clarification (T061, owner answers on 2026-11-18): preserve `planned` and dependencies
T024/T037. Replace the open desktop-provider choice with selected Stripe and a compatibility gate.
Keep prices/features/limits/trial eligibility/offline-entitlement decisions explicitly owner-held;
this clarification neither starts T040 nor accepts any payment design.

## Acceptance Criteria

1. Every capability in scope is assigned to a one-session implementation or research/decision task, with sources, dependencies, precise areas and observable acceptance checks.
2. Held commercial questions stay in the owning specification and gate affected implementation tasks until the owner explicitly requests decisions. Infrastructure tasks use placeholders and do not predetermine those answers; new tasks follow TASK_FORMAT.md.
3. Roadmap and coverage links include all resulting tasks; completing this planning task does not complete the feature or milestone.

## Checks

- Compare task coverage with each linked specification and prerequisite output.
- Validate task metadata, dependency cycles, readiness and area conflicts.
- Review proposed execution checks against actual project tooling; commands not yet created must be marked unavailable.

## Open Questions

Held under L3: exact prices, free/paid features and limits, trial eligibility and offline paid-entitlement policy. T024 must resolve technical feasibility and return any Store/Stripe conflict to the owner; T037 must define account binding. These unresolved items are not permission to choose commercial behavior.

## Result

Not started. Created by T026 to make missing work or a split explicit. No implementation or checks have been completed.

2026-11-18 T061 clarification: task/specification consistency inspected; no planning deliverable, payment research, implementation or external change was completed for T040.
