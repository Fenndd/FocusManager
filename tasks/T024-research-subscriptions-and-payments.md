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

Research the feasibility and implementation choices for the owner-selected Google Play, App Store and desktop Stripe channels, and produce an entitlement/accounting design, implementation tasks and legal questions for the owner.

## Sources

Legacy classification: type `research`; platform `all`; priority `medium`.

Requirement identifiers: `PAY-001`, `PAY-002`, `PAY-003`, `PAY-005`, `PAY-006`, `SYNC-002`.

Legacy ADR associations: ADR-0006.

[Monetization](../docs/spec/monetization.md), [synchronization](../docs/spec/sync.md), [owner decisions](../docs/spec/decisions.md), and [ADR-0006](../docs/architecture/decisions/0006-local-storage-and-firebase.md).

Decisions G1–G6 retain the existing subscription/trial direction, subject to [L3](../docs/spec/decisions.md#l3-billing-channels-and-owner-controlled-commercial-decisions):
mobile purchases use Google Play/App Store, desktop uses Stripe, and exact prices, free/paid features,
limits, trial eligibility and offline paid-entitlement policy remain on owner hold. Legal form is decided
before the first paid release. Windows website and Microsoft Store distribution (H2) remain targets;
Store distribution does not automatically select Microsoft billing. iOS remains deferred and Linux
research is held under J1.

## Scope

In scope:
1. Research Google Play Billing subscriptions, trial mechanisms, cancellation, refunds and paywall requirements without choosing trial eligibility. Record the App Store integration boundary for later work; do not begin iOS implementation or platform development.
2. Research the selected desktop Stripe flow, EU VAT responsibilities and compatibility with the intended Windows website/Microsoft Store distribution. If a Store rule or capability conflicts, present sourced findings to the owner; do not silently replace Stripe, add Microsoft billing or drop Store distribution. Compare implementation options within the selected channels; provider alternatives require an owner decision before changing that selection.
3. Compare RevenueCat integration with custom Firebase subscription accounting, trusted validation and cross-platform account binding for the selected channels. Keep commercial values as placeholders and isolate offline-entitlement policy as an unresolved owner gate.
4. Identify legal requirements for selling from Slovakia (sole trade/company, DSA trader status in stores); produce questions for the owner and accountant.

Out of scope: production implementation, external resource/billing changes, Linux-specific work under J1,
iOS implementation, and selecting prices, tiers/limits, trial eligibility or offline-entitlement policy.
Legal choices and any proposed departure from the selected providers require the owner. Researching
technical feasibility can continue without resolving those held commercial choices.

Planning change (T026, owner milestone request on 2026-11-17): moved from M6 to M11 to give this feature area a separate acceptance milestone. Task ID and requirement meaning are unchanged.

Planning change (T061, owner answers on 2026-11-18): replace the open desktop provider comparison
with research of selected Stripe and Store-distribution compatibility; preserve `ready` research-only
status. L3's commercial hold gates affected implementation, not this technical feasibility comparison.

## Acceptance Criteria

1. A payments/subscription-accounting ADR exists; `monetization.md` and `sync.md` are updated.
2. Implementation tasks and a legal-action list for the owner exist, with held commercial decisions and any Store/Stripe incompatibility explicitly returned to the owner.
3. The research preserves Google Play/App Store/desktop Stripe, Windows Store distribution, offline local operation and the J1 hold; no silent provider substitution or tier/pricing decision is made.

## Checks

- Verify payment/store requirements, provider capabilities, and Slovak/EU legal sources current at research time; distinguish sourced facts, options, and decisions for the owner/accountant.
- Review the ADR, requirement updates, implementation-task dependencies, and legal-action list. No code checks apply to this research task.

## Open Questions

No owner answer blocks technical research. Store/Stripe compatibility and accounting integration must be established from current sources; commercial decisions stay on L3 hold.

## Result

Not started. No implementation or checks have been completed for this task.

2026-11-18 planning clarification by T061: scope checked against L3 and monetization.md; no provider research, legal validation or payment implementation was performed by this clarification.
