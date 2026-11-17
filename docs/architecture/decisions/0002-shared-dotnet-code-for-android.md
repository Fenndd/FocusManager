# ADR-0002: Android on .NET with Shared C# Code

- Status: **Superseded by ADR-0005** (2026-11-16)
- Date: 2026-09-30

## Context

An Android version is planned. The requirement is to share code where practical.
The current code uses C#/.NET 8. The platform-independent part consists of models, matching rules (`RuleEvaluator`),
SQLite storage, and (after refactoring) study-mode orchestration and input validation.
Android blocking mechanisms are fundamentally different (Accessibility/VPN instead of WMI/COM/registry),
so they cannot be shared.

## Decision

- The Android application uses **.NET for Android** (`net8.0-android` or newer) and references projects in `src/Shared`.
- Shared code targets `net8.0` (without an OS suffix), allowing these assemblies to be referenced by both Windows and Android projects.
- Platform mechanisms are implemented in `FocusManager.Platform.Android` behind interfaces from `FocusManager.Core`.

## Alternatives

- **Kotlin + Jetpack Compose.** The best native Android tooling and documentation, but no code would be shared
  with Windows: rules would have to be duplicated. Shared test vectors (JSON) partly compensate,
  but every rule change still means doing the work twice.
- **.NET MAUI for both platforms (shared UI).** The UI could also be shared, but Windows would require
  rewriting the already working WinUI 3 UI, and platform specifics (tray and agent on Windows, permissions
  screen and services on Android) still require platform-specific UI.

## Open Questions

- Android UI: native Android Views/`AndroidX` from C#, or .NET MAUI for Android only.
  To be resolved in task `T008` and recorded in a separate ADR.
- .NET version: .NET 8 is supported until November 2026; upgrade to .NET 10 (LTS) in task `T007`,
  preferably before Android development begins.

## Consequences

- Everything that can be shared must live in `src/Shared` and have no OS dependency (see `docs/architecture/README.md`).
- Shared-code tests must pass on every OS (task `T001`); otherwise Android behavior cannot be guaranteed.
- Building Android requires the `android` workload; Windows developers build `FocusManager.Windows.slnf` without it.
