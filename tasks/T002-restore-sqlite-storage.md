---
id: T002
title: Restore local SQLite storage without Windows defaults
milestone: M1
status: review
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

Implemented by a delegated tier-2 worker under main-agent review (2026-11-17/18); awaiting owner acceptance.

Independent review on 2026-11-18 found no confirmed introduced storage defect. AC1 remains partly
verified; [T055](T055-verify-windows-sqlite-startup-and-restarts.md) owns the isolated Windows
application startup/restart check. See the [review](../docs/reviews/2026-11-18-uncommitted.md).
The owner's Linux hold defers non-Windows execution; it does not mark it verified. Historical runs below
precede T045/T001's final fixes; the independent review subsequently passed all 155 Windows-suite tests.

Changes:
- New `src/Shared/FocusManager.Persistence.Sqlite` (`Microsoft.Data.Sqlite` 8.0.34): `SqliteWhitelistStore` with the pre-2026-09-30 schema, and `SqliteWhitelistStoreOptions` (`DatabasePath`, `DefaultFolders`). The directory and schema are created lazily on first use; the `allow_subfolders` column migration runs in one transaction; `SaveAsync` replaces all lists in one transaction.
- The Windows agent supplies `%LOCALAPPDATA%\FocusManager\focusmanager.db` and the `C:\` default folder (FLD-004) in `Program.cs`; shared code has no Windows paths. `appsettings.json` and its csproj items are removed.
- `FocusManager.Persistence.Postgres` and its tests are removed; `FocusManager.sln` and both `.slnf` files reference the SQLite projects; PostgreSQL sections are removed from `README.md` and `AGENTS.md`.
- New `tests/Shared/FocusManager.Persistence.Sqlite.Tests` (35 tests, no external services), including legacy-schema and migration cases.

Checks:
- `dotnet test FocusManager.Shared.slnf` (Windows host, no database environment variables): Core 14, SQLite 35 passed. Verified by the main agent.
- `dotnet test FocusManager.Windows.slnf`: all passed except `AppEnforcerTests.EnforceAsync_Notifies_WhenDeniedProcessWasKilled` when started from Git Bash (PATH issue, tracked as T045, unrelated to storage). Verified by the main agent.
- Independent read-only review (tier 2): old-format database opens (AC2), no Windows defaults in shared persistence code and no Postgres/Npgsql references (AC3); verdict accept-with-fixes, only bookkeeping findings (this Result, STATUS).
- Repository grep: no PostgreSQL/Npgsql references remain outside historical documentation.

Not verified:
- Manual `Start-FocusManager.cmd` run (change data, restart, open a pre-existing `%LOCALAPPDATA%\FocusManager\focusmanager.db`): the agent starts elevated and a launch would touch the owner's real database. AC1 is therefore only partly verified (persistence across store instances is covered by tests).
- The new SQLite tests on a non-Windows host (no Linux host available).
- Real two-process sharing of one database file (covered only by an in-test second-connection lock test).

Known limitation (unchanged legacy behavior): entries differing only by case violate the UNIQUE NOCASE
constraint, so `SaveAsync` throws and rolls back (previous data kept). Correction from review:
`AgentHostedService.HandleClientAsync` returns a failed response, `AgentClient` surfaces the error and
UI save handlers display it in `StatusText`; the earlier claim of no user-facing message was inaccurate.
Validation/de-duplication remains separate future work, not an introduced storage defect.
