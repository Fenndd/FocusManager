# FocusManager

Focus blocker for study and work: profiles with allow-lists or block-lists of apps, websites and folders,
schedules and limits, and protection levels against bypassing.

- Main platforms: Android, Windows, Linux; iOS later (`docs/spec/product.md`).
- Today only the Windows version exists: `src/Windows` (background agent + WinUI 3 UI, to be replaced by a Tauri UI).
- Plan: [ROADMAP.md](ROADMAP.md). Current work: [STATUS.md](STATUS.md).
- Architecture: [docs/architecture/README.md](docs/architecture/README.md).

## Repository map

| Path | Purpose |
|------|---------|
| `docs/spec/` | Product specification — source of truth for behavior |
| `docs/architecture/` | Architecture, project layout rules, decision records (ADR) |
| `ROADMAP.md`, `STATUS.md`, `tasks/` | Milestones, current state, and individual development tasks |
| `docs/engineering/` | Shared task format, development workflow, and AI orchestration policy |
| `.agents/skills/` | Canonical project skills, references, metadata, and licenses |
| `.claude/skills/`, `.claude/agents/` | Thin skill adapters and model/effort presets for Claude Code |
| `src/Shared/` | C# code shared by the Windows and Linux agents (rules, storage) |
| `src/Windows/` | Windows-only projects (App, Agent, IPC contracts, OS integrations) |
| `src/Linux/`, `src/Desktop/`, `src/Android/` | Linux agent, Tauri desktop UI, Android app (planned) |
| `tests/` | Tests, mirrored by platform |
| `scripts/` | Developer scripts, per platform |
| `AGENTS.md` | Working rules for AI agents |

## Build and test

```bash
dotnet test FocusManager.Shared.slnf    # shared code, any OS
dotnet test FocusManager.Windows.slnf   # full Windows build, Windows only
```

## Database (PostgreSQL — temporary)

The agent will return to a local SQLite file (task T002, ADR-0006). Until then it stores settings and whitelists in PostgreSQL. Tables are created automatically on first start;
the database and role must exist:

```sql
CREATE ROLE focusmanager LOGIN PASSWORD '<password>';
CREATE DATABASE focusmanager OWNER focusmanager;
```

The connection string is `ConnectionStrings:FocusManager` in `src/Windows/FocusManager.Agent/appsettings.json`
(no password there). Provide the password without committing it, e.g. via the
`ConnectionStrings__FocusManager` environment variable (full connection string) or PostgreSQL's
`%APPDATA%\postgresql\pgpass.conf`.

Storage tests need a database too: set `FOCUSMANAGER_TEST_POSTGRES` to a connection string
(tests are skipped otherwise). See `AGENTS.md`.

## Dev launch (Windows)

Use `Start-FocusManager.cmd` from the repository root to run FocusManager during development.

The launcher builds `FocusManager.Windows.slnf`, starts `FocusManager.Agent` as administrator with its console window visible, waits briefly for the agent to initialize, and then starts the regular UI app. Confirm the UAC prompt for the agent when Windows asks; the UI should open shortly after the prompt is accepted.

Do not start the agent with plain `dotnet run` when application blocking is needed. The process-start monitor uses Windows WMI and may require elevated permissions.

If the UI shows `Access to the path is denied` for the agent status or whitelist pages, restart FocusManager with `Start-FocusManager.cmd`. A previously running elevated agent may still be using an old named pipe security descriptor.

## AI-assisted development

Read [AGENTS.md](AGENTS.md) before working in this repository. Claude Code and Gemini load the
same rules through `CLAUDE.md` and `GEMINI.md`. All project documentation is in English.

Use the shared [development workflow](docs/engineering/DEVELOPMENT_WORKFLOW.md),
[task format](docs/engineering/TASK_FORMAT.md), and
[orchestration policy](docs/engineering/AI_ORCHESTRATION.md). The project adopts these processes;
[ADR-0008](docs/architecture/decisions/0008-adopt-ai-development-infrastructure.md) records the
one-time migration from the previous planning layout. Legacy task `0001` maps to `T001`, and so on;
requirement IDs and ADR numbers remain unchanged.

The installed skills are `model-routing`, `session-teacher`, `security-best-practices`, and
`security-threat-model`. Their own triggers define when to use them. The Claude presets are
`sonnet-high`, `opus-medium`, `opus-high`, and `opus-max`; the orchestration policy owns model routing.
These repository files do not install clients, enable permissions, or configure accounts.
