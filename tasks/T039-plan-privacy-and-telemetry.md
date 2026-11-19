---
id: T039
title: Plan consent privacy and technical telemetry
milestone: M10
status: ready
depends_on: []
area: ["tasks/**", "ROADMAP.md", "docs/spec/**", "docs/architecture/**"]
---

# T039 — Plan consent privacy and technical telemetry

## Goal

Produce a bounded, source-linked implementation backlog and identify the decisions needed before coding this feature area.

## Sources

[telemetry.md](../docs/spec/telemetry.md), [sync.md](../docs/spec/sync.md), [content.md](../docs/spec/content.md). Requirement coverage: TEL-001–TEL-005.

## Scope

In scope: Create separate tasks for data inventory/privacy review, registration-time consent and withdrawal,
crash reporting and usage-event instrumentation, and checks preventing list/notification content in telemetry.
L4 defers popular blocked-item collection and does not infer guest consent. Review actual consent/data flows
for release without treating the owner's product instruction as a legal conclusion. Include local blocked-
notification inbox retention/clearing and sensitive content in the inventory. Feed reviewed flows to T015.
Out of scope: production implementation, external resource changes, or deciding unspecified product behavior.

Planning change (T026, owner milestone request on 2026-11-17): moved from M6 to M10 to give this feature area a separate acceptance milestone. Task ID and requirement meaning are unchanged.

## Acceptance Criteria

Owner update (T061, 2026-11-18): [L4](../docs/spec/decisions.md#l4-registration-consent-and-deferred-content-collection)
defers popular blocked-item analytics and requests consent at registration. Distinguish crash and usage
purposes, preserve withdrawal and do not infer consent for guests/account creation. Local functions must
not depend on registration, consent or network. Incorporate the local blocked-notification inbox data flow
from CNT-003 into the privacy/storage review without authorizing content upload. Legal review remains a
release gate; the owner answer is a product instruction, not a legal conclusion.

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
