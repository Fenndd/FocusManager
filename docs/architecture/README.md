# FocusManager Architecture

Decisions: [ADR-0001](decisions/0001-docs-spec-plans-layout.md) (original documentation layout),
[ADR-0003](decisions/0003-source-tree-by-platform.md) (source layout),
[ADR-0005](decisions/0005-native-stack-per-platform.md) (platform stacks),
[ADR-0006](decisions/0006-local-storage-and-firebase.md) (storage and cloud),
[ADR-0007](decisions/0007-desktop-agent-and-tauri-ui.md) (desktop architecture),
[ADR-0008](decisions/0008-adopt-ai-development-infrastructure.md) (current development infrastructure and task layout).

## Platforms and Stacks

| Platform | Logic | UI | Local storage | Cloud |
|-----------|--------|-----------|---------------------|--------|
| Windows | C#/.NET agent (service) | Tauri 2 + TypeScript + React | SQLite | Firebase (through the UI) |
| Linux | C#/.NET agent (systemd), code shared with Windows | The same Tauri application | SQLite | Firebase (through the UI) |
| Android | Kotlin | Jetpack Compose | Room (SQLite) | Firebase Android SDK |
| iOS (later) | Swift | SwiftUI | — | — |

What **all** platforms share is the **specification**, **test vectors** (`test-vectors/`), and **data formats**
(`contracts/`), rather than code (ADR-0005).

## Repository Layout

Current state and target (ADR-0007). ⏳ means not yet created.

```
FocusManager.sln                 ← C# projects (solution folders: src/Shared, src/Windows, src/Linux, tests/...)
FocusManager.Shared.slnf         ← shared C# code + tests (builds on any OS)
FocusManager.Windows.slnf        ← Shared + Windows (builds on Windows)
Directory.Build.props           ← shared C# project settings
contracts/                   ⏳ JSON Schema: UI ↔ agent messages, sync documents
test-vectors/                ⏳ shared rule test vectors (JSON) for all platforms
src/
├── Shared/                     ← C#, shared desktop-agent code (Windows + Linux), net8.0, no OS APIs
│   ├── FocusManager.Core/                  models, rules, abstractions (ports)
│   └── FocusManager.Persistence.Sqlite/    local SQLite storage; path and default folders come from the platform host (ADR-0006)
├── Windows/                    ← C#, Windows only
│   ├── FocusManager.Platform.Windows/      WMI, File Explorer COM, browser-policy registry
│   ├── FocusManager.Contracts/             IPC messages (to be generated from contracts/, task T005)
│   ├── FocusManager.Agent/                 agent (to become a service without UI, task T016)
│   └── FocusManager.App/                   WinUI 3 — removed after the Tauri migration (task T018)
├── Linux/                   ⏳ C#: Platform.Linux, agent host (task T020)
├── Desktop/                 ⏳ Tauri 2 + TypeScript + React — desktop UI (task T017)
├── Android/                 ⏳ Kotlin + Compose, Gradle (task T009)
└── iOS/                        later
tests/
├── Directory.Build.props       ← shared C# test packages (xUnit)
├── Shared/                     ← shared C# code tests, on any OS
└── Windows/                    ← Windows project tests, on Windows
scripts/
└── windows/                    ← developer scripts for Windows
```

Each non-.NET project (Tauri, Android) keeps its tests inside its own directory according to its
tooling conventions (`src/Desktop/...`, `src/Android/...`).

Work is tracked in the root [ROADMAP.md](../../ROADMAP.md), [STATUS.md](../../STATUS.md), and
`tasks/T<NNN>-<slug>.md`; [TASK_FORMAT.md](../engineering/TASK_FORMAT.md) defines the task format and lifecycle.

## C# Project Dependencies

```
Shared:   Persistence.*         ──► Core
Windows:  Platform.Windows      ──► Core
          Contracts             (no dependencies; later generated from contracts/)
          Agent                 ──► Core, Persistence.*, Platform.Windows, Contracts
          App (temporary WinUI) ──► Core, Contracts
Linux:    Platform.Linux        ──► Core                                      (planned)
          Agent (Linux host)    ──► Core, Persistence.Sqlite, Platform.Linux  (planned)
```

Rules (checked in review):

1. `src/Shared/*` references only `src/Shared/*` and targets `net8.0` (without `-windows`). No OS APIs
   (registry, WMI, COM, P/Invoke, `System.Windows.*`, `/proc`, D-Bus). When an OS capability is needed,
   define an interface in `Core.Abstractions` and an implementation in `Platform.<OS>`.
2. `FocusManager.Core` depends only on the BCL.
3. Projects for platform X do not reference projects for platform Y.
4. The desktop UI communicates with the agent only through contracts (`contracts/`); the agent does not depend on the UI.
5. Blocking does not depend on the network or a running UI: the agent applies the last saved rules.
6. C# tests mirror `src/`: shared code in `tests/Shared`, Windows code in `tests/Windows`, Linux code in `tests/Linux`.

## Where to Put New Code

| Code | Location |
|-----|------|
| Desktop domain model or allowed/blocked rule | `src/Shared/FocusManager.Core` |
| Desktop-agent session, profile, and trigger logic (the same on Windows and Linux) | `src/Shared` (task T003) |
| Desktop local storage | `src/Shared/FocusManager.Persistence.Sqlite` |
| Code calling Windows / Linux APIs | `src/Windows/FocusManager.Platform.Windows` / `src/Linux/...` |
| Desktop screens, tray, sign-in, Firebase sync | `src/Desktop` (Tauri) |
| UI ↔ agent messages, sync documents | `contracts/` (JSON Schema) |
| A rule that must behave identically on several platforms | Implementation on each platform + vectors in `test-vectors/` |
| All Android code | `src/Android` |

## Conventions

- Code, identifiers, comments, and documentation are written in English.
- C#: file-scoped namespaces, `sealed` by default, `record` for models, async methods with `CancellationToken`.
- C# projects: `FocusManager.<Layer>` for shared code, `FocusManager.Platform.<OS>` for OS integrations.
- In the `FocusManager.Platform.Windows` / `...Linux` namespace, fully qualified OS types use
  `global::` (otherwise the name resolves within our namespace); a `using` at the top of the file works without it.
