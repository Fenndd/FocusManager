---
id: T037
title: Resolve account and synchronization contracts
milestone: M10
status: planned
depends_on: ["T021"]
area: ["tasks/**", "ROADMAP.md", "docs/spec/**", "docs/architecture/**"]
---

# T037 — Resolve account and synchronization contracts

## Goal

Obtain the account/release/synchronization decisions and define the accepted contract needed by Firebase setup and client implementation.

## Sources

[sync.md](../docs/spec/sync.md), [monetization.md](../docs/spec/monetization.md), [decision K3](../docs/spec/decisions.md#k3-cloud-availability-and-cost-constraint), [L2 account/offline/cost](../docs/spec/decisions.md#l2-account-synchronization-offline-operation-and-cloud-cost), [L3 commercial hold](../docs/spec/decisions.md#l3-billing-channels-and-owner-controlled-commercial-decisions), and [T060 cost research](T060-estimate-cloud-costs-before-enabling-paid-services.md). Requirement coverage: SYNC-001–SYNC-003, SYNC-005–SYNC-006, PAY-005; removed SYNC-004 retained only for traceability.

## Scope

In scope: Apply K3, which permits cloud from the first version when needed and supersedes D1's no-cloud restriction; clarify the actual account/synchronization feature rollout with the owner without assuming an internal MVP interpretation. Apply Google/email sign-in, latest-change-wins conflicts, shared accounts with independent local sessions and offline local functions from L2; define deletion/offline-reconnect behavior and account/subscription binding requirements, resolving remaining missing product semantics with the owner. Do not revive removed SYNC-004 or design automatic cross-device start/stop. Produce a proposed architecture design for per-user data, platform separation and statistics/session-history merging with alternatives, cost assumptions and access-rule tests. No Firebase account changes or production client code.
Out of scope: production implementation, external resource changes, or deciding unspecified product behavior.

Planning change (T026, owner milestone request on 2026-11-17): moved from M6 to M10 to give this feature area a separate acceptance milestone. Task ID and requirement meaning are unchanged.

Planning clarification (T057, owner answers on 2026-11-18): K3 resolves the original cloud-availability
conflict only. This task remains `planned`; T021 and the remaining product/contract decisions are still
required. At that checkpoint no sign-in method, feature schedule, paid Firebase plan or monthly cap was selected; later L2/L3 decisions below supersede the resolved portions.

Planning clarification (T061, owner answers on 2026-11-18): L2 selects Google/email sign-in,
latest-change-wins conflicts and independent local sessions; removes SYNC-004; requires all local
functions offline; sets EUR 0/month target and EUR 5/month accepted as the operating-budget ceiling, with Blaze acceptable
if needed. L3 defers prices/tier limits/trial eligibility/offline-entitlement policy until an explicit
owner request. T021 remains a dependency and this contract task remains `planned`; no external change
or implementation is authorized by these answers.

## Acceptance Criteria

1. The contract applies K3's permission to use cloud from the first version when needed, preserves local-first/offline behavior, and records the owner-approved feature rollout. D1's superseded restriction is not used to block the design or infer an internal MVP.
2. Google/email sign-in, account/subscription binding and independent local sessions are explicit in the owning specifications; T023 can consume the resolved contract without choosing product behavior. Removed SYNC-004 remains reserved and no automatic remote session start/stop is specified.
3. A versioned synchronization contract implements latest-change-wins and defines ordering/ties, deletion, offline/reconnect, platform separation and statistics/history merging without controlling another device's active session. All local functions work offline without per-action cloud checks; login/registration, sync and store purchases are network-dependent. PRF-005 uses T028's approved rule. Significant architectural choices have an accepted ADR before implementation.
4. Per-user access-rule cases, cost assumptions (using T060 research where available) and document-layout references are recorded. The EUR 0/month target, accepted EUR 5/month operating-budget ceiling, Blaze permission if needed and owner approval gate before actual paid configuration remain explicit; the budget is not an enforceable technical cap or actual upgrade; costs above EUR 5/month require a new owner decision. The deferred paid-entitlement offline policy must not be invented to close this contract. T023/T038 prerequisites and areas are updated to the accepted design.
5. Unanswered decisions prevent completion of this task; do not satisfy these criteria merely by creating downstream decision tasks or marking proposals accepted.

## Checks

- Compare task coverage with each linked specification and prerequisite output.
- Validate task metadata, dependency cycles, readiness and area conflicts.
- Review proposed execution checks against actual project tooling; commands not yet created must be marked unavailable.

## Open Questions

- What first-release synchronization rollout, account/subscription binding, deletion/tombstone and reconnect details should the final contract use?
- What version/timestamp ordering and tie handling make latest-change-wins deterministic across devices, and how should statistics/history merge without cross-device session control?
- Prices, tier limits, trial eligibility and offline paid-entitlement policy remain deferred under L3; do not initiate those owner decisions or invent a grace period. Record their boundary explicitly if the neutral synchronization contract can be completed independently.

## Result

Contract work not started. Created by T026 to make missing work or a split explicit.

2026-11-18 clarification recorded by T057: cloud availability is resolved by K3; the remaining
contract work above has not started. Documentation was compared with sync.md and the owner's recorded
clarification; no Firebase configuration, synchronization implementation or cloud cost validation was performed.

2026-11-18 T061 clarification: owning specification and task coverage were reconciled with L2/L3. Google/email, conflicts, independent sessions and the EUR 5/month operating budget are resolved; the technical contract, T021 dependency and held commercial policy remain outstanding. No Firebase configuration or contract implementation was performed.
