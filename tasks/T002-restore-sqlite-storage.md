---
id: T002
title: Restore local SQLite storage without Windows defaults
milestone: M1
status: ready
depends_on: []
area: ["src/Shared/FocusManager.Persistence.Sqlite/**", "src/Shared/FocusManager.Persistence.Postgres/**", "tests/Shared/FocusManager.Persistence.Sqlite.Tests/**", "tests/Shared/FocusManager.Persistence.Postgres.Tests/**", "src/Windows/FocusManager.Agent/**", "FocusManager.sln", "*.slnf", "README.md", "AGENTS.md"]
---

# T002 — Restore local SQLite storage without Windows defaults

## Goal

Restore compatible local SQLite storage for desktop agents, with the database path and default folders supplied by each platform.

## Sources

Legacy classification: type `change`; platform `shared`; priority `high`.

Requirement identifiers: `WIN-006`, `GEN-005`, `FLD-004`, `SYNC-001`.

Legacy ADR associations: ADR-0006.

[ADR-0006](../docs/architecture/decisions/0006-local-storage-and-firebase.md), [Windows requirements](../docs/spec/platforms/windows.md), [product requirements](../docs/spec/product.md), [folder requirements](../docs/spec/folders.md), and [synchronization requirements](../docs/spec/sync.md).

ADR-0006 / owner decision C6 replaces PostgreSQL with SQLite. The current store is `src/Shared/FocusManager.Persistence.Postgres` (`PostgresWhitelistStore`) and requires a connection string. Before 2026-09-30, `SqliteWhitelistStore` existed; retrieve it with `git show 31e6e31:src/Shared/FocusManager.Persistence.Sqlite/SqliteWhitelistStore.cs`. Storage currently hardcodes the Windows rule that `C:\` is always allowed (FLD-004), although storage is shared with Linux.

## Scope

In scope:
1. Create `src/Shared/FocusManager.Persistence.Sqlite` using `Microsoft.Data.Sqlite`, with `SqliteWhitelistStore` compatible with the old schema and database location (`%LOCALAPPDATA%\FocusManager\focusmanager.db` on Windows). Pass the path as an argument so Linux can choose its own.
2. Supply default folders through options, for example `SqliteWhitelistStoreOptions.DefaultFolders`. The Windows agent supplies `C:\` (FLD-004); shared code contains no Windows paths.
3. Remove `FocusManager.Persistence.Postgres`, its tests, agent `appsettings.json` if no longer needed, and their references in `FocusManager.sln` / `*.slnf`.
4. Move storage tests, including migration of the old `allow_subfolders` schema, to `tests/Shared/FocusManager.Persistence.Sqlite.Tests`; tests require no external services.
5. Remove PostgreSQL sections from `README.md` and `AGENTS.md`.

Out of scope: the new profile/trigger schemas belong to T021 and the later model/storage migration belongs to T029.

Planning change (T026, 2026-11-17): Scope the storage criteria to persistence so unrelated Core path behavior does not silently expand this ready task. Keep the full-suite check and record any baseline failure.

## Acceptance Criteria

1. The agent starts without database configuration and preserves data across restarts.
2. An existing old-format SQLite database opens and can be read.
3. Shared persistence code contains no Windows-specific default paths; removed PostgreSQL/Npgsql project and package references are absent. Core path semantics remain T001.
4. SQLite persistence tests pass without database environment variables. Run the complete Shared suite and report unrelated failures separately; host-independent Core path fixes belong to T001.

## Checks

- Run `dotnet test FocusManager.Shared.slnf` without database environment variables; include old-schema and persistence tests.
- Run `dotnet test FocusManager.Windows.slnf` on Windows.
- Start with `Start-FocusManager.cmd`, change data, restart, and verify persistence and old-database compatibility.
- Inspect shared code, solution/filter references, and the PostgreSQL sections removed from `README.md` / `AGENTS.md`.

## Open Questions

None

## Result

Not started. No implementation or checks have been completed for this task.
