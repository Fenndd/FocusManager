---
id: T009
title: Bootstrap the Kotlin and Compose Android application
milestone: M3
status: planned
depends_on: ["T008"]
area: ["src/Android/**", ".github/workflows/ci.yml", "AGENTS.md", "docs/architecture/README.md"]
---

# T009 — Bootstrap the Kotlin and Compose Android application

## Goal

Provide a buildable Android application with persisted session state, localization, and a foreground-service scaffold for later feature work.

## Sources

Legacy classification: type `feature`; platform `android`; priority `medium`.

Requirement identifiers: `AND-001`, `AND-006`, `GEN-002`.

Legacy ADR associations: ADR-0005, ADR-0006, ADR-0007.

[Android requirements](../docs/spec/platforms/android.md), [product localization](../docs/spec/product.md), [ADR-0005](../docs/architecture/decisions/0005-native-stack-per-platform.md), [ADR-0006](../docs/architecture/decisions/0006-local-storage-and-firebase.md), and [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md).

Later Android tasks should build features on an existing application skeleton rather than recreate infrastructure.

## Scope

In scope:
1. Create a Kotlin / Jetpack Compose Gradle project in `src/Android/`, minSdk 26 (AND-006), with README instructions for building, emulator launch, and tests.
2. Add a Room database with a minimal session-state entity and a test.
3. Add one screen with a session switch that persists across restarts.
4. Localize from the start: resource strings in English and Russian (GEN-002).
5. Add a foreground-service scaffold without blocking; check that the service survives closing the screen.
6. Document Android build/test commands in `AGENTS.md`, `src/Android/README.md`, and `docs/architecture/README.md`.
7. Add the Android CI job if T006 is complete.

Out of scope: actual blocking and Firebase; these belong to later M3 tasks.

## Acceptance Criteria

1. `./gradlew build` and tests pass on a machine with Android SDK.
2. The emulator launches the application; the switch state survives restart.
3. UI text is stored only in resources, including `values` and `values-ru`.

## Checks

- From `src/Android`, run `./gradlew build` after this task creates the wrapper; record the exact test task from the new README and run it. Those Gradle commands are not available before bootstrap.
- On an emulator, toggle the session, restart, and verify persistence; close the screen and verify foreground-service survival.
- Inspect localized resources and the conditional Android CI job.

## Open Questions

None

## Result

Not started. No implementation or checks have been completed for this task.
