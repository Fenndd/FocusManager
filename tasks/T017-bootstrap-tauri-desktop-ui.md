---
id: T017
title: Bootstrap the Tauri 2 and React desktop UI
milestone: M2
status: planned
depends_on: ["T005"]
area: ["src/Desktop/**", ".github/workflows/ci.yml", "AGENTS.md"]
---

# T017 — Bootstrap the Tauri 2 and React desktop UI

## Goal

Create the shared Windows/Linux desktop UI with tray control, localized session status, and typed agent communication.

## Sources

Legacy classification: type `feature`; platform `all`; priority `high`.

Requirement identifiers: `GEN-001`, `GEN-002`, `SM-001`, `SM-007`, `WIN-007`.

Legacy ADR associations: ADR-0007.

[Product requirements](../docs/spec/product.md), [study mode](../docs/spec/study-mode.md), [Windows requirements](../docs/spec/platforms/windows.md), and [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md).

ADR-0007 / decision C3 selects Tauri 2 + TypeScript + React for one desktop UI. It replaces `src/Windows/FocusManager.App` (WinUI 3) and the agent tray.

## Scope

In scope:
1. Create `src/Desktop/` with Tauri 2, Vite, React, TypeScript, and README instructions for dependencies, development, build, and tests.
2. Add a tray menu: open window, start/end session, exit.
3. Connect to the agent through a Windows named pipe; establish a transport interface for Linux Unix sockets. Use message types generated from `contracts/` by T005.
4. Main screen: agent availability (SM-007), session state, and start/end control (SM-001).
5. Localize from the start (GEN-002): all UI strings in English/Russian translation files.
6. Build on Windows and Linux; verify WebKitGTK rendering on Linux, the main ADR-0007 risk, and record the result.
7. Add UI CI build if T006 is complete; update `AGENTS.md`.

Out of scope: list screens (T018), account sign-in, and Firebase (M10).

## Acceptance Criteria

1. The application builds and starts on Windows and Linux; on Windows it controls sessions through the agent.
2. UI strings exist only in translation files (`en`, `ru`).
3. Result records WebKitGTK verification; serious issues are escalated to the owner as a question about Electron.

## Checks

- Run the install/build/test commands established in `src/Desktop/README.md`; no desktop commands exist yet.
- On Windows, check tray/menu, agent availability, and session start/end through IPC.
- On Linux, build/start and inspect WebKitGTK rendering; record distribution/runtime versions and limitations.
- Inspect translation files and the conditional UI CI job.

## Open Questions

None

## Result

Not started. No implementation or checks have been completed for this task.
