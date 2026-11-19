# T055 evidence index: Windows SQLite startup and restarts

Date: 2026-11-18. Task: [T055](../../../../tasks/T055-verify-windows-sqlite-startup-and-restarts.md). Context:
[independent review](../../2026-11-18-uncommitted.md), T002 AC1.

## Outcome

**Blocked at the execution prerequisite.** No safe isolated Windows environment exists on the machine that holds the
repository, so the real launcher, agent and UI were **not** started and no real-lifecycle observation was made. No
substitute check was run in its place. What exists is the evidence that establishes the blocker, a read-only trace of
what a launch would touch, and an exact runbook for the missing environment.

Smallest owner action: provide a **disposable Windows 10/11 x64 environment** and run (or authorize an agent to run)
[runbook.md](runbook.md) in it. Options, in order of least effort:

1. A spare physical Windows PC / dedicated test machine.
2. A Windows VM. VirtualBox 7.1.10 is already installed on the host but holds only a Linux guest and no Windows image
   was found; a Windows ISO or evaluation image the owner is licensed to use, about 60 GB disk and 4 GB RAM are
   needed (about 3 GB of RAM was free at inspection, so other applications would have to be closed). Whether
   VirtualBox runs a Windows guest acceptably next to the host's active Windows hypervisor is **not verified**.

The host is Windows 11 Home, which has no Hyper-V manager and no Windows Sandbox. A second Windows account on the
host is not isolation (acceptance rule) and would be unsafe here (see below).

## Files

| File | Content |
| --- | --- |
| [environment-inspection.md](environment-inspection.md) | Read-only commands and results: edition, virtualization, VMs, install media, accounts, UAC, owner-resource presence. Sanitized. |
| [code-trace.md](code-trace.md) | Read-only trace of `Start-FocusManager.cmd`, both scripts, `Program.cs`, pipe, registry, process effects, effective `LocalApplicationData`; source hashes. |
| [runbook.md](runbook.md) | Not executed. Go/no-go gate, fixtures, scenarios A (fresh), B (pre-`allow_subfolders`), C (current schema), cleanup, evidence list. |
| [fixture_tools.py](fixture_tools.py) | Standard-library Python helper that creates the synthetic legacy/current fixtures and prints a read-only report. |

## Verified, not verified, failed

| Item | Result | Evidence |
| --- | --- | --- |
| Inspection of isolation options without changing the machine | verified | [environment-inspection.md](environment-inspection.md) |
| `Get-WindowsOptionalFeature -Online` (feature state) | **failed** (needs elevation); feature state not verified, replaced by file/command checks that show the Hyper-V manager and Windows Sandbox binaries are absent | same |
| Read-only code trace of the launch path | verified (static reading only; nothing executed) | [code-trace.md](code-trace.md) |
| Historical schema fixtures derived from source | verified: `LegacySchemas.cs` text matches `git show 31e6e31:` store (`WithAllowSubfolders`) and `5f2e7e7`/`f7abfaf` store sources (`WithoutAllowSubfolders` / `WithAllowSubfolders`) | `git show` of `src/FocusManager.Infrastructure/Persistence/SqliteWhitelistStore.cs` at those commits |
| `fixture_tools.py create` and `report` | verified once, with output in the session scratch directory only (not the repository or the owner's data): created both fixtures, refused to overwrite, reported schema and rows; fixture hashes are not recorded here because the runbook has them recorded at execution time | |
| AC1 (isolated identity, revision, path, hashes) | **not verified** (no environment) | blocker |
| AC2 fresh start and restart through real agent/UI | **not verified** | blocker |
| AC3 pre-`allow_subfolders` database through real agent lifecycle | **not verified** | blocker |
| AC4 `dotnet test FocusManager.Shared.slnf` / `FocusManager.Windows.slnf` | **not verified by T055 author**: not run on instruction (another agent is building); the main agent runs them | |
| New storage test `SqliteWhitelistStoreRestartLifecycleTests` | written, **not run** (see below) | |

## Why launching on this machine is unsafe (summary of [code-trace.md](code-trace.md))

* The only enabled administrator is the owner's account; a UAC elevation of the launcher therefore runs the agent as
  the owner. `Environment.SpecialFolder.LocalApplicationData` is resolved from the known-folder API, so the agent
  opens the owner's live `focusmanager.db` (it exists); the path has no override, and a `LOCALAPPDATA` variable change
  does not redirect it (verified for the PowerShell 5.1 call, same API as .NET 8 per the source).
* Startup may write to an existing database (`ALTER TABLE`, settings row); every UI save deletes and re-inserts all
  rows.
* The launcher stops every process named `FocusManager.Agent` (any session), the pipe name
  `\\.\pipe\FocusManager.Agent.Pipe` is machine-wide, and an agent start with study mode off deletes the
  `URLAllowlist` / `URLBlocklist` subkeys under `HKCU\Software\Policies\Google\Chrome` of the running account.
* The launcher also rebuilds `bin/` and `obj/` in the repository, which can race with another agent's build.

## Storage-level coverage gap and proposed test

`tests/Shared/FocusManager.Persistence.Sqlite.Tests/SqliteWhitelistStoreRestartLifecycleTests.cs` (one `[Fact]`,
added during the interrupted attempt and reviewed by reading) models the agent's sequence at storage level: legacy
file, run 1 (`LoadAsync`, edit, `SaveAsync`), disposal, runs 2 and 3 (`LoadAsync` only), asserting rows and flags,
default-folder position, a byte-identical file across load-only restarts, and migrated schema equal to the current
schema. The existing tests cover these facets separately (`MigratedDatabase_RoundTripsAllowSubfolders`,
`Data_PersistsAcrossStoreInstances`, `OpensPreviousSqliteDatabase_...WithoutModifyingTheFile`); the combination in one
scenario is the only addition. It reuses `TempDatabase` and `LegacySchemas` and does not call the agent. It does
**not** replace the real-agent evidence. It has not been compiled or run (no `dotnet` execution was allowed); the
main agent should run `dotnet test FocusManager.Shared.slnf` and, if it fails to compile or pass, fix or delete it.

## Items for other tasks (not decided here)

* T058 / owner decisions K1 and K3: the runbook uses the current per-user database and per-user `HKCU` policy, which
  is compatible with Q2 and Q15. Q5 (apply rules immediately on service start, no new snapshot) conflicts with the
  current `AgentHostedService.EnableStudyModeAsync` snapshot capture; study mode is deliberately not exercised.
  Q11 says current development databases are test data, but that does not authorize touching the owner's database in
  T055; future real-user data must still migrate, which scenario B supports as evidence.
* Q16(b) crash cleanup: the agent clears browser policy only on start with study mode off and on disabling study
  mode; `StopAsync` does not clear it ([code-trace.md](code-trace.md), section 5).
* IPC hardening for T058/T056: `IpcPipeFactory` does not use `FirstPipeInstance`, so a second agent on one OS
  instance creates another server instance of the same name.
* T054 changed `Program.cs` during this work (path-rules wiring); scenarios that depend on folder normalization
  should be repeated against the final revision.

## Limitations

* All findings are from read-only inspection of one machine on one day; other folders and drives were not searched
  for Windows media.
* Nothing here is owner acceptance of T001 or T002. T002 AC1 remains **partly verified**: store tests exist, the real
  startup/restart/legacy lifecycle does not.
* Linux was not touched (J1). One registered Linux VM was seen in a listing and left alone.
