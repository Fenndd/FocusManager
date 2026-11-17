# ADR-0004: PostgreSQL Instead of SQLite

- Status: **Superseded by ADR-0006** (2026-11-16)
- Date: 2026-09-30

## Context

Settings and allowlists were stored in a local SQLite file (`%LOCALAPPDATA%\FocusManager\focusmanager.db`).
The product owner decided to switch to PostgreSQL. The application was originally built for Windows.

## Decision

- The `FocusManager.Persistence.Sqlite` project was replaced by `src/Shared/FocusManager.Persistence.Postgres`
  (Npgsql driver); `PostgresWhitelistStore` implements the same `IWhitelistStore`, leaving the rest of the code unchanged.
- Connection string: `ConnectionStrings:FocusManager` in agent configuration
  (`src/Windows/FocusManager.Agent/appsettings.json` or the `ConnectionStrings__FocusManager` environment variable).
  Passwords are not committed to the repository: use an environment variable, `PGPASSWORD`, or `pgpass.conf`.
- The store creates the schema (tables `settings`, `allowed_apps`, `allowed_folders`, `allowed_sites`)
  on first access (`CREATE TABLE IF NOT EXISTS`, under an advisory lock). The database and role are created beforehand.
- Entries are unique without regard to case (unique indexes on `lower(...)`), as in SQLite (`COLLATE NOCASE`).
- Storage tests use a real PostgreSQL instance: the connection string is in `FOCUSMANAGER_TEST_POSTGRES`,
  and each test uses its own temporary schema. Without the variable, tests are skipped rather than failed.

## Alternatives

- **Keep SQLite.** No server is needed, but this contradicts the owner's decision.
- **EF Core + migrations.** Excessive for four tables; reconsider as the schema grows
  (or use a lightweight migration tool such as DbUp).

## Consequences

- The agent requires an available PostgreSQL instance. If the connection string is missing, the agent does not start
  and reports a clear error; if the server is unavailable, startup/operations fail (see the question in `docs/spec/platforms/windows.md`, WIN-006).
- Data from the old SQLite database is not migrated automatically (task `T010`).
- Developers and CI need PostgreSQL for storage tests (task `T006`: a PostgreSQL service in CI).
- **Android:** connecting a phone directly to PostgreSQL is not recommended (credentials inside the APK,
  a database port exposed to the network). If Android must use the same data, a server API is needed
  between the application and database; if Android data is local, local storage is needed. Task `T008`
  resolves this question; until then, only the Windows agent uses `FocusManager.Persistence.Postgres`.
