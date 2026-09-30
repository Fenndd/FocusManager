# FocusManager

Study mode for your devices: while it is on, only whitelisted applications, folders and websites can be opened.

- Windows version: `src/Windows` (WinUI 3 UI + elevated background agent).
- Android version: planned, see `docs/plans/roadmap.md`.

## Repository map

| Path | Purpose |
|------|---------|
| `docs/spec/` | Product specification — source of truth for behavior |
| `docs/architecture/` | Architecture, project layout rules, decision records (ADR) |
| `docs/plans/` | Roadmap and task files for AI agents / developers |
| `src/Shared/` | Cross-platform code (domain rules, SQLite storage) |
| `src/Windows/` | Windows-only projects (App, Agent, IPC contracts, OS integrations) |
| `src/Android/` | Android projects (planned) |
| `tests/` | Tests, mirrored by platform |
| `scripts/` | Developer scripts, per platform |
| `AGENTS.md` | Working rules for AI agents |

## Build and test

```bash
dotnet test FocusManager.Shared.slnf    # shared code, any OS
dotnet test FocusManager.Windows.slnf   # full Windows build, Windows only
```

## Dev launch (Windows)

Use `Start-FocusManager.cmd` from the repository root to run FocusManager during development.

The launcher builds `FocusManager.Windows.slnf`, starts `FocusManager.Agent` as administrator with its console window visible, waits briefly for the agent to initialize, and then starts the regular UI app. Confirm the UAC prompt for the agent when Windows asks; the UI should open shortly after the prompt is accepted.

Do not start the agent with plain `dotnet run` when application blocking is needed. The process-start monitor uses Windows WMI and may require elevated permissions.

If the UI shows `Access to the path is denied` for the agent status or whitelist pages, restart FocusManager with `Start-FocusManager.cmd`. A previously running elevated agent may still be using an old named pipe security descriptor.
