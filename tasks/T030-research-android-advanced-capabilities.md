---
id: T030
title: Research Android content triggers and protection
milestone: M3
status: planned
depends_on: ["T008"]
area: ["docs/spec/platforms/android.md", "docs/architecture/decisions/**", "tasks/**", "ROADMAP.md"]
---

# T030 — Research Android content triggers and protection

## Goal

Validate advanced Android mechanisms separately from the application/site foundation.

## Sources

[Content](../docs/spec/content.md) (CNT-001–CNT-003), [triggers](../docs/spec/triggers.md) (TRG-003–TRG-005), [protection](../docs/spec/protection.md) (PROT-007, PROT-008), [Android](../docs/spec/platforms/android.md).

## Scope

In scope: one bounded prototype question at a time for content recognition, notification hiding, usage/launch tracking, geofence/Wi-Fi and strict-mode capabilities. Record version/device evidence, permissions, bypasses and current official store requirements. Create separate research child tasks before executing a matrix too large for one session.
Out of scope: production feature implementation or weakening requirements without owner approval.

## Acceptance Criteria

1. Each advanced capability has evidence or a named remaining research task; untested hypotheses retain research status.
2. Accepted mechanisms have separate implementation tasks per capability, permission flow and checks; unresolved feasibility/owner choices remain explicit.

## Checks

- Record actual prototype commands, Android/browser/device versions and outcomes.
- Check current official platform/store sources when researching. Review generated task scopes, dependencies and specification status evidence.

## Open Questions

None

## Result

Not started. Created by T026 to make missing work or a split explicit. No implementation or checks have been completed.
