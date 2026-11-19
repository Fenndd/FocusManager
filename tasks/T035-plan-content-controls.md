---
id: T035
title: Plan content filtering and notification controls
milestone: M8
status: ready
depends_on: []
area: ["tasks/**", "ROADMAP.md", "docs/spec/**", "docs/architecture/**"]
---

# T035 — Plan content filtering and notification controls

## Goal

Produce a bounded, source-linked implementation backlog and identify the decisions needed before coding this feature area.

## Sources

[content.md](../docs/spec/content.md), [notifications.md](../docs/spec/notifications.md). Requirement coverage: CNT-001–CNT-003, NTF-001–NTF-002.

## Scope

In scope: Create separate desktop feasibility tasks for address/search-query keyword matching, section
filtering and notification suppression, then per-platform implementation tasks after evidence (Android
uses T030). L4 defers general visible-text scanning; L5 requires a Blocked notifications tab usable during
active sessions. Preserve the distinction between suppressed app notifications (CNT) and FocusManager
notifications (NTF). Include local/offline inbox data, permissions, retention/clearing, reliability,
quick section-rule updates and bypass checks without claiming unavailable platform capabilities.
Out of scope: production implementation, external resource changes, or deciding unspecified product behavior.

Planning change (T026, owner milestone request on 2026-11-17): moved from M5 to M8 to give this feature area a separate acceptance milestone. Task ID and requirement meaning are unchanged.

## Acceptance Criteria

Owner update (T061, 2026-11-18): [L4/L5](../docs/spec/decisions.md#l4-registration-consent-and-deferred-content-collection)
limits keyword work to addresses/search queries and defers general visible-text analysis. CNT-003 now
requires a dedicated Blocked notifications tab usable during active sessions, with local/offline data.
Create bounded capability, notification capture/storage and UI tasks; establish retention/clear/sensitive
display behavior and test that opening entries does not bypass active app/site rules. Do not implement
notification-content cloud upload, resume Linux work, or substitute a post-session summary for this tab.

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
