---
id: T029
title: Migrate desktop whitelist data to the profile model
milestone: M5
status: planned
depends_on: ["T002", "T021"]
area: ["src/Shared/FocusManager.Core/**", "src/Shared/FocusManager.Persistence.Sqlite/**", "tests/Shared/**", "tests/Windows/**"]
---

# T029 — Migrate desktop whitelist data to the profile model

## Goal

Apply the agreed versioned profile schema to desktop storage without losing current whitelist entries or session state.

## Sources

[Profiles](../docs/spec/profiles.md) (PRF-001, PRF-003), [synchronization](../docs/spec/sync.md) (SYNC-001, SYNC-005), T021 schema and T002 SQLite output.

## Scope

In scope: C# model mapping, versioned SQLite migration into the initial Study profile, compatibility/error handling and fixture-based data comparisons.
Out of scope: profile UI, triggers, cloud connection or changing approved schema semantics.

## Acceptance Criteria

1. Current whitelist data and options survive migration into the initial profile and reopening.
2. Tests cover old/new schema, repeated open, failed/interrupted migration and explicit incompatible-version handling.

## Checks

- Run dotnet test FocusManager.Shared.slnf and dotnet test FocusManager.Windows.slnf on Windows.
- Migrate disposable fixture copies and compare every entry/option and session state; never use the only copy of user data.

## Open Questions

None

## Result

Not started. Created by T026 to make missing work or a split explicit. No implementation or checks have been completed.
