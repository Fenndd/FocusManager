---
id: T055
title: Verify Windows SQLite startup and restarts
milestone: M1
status: blocked
depends_on: []
area: ["docs/reviews/evidence/T055/**", "tests/Shared/FocusManager.Persistence.Sqlite.Tests/**", "tasks/T055-verify-windows-sqlite-startup-and-restarts.md"]
---

# T055 — Verify Windows SQLite startup and restarts

## Goal

Close the real Windows lifecycle evidence gap in T002 using synthetic current/legacy SQLite data in an isolated Windows environment, with no interaction with the owner's live data or processes.

## Sources

- [Review and startup boundaries](../docs/reviews/2026-11-18-uncommitted.md), [T002](T002-restore-sqlite-storage.md), [ADR-0006](../docs/architecture/decisions/0006-local-storage-and-firebase.md).
- [Windows requirements](../docs/spec/platforms/windows.md), [folder requirements](../docs/spec/folders.md), [architecture](../docs/architecture/README.md).
- Existing `Start-FocusManager.cmd`, `scripts/windows/Start-FocusManager.ps1`, `scripts/windows/Start-FocusManager.AgentElevated.ps1`, `src/Windows/FocusManager.Agent/Program.cs`, SQLite store and migration tests.
- Owner instruction on 2026-11-18: halt all Linux development until explicit owner resumption.

## Scope

In scope: prepare a verified isolated Windows user/session or VM, use the existing real agent/UI/launcher, and record startup, current-schema persistence, restarts and old-schema migration with synthetic fixtures. Add a focused storage regression test only if the lifecycle investigation exposes a concrete coverage gap. This verifies unaccepted T002 work; it consumes no accepted new feature output, so `depends_on: []` is intentional and does not accept T001/T002. Default handoff order is after T054; if started independently, record which source state was tested and repeat affected scenarios after relevant changes.

Out of scope: changing production startup/storage/IPC/service architecture; using the owner's real database or terminating the owner's processes; implementing T007/T012; Linux tooling/provisioning/execution/research. No Linux environment is required for this task. If safe Windows isolation is unavailable, record a concrete blocker and the missing environment rather than fabricating results or modifying production behavior solely to obtain a test.

## Acceptance Criteria

Use a disposable Windows VM or dedicated test machine where possible. A second Windows account/session on
the owner's current machine alone is not sufficient isolation: named pipes and elevated process-by-name
cleanup can cross session boundaries. Any alternative must demonstrate separation of every affected resource
before the real launcher runs; otherwise record the missing VM/machine as the execution blocker.

1. Evidence identifies the isolated Windows identity/session or VM, source revision/diff, actual database path and fixture hashes, and explains why startup cannot affect the owner's database, processes, IPC or registry state. Paths/account labels are sanitized where appropriate.
2. With no database configuration, a fresh real agent/UI launch initializes SQLite at the expected per-user location; synthetic app/folder/site values saved through the application remain after a complete agent/UI stop and restart. Record actual processes/exit status or other observed startup evidence; a build alone is insufficient.
3. A synthetic pre-`allow_subfolders` schema, based on the existing historical schema/tests, opens through the real agent lifecycle. Existing rows remain readable, migration/default semantics match T002, and subsequent save plus restart preserves the data. Keep original and resulting fixture hashes/schema evidence.
4. Applicable automated Windows/Shared checks pass or failures are diagnosed and recorded independently of the manual observations. Restore/close only the synthetic environment's processes and fixtures; record cleanup without touching owner resources.
5. Evidence and limitations are stored under `docs/reviews/evidence/T055/`, T055 enters `review`, and T002's AC1 coverage is accurately reported. T002 acceptance still belongs to the owner; Linux verification is deferred under owner hold.

## Checks

- Before launching, inspect `Program.cs` and both launcher scripts. Confirm the actual Windows identity used after elevation, effective `Environment.SpecialFolder.LocalApplicationData`, named-pipe scope, process-name stop behavior and registry effects in the isolated environment. A copied repository, changed working directory or `LOCALAPPDATA` environment override alone is insufficient isolation.
- Use `Start-FocusManager.cmd` **only inside the verified isolated Windows environment**. Observe fresh startup, save identifiable synthetic data through UI, stop only the isolated environment's app/agent, relaunch, and compare persisted values. Record exact commands/manual steps actually used; no isolated account/VM provisioning command is currently prescribed by the project.
- In the same isolated environment, install the synthetic historical database fixture at the effective agent path while its app/agent are stopped. Start, inspect migrated values/schema, save, restart and compare. Derive fixture setup from the actual storage schema/tests, record SQL/commands in evidence, and do not substitute an arbitrary database for legacy-format coverage.
- Run `dotnet test FocusManager.Shared.slnf` and `dotnet test FocusManager.Windows.slnf` on Windows. Use Visual Studio MSBuild as supported by the existing launcher for a full UI build; record the actual resolved executable/command and outcome. Distinguish a CLI MSB4062 failure from the baseline/tooling evidence in the review.
- Inspect evidence for confidential data, validate links and run `git diff --check` for assigned files. No Linux checks are permitted pending owner resumption.

## Open Questions

None for the verification scope. Availability of a safe isolated Windows environment is an execution prerequisite to establish before launch; if unavailable, report it as a blocker rather than changing the task's acceptance criteria.

## Result

2026-11-18, executor tier 2. **Blocked at the execution prerequisite: no safe isolated Windows environment exists on the machine that holds the repository.** The real launcher, agent and UI were not started; no application database, process, registry key, named pipe or service was touched; no substitute check was run. Evidence index: [docs/reviews/evidence/T055/README.md](../docs/reviews/evidence/T055/README.md).

**Missing environment and smallest owner action:** a disposable Windows 10/11 x64 VM or dedicated test machine, then execution of [runbook.md](../docs/reviews/evidence/T055/runbook.md) in it (prepared, not executed). Facts: the host is Windows 11 Home with no Hyper-V manager and no Windows Sandbox binaries; VirtualBox 7.1.10 is installed but registers only a Linux guest and no Windows image was found in the searched user folders; a second account on the host is not isolation and is unsafe because the only enabled administrator is the owner account, so the elevated agent would use the owner's `LocalApplicationData` (database exists) and `HKCU`, the pipe name is machine-wide and the launcher stops every `FocusManager.Agent` by name. Details: [environment-inspection.md](../docs/reviews/evidence/T055/environment-inspection.md), [code-trace.md](../docs/reviews/evidence/T055/code-trace.md).

Verified: read-only environment inspection; read-only code trace (static reading, nothing executed); fixture schemas match `git show` of the store at commits `5f2e7e7`, `f7abfaf` and `31e6e31`; `fixture_tools.py` create/report run once in the session scratch directory only. Not verified: AC1 (isolated identity, database path and hashes), AC2 (fresh start and restart), AC3 (pre-`allow_subfolders` database through the real agent), and AC4 (this task's author did not run `dotnet test`; the main agent runs both slnf suites). Failed: `Get-WindowsOptionalFeature -Online` needs elevation, so feature state is unverified (file and command checks used instead). Added, written but **not run or compiled**: `SqliteWhitelistStoreRestartLifecycleTests` in `tests/Shared/FocusManager.Persistence.Sqlite.Tests/`, a storage-level combination of legacy open, save and repeated restarts that does not replace real-agent evidence.

T002 AC1 remains **partly verified**: store tests cover persistence across store instances and legacy migration; the real startup, restart and legacy-database lifecycle has no evidence. Nothing here accepts T001 or T002; Linux was not touched (hold J1). Status stays for the main agent to set (blocked until the environment exists). Completion enters `review`; owner acceptance is required for `done` and is not acceptance of T002 by itself.
