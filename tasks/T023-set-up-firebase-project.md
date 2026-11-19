---
id: T023
title: Set up the Firebase project
milestone: M10
status: planned
depends_on: ["T021", "T037"]
area: []
---

# T023 — Set up the Firebase project

## Goal

Configure the owner-controlled Firebase project and test per-user Firestore access rules, with documented configuration handling and cost estimates.

## Sources

Legacy classification: type `chore`; platform `all`; priority `medium`.

Requirement identifiers: `SYNC-002`, `SYNC-003`, `SYNC-006`, `TEL-001`, `TEL-002`, `TEL-003`.

Legacy ADR associations: ADR-0006.

[Synchronization](../docs/spec/sync.md), [telemetry](../docs/spec/telemetry.md), [owner decisions](../docs/spec/decisions.md), and [ADR-0006](../docs/architecture/decisions/0006-local-storage-and-firebase.md).

Firebase provides accounts, synchronization, analytics, and crash reporting (decision D2). The owner must create the project in their Google account and grant access.

## Scope

In scope:
1. The owner creates a Spark-plan Firebase project and registers Android and web applications (web for Tauri).
2. Enable only the owner-approved Authentication methods from T037 (system-browser sign-in on desktop, ADR-0007) and Firestore. Analytics/Crashlytics activation follows the consent/data decisions from T039; project setup does not authorize collection.
3. Write Firestore security rules so users can read/write only their own documents; test them in the Firebase emulator.
4. Document application configuration storage and what may/may not be committed in `AGENTS.md`.
5. Estimate costs as free users with synchronization grow (SYNC-003).
6. Determine repository paths for Firebase configuration/rules/tests after project setup. `area` is empty until these paths are established; run sequentially.

Out of scope: application synchronization code, which belongs to T038 and its platform implementation tasks.

Planning change (T026, 2026-11-17): Remove the unsupported sign-in choice, separate telemetry collection from project setup and require the account/sync contract before access rules.

Planning change (T026, owner milestone request on 2026-11-17): moved from M6 to M10 to give this feature area a separate acceptance milestone. Task ID and requirement meaning are unchanged.

## Acceptance Criteria

1. The project exists and emulator tests cover security rules.
2. The repository contains no secrets, and configuration handling is documented.

## Checks

- Confirm the owner project/application registrations and enabled Firebase services through authorized account access.
- Run the rule-emulator test command documented when the configuration/tests are added; it does not exist yet.
- Review per-user authorization cases, secret/configuration handling, and the cost estimate assumptions.

## Open Questions

Google and email sign-in are selected by L2/SYNC-002; do not reopen that product choice. T037 still owns
the versioned account/sync contract and technical details. The owner must create/select the Firebase project
and provide the required access. Blaze is acceptable when needed within the EUR 5/month operating budget,
but no external project/billing change is performed by recording that direction. Establish configuration,
rules and emulator-test paths from the actual project and task; do not guess credentials or deploy without
the required authorization. All local functions remain offline-capable and SYNC-004 remote control is removed.

## Result

Not started. No implementation or checks have been completed for this task.
