# ADR-0005: A Stack per Platform, Sharing Code Only Where It Is Truly Shared

- Status: Accepted
- Date: 2026-11-16
- Supersedes: ADR-0002

## Context

ADR-0002 proposed Android on .NET to share C# code with Windows. The owner decided otherwise
(decisions B1, C1, C2, C4 in `docs/spec/decisions.md`): the primary platforms are Android, Windows, and Linux;
iOS comes later; code should be shared only where that is genuinely better than separating it. Blocking relies
on system mechanisms (Accessibility/VPN on Android, WMI/COM/registry on Windows, root services on Linux),
so there is little shared code between mobile and desktop.

## Decision

| Platform | Logic | UI |
|-----------|--------|-----------|
| Android | Kotlin | Jetpack Compose |
| Windows | C#/.NET (agent) | Tauri 2 + TypeScript/React (ADR-0007) |
| Linux | C#/.NET (agent, code shared with Windows) | The same Tauri application |
| iOS (later) | Swift | SwiftUI |

What **all** platforms share is not code but:

- the specification (`docs/spec/`);
- **test vectors** in JSON (`test-vectors/`): inputs and expected rule results
  (site, application, and path matching). Each platform runs them through its own tests (task T022);
- the data format for synchronization through Firebase (task T021).

Shared **code** exists where it is truly shared: the Windows and Linux C# agent (`src/Shared`) and
the desktop UI (one Tauri application for Windows and Linux).

## Alternatives

- **.NET for Android** (ADR-0002): little shared code, while Android system mechanisms are easier in Kotlin.
- **Kotlin Multiplatform for Android + iOS:** reconsider if duplication between them becomes a problem.
- **Compose Multiplatform for desktop:** a UI shared with Android, but it requires a Java VM, lacks a desktop Firebase SDK,
  and contradicts decision C4.

## Consequences

- `src/Shared/*` projects (C#) contain shared **desktop-agent** code (Windows + Linux), not code for all platforms.
- The Android project uses Gradle/Kotlin in `src/Android/`, without .NET.
- Rules present on several platforms must be covered by shared test vectors.
