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

[sync.md](../docs/spec/sync.md), [monetization.md](../docs/spec/monetization.md). Requirement coverage: SYNC-001–SYNC-006, PAY-005.

## Scope

In scope: Resolve with the owner the first-version/no-cloud versus first-public-paid-release boundary (D1, PAY-005, PAY-006), without assuming an internal MVP interpretation. Obtain owner sign-in-method and cross-device-session tier decisions; record conflict/deletion/offline-reconnect behavior and account/subscription binding requirements, resolving missing product semantics with the owner. Produce a proposed architecture design for per-user data, platform separation and statistics/session merging with alternatives, cost assumptions and access-rule tests. No Firebase account changes or production client code.
Out of scope: production implementation, external resource changes, or deciding unspecified product behavior.

Planning change (T026, owner milestone request on 2026-11-17): moved from M6 to M10 to give this feature area a separate acceptance milestone. Task ID and requirement meaning are unchanged.

## Acceptance Criteria

1. The owner-approved first-version/no-cloud versus first-public-paid-release boundary is recorded next to the SYNC question with decision provenance; no internal-MVP interpretation is assumed.
2. The owner-selected sign-in methods, account/subscription binding and cross-device-session tier are explicit in the owning specifications; T023 can consume these decisions without choosing them.
3. A versioned synchronization contract describes owner-approved conflict, deletion, offline/reconnect, platform separation, statistics merging and cross-device session behavior. PRF-005 uses T028's approved rule. Significant architectural choices have an accepted ADR before implementation.
4. Per-user access-rule cases, cost assumptions and document-layout references are recorded. T023/T038 prerequisites and areas are updated to the accepted design.
5. Unanswered decisions prevent completion of this task; do not satisfy these criteria merely by creating downstream decision tasks or marking proposals accepted.

## Checks

- Compare task coverage with each linked specification and prerequisite output.
- Validate task metadata, dependency cycles, readiness and area conflicts.
- Review proposed execution checks against actual project tooling; commands not yet created must be marked unavailable.

## Open Questions

None

## Result

Not started. Created by T026 to make missing work or a split explicit. No implementation or checks have been completed.
