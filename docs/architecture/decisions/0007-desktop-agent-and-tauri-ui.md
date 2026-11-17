# ADR-0007: Desktop Architecture — Background C# Agent + Tauri UI

- Status: Accepted
- Date: 2026-11-16
- Extends: ADR-0003 (source layout)

## Context

The desktop version must work the same way on Windows and Linux (decisions B1, B5). Windows currently has
an agent (C#, with a WinForms tray icon) and a WinUI 3 UI (Windows only). The owner chose
Tauri 2 + web technologies for the UI (decision C3); the logic stays in C#.

## Decision

### Components

| Component | Technology | Runs as | Responsible for |
|-----------|-----------|--------------|-------------|
| **Agent** | C#/.NET, no UI | Windows service / systemd service (administrator/root privileges) | Monitoring, blocking, local storage (SQLite), applying rules |
| **UI** | Tauri 2 (Rust shell) + TypeScript + React | User process, tray icon | Screens, tray, notifications, account sign-in and sync through Firebase JS SDK, payments |
| **Contracts** | JSON Schema → generated C# and TypeScript types | — | UI ↔ agent message format |

### UI–Agent Communication

- Windows: named pipe (as now); Linux: Unix domain socket; access limited to the current user.
- Messages use JSON conforming to the schema in `contracts/`; C# and TypeScript types are generated from the schema (task T005).
- When the UI is not running, the agent continues blocking according to the last saved rules.

### Source Layout (Target)

```
src/
├── Shared/            C# — code shared by Windows and Linux agents (Core, Persistence.Sqlite, session logic)
├── Windows/           C# — Platform.Windows, Windows agent host
├── Linux/             C# — Platform.Linux, Linux agent host (task T020)
├── Desktop/           Tauri 2 + TypeScript + React — desktop UI (task T017)
├── Android/           Kotlin + Compose, Gradle (task T009)
└── iOS/               later
contracts/             JSON Schema for UI ↔ agent messages and sync documents
test-vectors/          shared rule test vectors (JSON) for all platforms
```

`FocusManager.App` (WinUI 3) and the WinForms tray icon are removed after the screens are migrated to Tauri
(tasks T016, T018).

### UI Framework

React + TypeScript: the largest ecosystem (charts, localization) and the most familiar to AI agents.
This may be revisited during bootstrap (task T017) if compelling reasons emerge.

## Alternatives

Discussed in the chat with the owner on 2026-11-16 and in `docs/spec/decisions.md` (C3): Electron (fallback,
using the same UI code), Compose Multiplatform, Flutter, Avalonia, WinUI 3 + a separate Linux UI.

## Risks

- **WebKitGTK on Linux** may render and behave differently from WebView2 on Windows: check during bootstrap;
  if problems are severe, switch to Electron using the same UI code.
- **Google sign-in** is prohibited by Google in an embedded window: use the system browser and return
  to the application (deep link or loopback).
- **Microsoft Store** and an administrator-privileged service: check store requirements (GEN-006).

## Consequences

- The desktop stack gains Rust (a thin Tauri shell), TypeScript, and Node.js build tooling.
- CI builds the UI and agent for Windows and Linux (task T006).
