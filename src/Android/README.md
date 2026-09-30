# Android (planned)

Android projects will live here. Nothing is implemented yet.

Planned layout (see `docs/plans/tasks/0009-bootstrap-android-projects.md`):

| Project | Purpose |
|---------|---------|
| `FocusManager.Platform.Android` | OS integrations (AccessibilityService, VpnService, foreground service) implementing ports from `FocusManager.Core` |
| `FocusManager.Android` | The Android app (UI + background service), references `src/Shared/*` |

Rules: `docs/architecture/README.md`. Decisions: `docs/architecture/decisions/0002-shared-dotnet-code-for-android.md`.
