---
id: T024
title: Research cross-platform subscription payments
milestone: M11
status: ready
depends_on: []
area: ["docs/architecture/decisions/**", "docs/spec/monetization.md", "docs/spec/sync.md", "tasks/**", "ROADMAP.md"]
---

# T024 — Research cross-platform subscription payments

## Goal

Compare payment/subscription accounting approaches for Android and desktop and produce implementation tasks plus legal questions for the owner.

## Sources

Legacy classification: type `research`; platform `all`; priority `medium`.

Requirement identifiers: `PAY-001`, `PAY-002`, `PAY-003`, `PAY-005`, `PAY-006`, `SYNC-002`.

Legacy ADR associations: ADR-0006.

[Monetization](../docs/spec/monetization.md), [synchronization](../docs/spec/sync.md), [owner decisions](../docs/spec/decisions.md), and [ADR-0006](../docs/architecture/decisions/0006-local-storage-and-firebase.md).

Decisions G1–G6 specify 1/3/6/12-month subscriptions, a seven-day auto-renewing trial, no lifetime purchase or advertising, one subscription across platforms, and paid features from the first public release. Legal form is decided before the first paid release. Android uses Google Play; desktop distributes through the product website and Microsoft Store (H2).

## Scope

In scope:
1. Research Google Play Billing subscriptions, trials, cancellation, refunds, and paywall requirements.
2. Compare website payment providers (for example Paddle, Lemon Squeezy, Stripe) with EU VAT handling, plus Microsoft Store purchases.
3. Compare RevenueCat with custom Firebase subscription accounting and account binding across platforms.
4. Identify legal requirements for selling from Slovakia (sole trade/company, DSA trader status in stores); produce questions for the owner and accountant.

Out of scope: implementation and prices. Provider/legal choices arising from this research must be resolved by the owner rather than assumed by the agent.

Planning change (T026, owner milestone request on 2026-11-17): moved from M6 to M11 to give this feature area a separate acceptance milestone. Task ID and requirement meaning are unchanged.

## Acceptance Criteria

1. A payments/subscription-accounting ADR exists; `monetization.md` and `sync.md` are updated.
2. Implementation tasks and a legal-action list for the owner exist.

## Checks

- Verify payment/store requirements, provider capabilities, and Slovak/EU legal sources current at research time; distinguish sourced facts, options, and decisions for the owner/accountant.
- Review the ADR, requirement updates, implementation-task dependencies, and legal-action list. No code checks apply to this research task.

## Open Questions

None

## Result

Not started. No implementation or checks have been completed for this task.
