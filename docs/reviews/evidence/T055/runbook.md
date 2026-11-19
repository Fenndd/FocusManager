# T055 runbook: real-agent SQLite lifecycle in a disposable Windows environment

Status: **prepared, not executed.** No step below was run by the author. It is for the owner or another agent,
and only **inside a disposable Windows VM or a dedicated test machine** (see
[environment-inspection.md](environment-inspection.md) for why the owner's machine does not qualify, and
[code-trace.md](code-trace.md) for what a launch touches). Do not follow it on the machine that holds the owner's
data. Everything is synthetic; every value carries the marker `FMT055`.

Covers T055 acceptance criteria 1-3 (AC4 automated suites are run on the host by the main agent, independently;
AC5 is bookkeeping). Linux is out of scope (owner hold J1). Study mode is never switched on in any scenario: it would
terminate processes and write browser policy, which this verification does not need.

## 0. Go / no-go gate (stop if any item is false)

| # | Requirement | How to show it (save output as evidence) |
| --- | --- | --- |
| G1 | A separate Windows 10/11 x64 OS instance: a VM guest with its own disk image, or a physical test machine. Not a second account, not Windows Sandbox on the owner's machine. | Guest `Get-ComputerInfo -Property CsName,OsName,OsBuildNumber,WindowsEditionId`; label the machine `T055-VM` in saved text. |
| G2 | No channel into the host's profile, processes, pipes or registry: VM has no shared folders, no clipboard / drag-drop sharing, no serial-port-to-host-pipe mapping, no host-only mounts. NAT or no network only. | Screenshot or exported VM settings page of those items. |
| G3 | A "clean" snapshot (or an image that can be deleted) exists before the first launch. | Snapshot name and time. |
| G4 | Guest starts clean: no `FocusManager` processes, no pipe, no `%LOCALAPPDATA%\FocusManager`, no `HKCU\Software\Policies\Google\Chrome\URLAllowlist`/`URLBlocklist`. | See block G4 below. |
| G5 | The guest account that runs the launcher is a split-token administrator (default for the first account of a Windows install), so the UAC prompt elevates the **same** account and the agent's `LocalApplicationData` is that account's. | `whoami /groups` shows `BUILTIN\Administrators` as "deny only"; later the process owner check in scenario A. |
| G6 | Source revision recorded (section 1). | Output of section 1. |

Block G4 (guest, Windows PowerShell, read-only):

```powershell
Get-Process -Name 'FocusManager*' -ErrorAction SilentlyContinue
[IO.Directory]::GetFiles('\\.\pipe\') | Where-Object { $_ -like '*FocusManager*' }
[Environment]::GetFolderPath('LocalApplicationData')
Test-Path (Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'FocusManager')
foreach ($n in 'URLAllowlist','URLBlocklist') { Test-Path "HKCU:\Software\Policies\Google\Chrome\$n" }
whoami /groups | Select-String 'Administrators|Mandatory'
```

Expected: no output for the first two commands; `LocalApplicationData` is the guest account's profile; `Test-Path` of the data directory `False`; both registry checks `False`.

Optional host-side control (metadata only, never read content): before and after the whole run, on the host record
`Length` and `LastWriteTimeUtc` of the owner's database file and `Get-Process FocusManager*` count. Unchanged values
support the "did not affect the owner's resources" statement of AC1; they do not replace G1-G2.

## 1. Source revision and build inputs

On the machine that holds the source (read-only commands):

```powershell
git rev-parse HEAD
git status --short | Measure-Object | Select-Object -ExpandProperty Count
git diff HEAD | Out-File -Encoding utf8 $env:TEMP\t055-source.diff      # outside the repository
(Get-FileHash $env:TEMP\t055-source.diff -Algorithm SHA256).Hash
Get-FileHash Start-FocusManager.cmd,scripts\windows\Start-FocusManager.ps1,scripts\windows\Start-FocusManager.AgentElevated.ps1,src\Windows\FocusManager.Agent\Program.cs,src\Shared\FocusManager.Persistence.Sqlite\SqliteWhitelistStore.cs -Algorithm SHA256
```

The working tree is dirty with other tasks' work (T001, T002, T054 and others). State which tree was tested; if
T054 or the SQLite store changes afterwards, repeat scenarios B and C.

Getting the code into the guest: copy the working tree (excluding `bin`/`obj`) into the guest by a one-way, one-time
transfer (e.g. a zip written to a removable image attached read-only). The launcher **builds**
`FocusManager.Windows.slnf` itself, so the guest needs .NET SDK 8.x and Visual Studio MSBuild with the WinUI /
Windows App SDK workload (the review recorded SDK 8.0.419 and VS MSBuild 18.3.0; a CLI `dotnet build` of the WinUI
app fails with MSB4062 `ExpandPriContent` on HEAD too, so the launcher needs the VS MSBuild). If installing those in
the guest is not acceptable, building on the host and copying only `bin` outputs is a **deviation**: then run
`Start-FocusManager.AgentElevated.ps1 -AgentExe <path>` and `FocusManager.App.exe` by hand and record that the
launcher itself was not exercised.

## 2. Fixtures

Create them with [fixture_tools.py](fixture_tools.py) (Python 3.9+, standard library only; run it with `-I`). Python
can live on the host (create files, then transfer them into the guest one-way) or in the guest. Do **not** use the
agent's SQLite assemblies from PowerShell 5.1 (they target .NET 8 and fail to load).

```powershell
python -I fixture_tools.py create --kind legacy  --out D:\t055\fx\legacy.db     # pre-allow_subfolders
python -I fixture_tools.py create --kind current --out D:\t055\fx\current.db    # current schema
Get-FileHash D:\t055\fx\legacy.db,D:\t055\fx\current.db -Algorithm SHA256
```

Schema source (verified by the author against git on 2026-11-18):

* `legacy`: `LegacySchemas.WithoutAllowSubfolders` = `allowed_folders (id, display_name, folder_path UNIQUE NOCASE)`
  with no `allow_subfolders`; the schema of `SqliteWhitelistStore` at commit `5f2e7e7`
  (`src/FocusManager.Infrastructure/Persistence/SqliteWhitelistStore.cs`, 2026-04-09 to 2026-04-20).
* `current`: `LegacySchemas.WithAllowSubfolders` = same plus `allow_subfolders INTEGER NOT NULL DEFAULT 0`; identical
  to `InitializeSchema` in `git show 31e6e31:src/Shared/FocusManager.Persistence.Sqlite/SqliteWhitelistStore.cs`
  (2026-09-30), which added the column via `EnsureColumnExists(..., "INTEGER NOT NULL DEFAULT 0")`, the same DDL the new
  store runs as `ALTER TABLE`.
* Both set `study_mode_enabled = '0'` and use the default rollback journal (no WAL). The fixture files are created
  by Python's bundled SQLite, so their file hashes are not comparable with a database written by the agent's
  Microsoft.Data.Sqlite; compare values and schema, and compare hashes only before/after the same operation.

Fixture rows (ids are in insertion order):

| Table | legacy.db | current.db |
| --- | --- | --- |
| allowed_apps | `FMT055 Notepad` `C:\Windows\System32\notepad.exe`; `FMT055 Legacy Tool` `C:\FMT055\Apps\LegacyTool.exe` | `FMT055 Notepad` (same); `FMT055 Current Tool` `C:\FMT055\Apps\CurrentTool.exe` |
| allowed_folders | `System Root (C:)` `C:\`; `FMT055 Legacy Study` `C:\FMT055\Study`; `FMT055 Legacy Notes` `C:\FMT055\Notes` (no flag column) | same names with Current; flags `0`, `1`, `0` |
| allowed_sites | `FMT055 Wiki` `fmt055-wiki.example.org`; `FMT055 Docs` `*.fmt055-docs.example.org` | same |
| settings | `study_mode_enabled = 0` | same |

Guest prerequisites for UI operations (the UI refuses non-existing paths): create
`C:\FMT055\Study`, `C:\FMT055\Notes`, `C:\FMT055\Apps`, and copy `C:\Windows\System32\notepad.exe` to
`C:\FMT055\Apps\FMT055Tool.exe`, `LegacyTool.exe`, `CurrentTool.exe`.

## 3. Report command (used after every stop)

Stop the agent **and** the UI first (section 4), then, in the guest:

```powershell
$db = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'FocusManager\focusmanager.db'
Get-FileHash $db -Algorithm SHA256
Get-ChildItem "$db*" | Select-Object Name,Length            # -journal / -wal / -shm must be absent when stopped
python -I fixture_tools.py report $db > report-<step>.txt   # read-only (mode=ro)
```

If Python is not in the guest, transfer the stopped database file out one-way and run the same report on the host.
Never transfer a database **into** the host profile path.

## 4. Start, stop, relaunch (the same in every scenario)

Start (guest, repository root): `.\Start-FocusManager.cmd`. Record: console output, launcher exit code
(`$LASTEXITCODE` / the "launcher failed" pause), that a UAC consent prompt appeared and was accepted by a person in the
guest, and the elapsed time.

Observe (guest, read-only):

```powershell
Get-Process FocusManager.Agent,FocusManager.App | Select-Object Name,Id,SessionId,StartTime
Get-CimInstance Win32_Process -Filter "Name='FocusManager.Agent.exe'" | ForEach-Object { Invoke-CimMethod -InputObject $_ -MethodName GetOwner | Select-Object Domain,User }
[IO.Directory]::GetFiles('\\.\pipe\') | Where-Object { $_ -like '*FocusManager*' }
```

Expected: one agent process owned by the interactive guest account (not SYSTEM, not another administrator), one UI
process, and the pipe `\\.\pipe\FocusManager.Agent.Pipe`. Screenshot the UI Dashboard (agent reachable) and the
tray icon.

Graceful stop: close the UI window, then tray icon `Exit` (calls `StopApplication`). Record that
`FocusManager.Agent` and `FocusManager.App` are gone from `Get-Process` and that the pipe is gone. Forced stop is
exercised only through the launcher itself (relaunching while an agent runs makes
`Start-FocusManager.AgentElevated.ps1` `Stop-Process -Force` every `FocusManager.Agent`): scenario A step A6.

Do not kill anything by name pattern broader than `FocusManager.*` and only inside the guest.

## 5. Scenarios

Take a snapshot between scenarios, or revert to the clean snapshot (G3), so each starts from a known state. Record
the actual commands and manual steps, including any deviation.

### A. Fresh start, no database (AC2)

| Step | Action | Expected / compare |
| --- | --- | --- |
| A1 | Confirm block G4 (no database directory). | `False` |
| A2 | Start (section 4). | agent + UI + pipe present; process owner = guest account; Dashboard reachable |
| A3 | Stop gracefully, run section 3 report `report-A3`. | Database exists under the guest account's `%LOCALAPPDATA%\FocusManager\focusmanager.db`; current schema (`allow_subfolders INTEGER NOT NULL DEFAULT 0`); `study_mode_enabled=0`; expected: `settings` 1 row, apps/folders/sites 0 rows each (derived from reading `LoadAsync`/`SaveAsync`: only a save writes the default folder). A deviation is an observation, not an automatic failure. |
| A4 | Start. In the UI add (each Add triggers a full `SaveWhitelist`): Apps: `FMT055 Tool` = `C:\FMT055\Apps\FMT055Tool.exe`; Folders: `FMT055 Study` = `C:\FMT055\Study` with *Allow subfolders* checked, then `FMT055 Notes` = `C:\FMT055\Notes` unchecked; Sites: `FMT055 Wiki` = `fmt055-wiki.example.org`, `FMT055 Docs` = `*.fmt055-docs.example.org`. Screenshot each page. | UI shows the entries and "Status: saved" |
| A5 | Stop gracefully, report `report-A5`, save the hash. | apps: 1 row; folders in id order: `System Root (C:)`/`C:\`/0, `FMT055 Study`/`C:\FMT055\Study`/1, `FMT055 Notes`/`C:\FMT055\Notes`/0; sites: 2 rows; no journal files |
| A6 | Start via the launcher again (graceful stop already done, so no kill); read the three UI pages. Then, with the agent still running, run the launcher a **second** time to exercise the forced replacement; observe the old agent is replaced by one new process. Stop gracefully; report `report-A6`. | UI values identical to A5; `report-A6` values identical to `report-A5` (ids unchanged because no save happened); file hash unchanged between A5 and A6 unless a save occurred |
| A7 | Optional: stop with `Stop-Process -Force` on the agent only (simulated crash), start, read values. | Values identical (rollback-journal recovery is not expected to be needed because no write was in progress); record whether a `-journal` file remained |

### B. Pre-`allow_subfolders` database (AC3)

| Step | Action | Expected / compare |
| --- | --- | --- |
| B1 | Revert to the clean snapshot (or stop everything and delete the guest's `FocusManager` data directory **in the guest only**). Create the directory, copy `legacy.db` to `%LOCALAPPDATA%\FocusManager\focusmanager.db` while agent and UI are stopped. Record `Get-FileHash` and `report-B1` (must show no `allow_subfolders` column). | hash equals the fixture hash |
| B2 | Start. In the UI open Folders without clicking anything else. Screenshot. | Three folders listed, all *Allow subfolders* unchecked (default `0`); two apps, two sites listed |
| B3 | Stop gracefully, report `report-B3`. | Column `allow_subfolders INTEGER NOT NULL DEFAULT 0` now exists, equals the current schema; row counts 2/3/2; all flags `0`; names and paths identical to the fixture; `study_mode_enabled=0`; file hash differs from B1 (the migration wrote the column) |
| B4 | Start, tick *Allow subfolders* on `FMT055 Legacy Study`, add site `FMT055 Added` = `fmt055-added.example.org`. Stop, report `report-B4`. | folders: `System Root (C:)` 0, `FMT055 Legacy Study` 1, `FMT055 Legacy Notes` 0; sites: previous two plus the new one; apps unchanged; ids renumbered after the save (compare values and order, not ids) |
| B5 | Start, read all pages, stop without changes, report `report-B5`. | values identical to B4; hash identical to B4 |
| B6 | Derive "semantics match T002": `allowed_folders.allow_subfolders` default `0` for pre-existing rows; existing rows readable; saved flag `1` survives restart. | satisfied by B3-B5 |

### C. Current-schema database written by an earlier store (supports AC2/AC3)

| Step | Action | Expected / compare |
| --- | --- | --- |
| C1 | Fresh snapshot or state; install `current.db` as in B1; hash and `report-C1`. | hash equals the fixture hash |
| C2 | Start, read Folders, stop gracefully, hash, `report-C2`. | `FMT055 Current Study` shows *Allow subfolders* checked; **file hash unchanged** (the store's `OpensPreviousSqliteDatabase_ReadsAllRows_WithoutModifyingTheFile` test says opening writes nothing; confirm in the real lifecycle) |
| C3 | Start, uncheck the flag on `FMT055 Current Study`, add one app (`C:\FMT055\Apps\CurrentTool.exe` is in the fixture; add `C:\FMT055\Apps\FMT055Tool.exe`). Stop, report `report-C3`; start, read, stop, report `report-C4`. | `report-C3` shows the new app, flag `0`; `report-C4` values and hash equal `report-C3` |

## 6. Cleanup (guest only)

1. Stop `FocusManager.App` and `FocusManager.Agent` (tray Exit) and confirm with `Get-Process`.
2. Copy out only the evidence files (section 7) by the same one-way route; do not copy the guest's database into any
   host profile folder.
3. Revert the guest to the clean snapshot or delete the VM and its disk image. If a physical test machine was used,
   delete `%LOCALAPPDATA%\FocusManager` and `C:\FMT055` in that machine's account and reset it.
4. Host-side control values, if collected (section 0), are recorded again and compared.

## 7. Evidence to save under `docs/reviews/evidence/T055/`

* `run-log.md`: date, tester, VM product/version and guest edition/build, G1-G6 outputs, source revision and diff
  hash (section 1), the exact commands and manual steps actually used, deviations, and a verified / not verified /
  failed line for every scenario step.
* `report-*.txt` from section 3 (they contain only synthetic values), hash lines before and after each step, fixture
  hashes, screenshots of Dashboard, tray, and the three pages after each restart (check that nothing outside the
  guest is visible).
* Sanitize account and machine names to labels (`T055-VM`, `guest-user`); no product keys, tokens or personal data.

## 8. Not covered

Study mode, enforcement, browser policy, session-0 / service behavior, reboot persistence, concurrent agents and
Linux are outside this runbook. Real-database migration of the owner's file is not authorized by T055 (the owner's
database is not touched). A pass here supports T002 AC1 only for the sources actually tested; it is not owner
acceptance of T002.
