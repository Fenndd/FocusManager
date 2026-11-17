# ADR-0006: Local Storage on the Device + Firebase for the Cloud

- Status: Accepted
- Date: 2026-11-16
- Supersedes: ADR-0004

## Context

ADR-0004 moved Windows-agent storage to PostgreSQL. This requires a database server installed
on the user's computer and contradicts the requirement to work without internet or setup (GEN-005).
Owner decisions: the first version has no cloud (D1), the cloud uses Firebase (D2), sync and backup
require an account (D3, D5), and desktop storage uses SQLite (C6).

## Decision

- **Local storage on each device** is the source of blocking rules and works offline:
  - Windows and Linux: SQLite (`Microsoft.Data.Sqlite`), project `FocusManager.Persistence.Sqlite`;
  - Android: Room (SQLite);
  - iOS (later): platform-specific.
- **Firebase** provides the cloud: accounts (Firebase Auth), sync (Firestore), analytics and crash reporting
  (Analytics, Crashlytics on Android; on desktop, through the web SDK in the Tauri UI, ADR-0007).
- **Who syncs on desktop:** the UI (Tauri) uses the official Firebase JS SDK and passes
  data to the agent over IPC; the agent stores it in SQLite and does not access the cloud itself.
- PostgreSQL is removed from the project (task T002).

## Alternatives

- **Keep PostgreSQL:** unacceptable for an ordinary user.
- **Supabase instead of Firebase:** PostgreSQL in the cloud and a C# client, but free projects are put to sleep;
  the owner chose Firebase.
- **The agent syncs with Firebase through REST:** additional code in the agent, while the UI is already
  always running in the tray. Reconsider if sync without a running UI is needed.

## Consequences

- The old Windows users' SQLite database (`%LOCALAPPDATA%\FocusManager\focusmanager.db`) becomes relevant
  again: no PostgreSQL data migration is needed (task T010 is cancelled).
- Storage tests no longer need a PostgreSQL server.
- The Firestore document format is shared across all platforms (task T021).
- A Firebase project is needed, to be created by the owner (task T023).
