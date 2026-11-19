---
id: T021
title: Define the profile data model and synchronization format
milestone: M5
status: planned
depends_on: ["T005", "T028"]
area: ["contracts/**", "docs/architecture/**"]
---

# T021 — Define the profile data model and synchronization format

## Goal

Define versioned profile and synchronization schemas independently of storage migration and feature implementation.

## Sources

Legacy classification: type `feature`; platform `all`; priority `high`.

Requirement identifiers: `PRF-001`, `PRF-002`, `PRF-003`, `PRF-005`, `PRF-006`, `TRG-001`, `TRG-002`, `TRG-003`, `TRG-004`, `TRG-005`, `TRG-006`, `PROT-001`, `SYNC-003`, `SYNC-005`, `STAT-005`.

Legacy ADR associations: ADR-0005, ADR-0006.

[Profiles](../docs/spec/profiles.md), [triggers](../docs/spec/triggers.md), [protection](../docs/spec/protection.md), [synchronization](../docs/spec/sync.md), [statistics](../docs/spec/stats.md), [ADR-0005](../docs/architecture/decisions/0005-native-stack-per-platform.md), and [ADR-0006](../docs/architecture/decisions/0006-local-storage-and-firebase.md).

The current `WhitelistConfig` contains one whitelist set. The specification introduces whitelist/blacklist profiles with lists, triggers, and protection levels; Firebase synchronizes Android, Windows, and Linux. All platforms need the same document format.

## Scope

In scope:
1. JSON Schema for profiles and platform-specific lists, triggers, protection settings and statistics documents, with versioning and compatibility rules.
2. Document the proposed Firestore document layout in docs/architecture/; unresolved account/conflict behavior belongs to T037.
3. Validate representative documents using T005 tooling and owner-approved PRF-005 examples from T028. Add an ADR for significant format decisions.

Out of scope: C# storage models and data migration (T029), profile runtime/UI (T032), trigger execution (T033), cloud connection (T023/T038), resolving PRF-005 (T028).

Planning change (T026, 2026-11-17): Split the former combined schema/model/migration task: T028 owns the product decision; T021 owns formats; T029 owns migration. Prior task content remains in Git history and this split preserves its required outputs.

## Acceptance Criteria

Owner update (T061, 2026-11-18): L1/L2 settle combined restrictions, no seeded profiles, last-used/explicit
startup selection (PRF-007), Google/email sign-in and latest-change-wins. Model user/account data separately
from independent device-local active sessions; SYNC-004 is removed and no remote start/stop schema is required.
Carry version/order/deletion metadata needed by T037 without choosing a hidden conflict policy. Linux-only
implementation/validation remains on hold; format planning does not resume that platform.

1. Versioned schemas and valid/invalid sample documents exist; compatibility rules are documented and schema validation passes.
2. Documents express the owner-approved composition rule without inventing trigger, protection, account or cloud conflict semantics.
3. The proposed cloud layout identifies decisions deferred to T037; no persisted-data migration occurs here.

## Checks

- Run the contract generation/validation command documented by T005 against valid/invalid examples; record the actual command.
- Review version compatibility and PRF-005 examples against T028; inspect documented Firestore boundaries.
- Documentation/schema checks apply; migration tests belong to T029.

## Open Questions

None after T028 is accepted. Do not infer the simultaneous-profile rule.

## Result

Not started. T026 split the original scope; no schema implementation or migration checks have run.
