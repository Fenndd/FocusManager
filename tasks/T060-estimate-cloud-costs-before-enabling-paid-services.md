---
id: T060
title: Estimate cloud costs before enabling paid services
milestone: M10
status: ready
depends_on: []
area: ["tasks/T060-estimate-cloud-costs-before-enabling-paid-services.md", "docs/research/cloud-costs.md"]
---

# T060 — Estimate cloud costs before enabling paid services

## Goal

Produce a bounded, source-backed cloud cost comparison for a minimal Firebase design so the owner
can assess zero or minimal operating expenses before any paid service is enabled.

## Sources

- [Owner decision K3](../docs/spec/decisions.md#k3-cloud-availability-and-cost-constraint) and [L2 budget/offline clarification](../docs/spec/decisions.md#l2-account-synchronization-offline-operation-and-cloud-cost).
- [Account and synchronization](../docs/spec/sync.md), [monetization](../docs/spec/monetization.md),
  and [T037 account/synchronization contract](T037-resolve-account-and-sync-contract.md).
- [ADR-0006 Firebase](../docs/architecture/decisions/0006-local-storage-and-firebase.md).
- Vendor research entry points, to verify at execution time:
  [Firebase pricing plans](https://firebase.google.com/docs/projects/billing/firebase-pricing-plans),
  [Firebase FAQ](https://firebase.google.com/support/faq), and
  [Cloud Functions FAQ](https://firebase.google.com/docs/functions/faq-and-troubleshooting).

## Scope

In scope: research only. Compare a minimal combination of authentication, documents for accounts,
profiles/settings and synchronization, and the trusted backend needed for subscription entitlement
validation. Explain which components are required by the considered feature set, which can be
omitted or deferred, and what depends on the unresolved T037/T021 decisions. Compare Spark and
Blaze availability and costs for those services; identify services/deployment flows requiring a
billing account, including any selected Cloud Functions or other entitlement backend approach.

Use explicitly labelled feature/traffic assumptions rather than treating undecided design choices
as requirements. Model small and growing usage scenarios with active users, devices per user,
authentication methods, document sizes, reads/writes/listeners/reconnects, synchronization frequency,
storage, egress and backend invocations. State price date, region, currency, included quotas, unknowns
and the cost drivers that determine whether zero/minimal expense is feasible. Distinguish no-billing
operation, paid-plan usage within included quotas, and billable usage.

Explain what fails or degrades when service quotas are reached and what continues locally/offline.
Separate billing alerts from enforceable spend controls; identify service-specific quota controls,
any actual hard limits and their limitations. Do not describe an alert or a quota as a universal hard
monthly cost cap. Recommend a minimal approach, list any remaining owner decisions and evaluate
the EUR 0/month target and EUR 5/month accepted operating-budget ceiling from L2. Blaze is acceptable if
needed, but no actual upgrade is requested. Explain when the budget is feasible, its uncertainty
and what would need explicit approval before external paid configuration. Costs above EUR 5/month
require a new owner decision; any technical cost-control gap must be reported explicitly.

Out of scope: enabling billing/Blaze, creating or changing cloud resources or accounts, deployment,
credentials, production implementation, selecting sign-in methods or subscription semantics for the
owner, and Linux-specific research or validation under J1.

## Acceptance Criteria

1. `docs/research/cloud-costs.md` compares the minimal Firebase alternatives and explains the
   authentication/document/entitlement-backend components, their dependencies and any Blaze requirements
   using dated primary vendor sources.
2. The report gives reproducible small/growing usage estimates with labelled assumptions, current
   unit prices and quotas, uncertainty and cost drivers. It makes no promise of free operation at scale.
3. Quota exhaustion and offline consequences are recorded per selected service; alerts, quotas and
   enforceable cost limits are distinguished with their scope and gaps.
4. A recommendation evaluates the EUR 0/month target and EUR 5/month accepted operating-budget ceiling,
   identifies unresolved usage/design choices and explicitly requires owner approval before actual
   paid configuration. Blaze is acceptable if needed; no billing upgrade, hard cap or guarantee that
   costs stay within the budget is inferred from K3/L2.
5. Sources, actual checks and limitations are recorded; no external resource, access or billing change
   is made. The research may finish in `review` with later usage/design or actual configuration decisions still pending.

## Checks

- Open the cited primary pricing, quota and billing-control documents at execution time; record dates,
  relevant service/region scope and any discrepancies.
- Recalculate the scenario estimates from the recorded assumptions and unit prices; inspect both
  within-quota and over-quota cases. Record the actual calculation method; no repository calculation
  command exists yet.
- Compare the recommendations with K3/L2, Google/email sign-in, independent local sessions, offline local functions and the remaining T037/T021 technical gaps.
- Inspect the report and task diff and check local links; confirm that no cloud configuration,
  billing upgrade, deployment or access change was performed.

## Open Questions

None blocks research. Explore usage/design choices as labelled scenarios against the owner-selected
EUR 0 target and EUR 5/month accepted operating-budget ceiling. Blaze may be considered if needed; actual
paid configuration still requires authorization. Held commercial decisions are placeholders, not an
invitation to select prices or entitlement behavior. This task grants research permission only.

## Result

Not started. Created by T057 on 2026-11-18 as a `ready`, research-only follow-up to K3. No cloud cost
estimate, pricing validation, calculation, deployment or billing change has been performed by this task.

2026-11-18 T061 clarification: cost-research scope reconciled with L2. The previous undecided-budget question is resolved as a EUR 5/month accepted ceiling, not a technical hard cap or billing action. No pricing research, scenario calculation or cloud configuration was performed by this update; T060 remains `ready`.
