---
id: T010
title: Migrate legacy SQLite data to PostgreSQL
milestone: M1
status: cancelled
depends_on: []
area: ["src/Shared/FocusManager.Persistence.Postgres/**", "src/Windows/FocusManager.Agent/**"]
---

# T010 — Migrate legacy SQLite data to PostgreSQL

## Goal

Historically, make legacy SQLite data visible after the move to PostgreSQL.

## Sources

Legacy classification: type `feature`; platform `windows`; priority `low`.

Requirement identifiers: `WIN-006`.

Legacy ADR associations: ADR-0004.

[Windows persistence](../docs/spec/platforms/windows.md) and [ADR-0004](../docs/architecture/decisions/0004-postgresql-storage.md).

After the PostgreSQL migration, legacy SQLite data was not visible to the agent.

## Scope

Historical scope: migrate the old database to PostgreSQL. No implementation is authorized under this cancelled task.

## Acceptance Criteria

1. The cancellation reason and replacement storage task remain recorded.

## Checks

Inspect the cancellation record and its T002 / ADR-0006 references. No implementation checks apply to this cancelled task.

## Open Questions

None

## Result

Cancelled on 2026-11-16. PostgreSQL is being removed and storage returns to SQLite (ADR-0006, T002), compatible with the old database; no data transfer is needed.
