---
id: T008
title: Research Android blocking mechanisms
milestone: M3
status: ready
depends_on: []
area: ["docs/architecture/decisions/**", "docs/spec/platforms/android.md", "tasks/**", "ROADMAP.md"]
---

# T008 — Research Android blocking mechanisms

## Goal

Validate Android blocking hypotheses with prototypes and record implementable mechanisms, limitations, and the Android MVP tasks.

## Sources

Legacy classification: type `research`; platform `android`; priority `high`.

Requirement identifiers: `AND-001`, `AND-002`, `AND-003`, `AND-004`, `AND-005`, `AND-006`, `AND-007`, `APP-001`, `SITE-001`, `SITE-005`, `SITE-006`, `CNT-001`, `CNT-002`, `CNT-003`, `PROT-007`, `PROT-008`.

Legacy ADR associations: ADR-0005.

[Android requirements](../docs/spec/platforms/android.md), [application rules](../docs/spec/apps.md), [site rules](../docs/spec/sites.md), [content](../docs/spec/content.md), [protection](../docs/spec/protection.md), [triggers](../docs/spec/triggers.md), [ADR-0005](../docs/architecture/decisions/0005-native-stack-per-platform.md), and [ADR-0006](../docs/architecture/decisions/0006-local-storage-and-firebase.md).

The stack is already Kotlin + Jetpack Compose, Room, and Firebase. Mechanisms in `docs/spec/platforms/android.md` are hypotheses that need prototype validation before main development.

## Scope

In scope:
1. Prototype application launch detection, blocking-screen behavior, essential system exceptions, service restart and power-management constraints on a documented API-26/current-version sample.
2. Check official store requirements for Accessibility and foreground/background operation relevant to this foundation.
3. Keep prototypes outside src/ and uncommitted, or in a separately authorized research branch. Record actual commands and evidence.
4. Record the foundational Android mechanisms ADR and identify remaining version/device gaps. T044 owns website methods; T030 owns advanced content/triggers/protection; T031 owns enforcement task decomposition.

Out of scope: production Android code (T009 and tasks created by T031), website/advanced prototype matrices moved to T044/T030.

Migration note: ADR-0008 is taken by infrastructure; allocate the next available ADR number.

Planning change (T026, 2026-11-17): Split the oversized research matrix into application/service foundation T008, website methods T044 and advanced capabilities T030.

## Acceptance Criteria

1. The foundational Android mechanisms ADR records evidence, permissions, restrictions and owner acceptance explicitly.
2. Only tested hypotheses are promoted; remaining research stays in T044/T030 or named child tasks.
3. T009/T031 prerequisites match the evidence; no research outcome silently removes a required feature.

## Checks

- Execute the application/service prototype sample and record actual commands, Android/device versions, permissions, lifecycle results and limits.
- Verify relevant official Android/Google Play requirements at research time.
- Inspect ADR acceptance and T009/T030/T031/T044 boundaries; no prototype command exists yet.

## Open Questions

None

## Result

Not started. No implementation or checks have been completed for this task.
