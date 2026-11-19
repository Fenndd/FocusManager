# T055 read-only code trace: what a launch touches

Source state: baseline HEAD `3035e03de6b3c25545bd6b252bfd1f5ab622aed9` plus the dirty working tree on 2026-11-18
(77 changed/untracked entries at inspection time; other agents were editing). Nothing here was executed; every
statement cites the source read. Line numbers refer to the working-tree files below (first 16 hex characters of
SHA-256 so a later reader can tell whether the source changed):

| File | SHA-256 (prefix) |
| --- | --- |
| `Start-FocusManager.cmd` | `fbca39d5cebe604c` |
| `scripts/windows/Start-FocusManager.ps1` | `0d692b864f3c65a2` |
| `scripts/windows/Start-FocusManager.AgentElevated.ps1` | `f3abe9278f6e4df2` |
| `src/Windows/FocusManager.Agent/Program.cs` | `782293b47f37bd71` (changed by another agent during this work: T054 added `WindowsHostPathRules` wiring; the earlier read was `4d995527b4e977a1`; the database path and store wiring statements are unaffected) |
| `src/Windows/FocusManager.Agent/app.manifest` | `b9de0203b7ce82c9` |
| `src/Windows/FocusManager.Agent/Services/IpcPipeFactory.cs` | `96f1475689cee7e7` |
| `src/Windows/FocusManager.Platform.Windows/ChromePolicyRegistry.cs` | `6a7ba9c910cc72c6` |
| `src/Shared/FocusManager.Persistence.Sqlite/SqliteWhitelistStore.cs` | `ffd732cb7f8d4b01` |
| `tests/Shared/FocusManager.Persistence.Sqlite.Tests/LegacySchemas.cs` | `b67d484429fe07e7` |

## 1. Launch sequence (`Start-FocusManager.cmd`)

1. `.cmd` runs `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts\windows\Start-FocusManager.ps1`.
2. `Start-FocusManager.ps1`
   * **Builds** `FocusManager.Windows.slnf` (Debug) with the MSBuild that `vswhere` or the fixed Visual Studio paths
     resolve; falls back to `dotnet build`. This writes `bin/` and `obj/` inside the repository checkout it runs from
     (in the owner's tree it would race with any other build in progress).
   * Locates `FocusManager.Agent.exe` (`src\Windows\FocusManager.Agent\bin\<cfg>\net8.0-windows[\win-x64]`) and
     `FocusManager.App.exe` (`src\Windows\FocusManager.App\bin\<cfg>\net8.0-windows10.0.19041.0[\win-x64]`).
   * `Start-Process powershell.exe ... Start-FocusManager.AgentElevated.ps1 -AgentExe <path> -Verb RunAs
     -WindowStyle Hidden` => **UAC elevation prompt** on the interactive desktop.
   * Sleeps `AgentStartupDelaySeconds` (2), then `Start-Process <App.exe>` (normal, un-elevated) in the current
     interactive session.
3. `Start-FocusManager.AgentElevated.ps1`
   * `Get-Process -Name "FocusManager.Agent"` then **`Stop-Process -Force` on every match**, then sleeps 1 s.
     The match is by process name only: there is no user, session, path or PID check. An elevated process sees and
     can kill agents of any session and user on that OS instance.
   * Starts the new agent: `Start-Process <Agent.exe> -WorkingDirectory <its dir>`.

## 2. Agent startup (`Program.cs`, `AgentHostedService.StartAsync`)

* `DatabasePath` = `Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
  "FocusManager", "focusmanager.db")` (Program.cs lines 21-30 in the working tree). No configuration, argument or environment variable
  can change it; there is no override hook. `DefaultFolders` = `System Root (C:)` / `C:\` / no subfolders (FLD-004).
* The store is lazy: constructing it does nothing. The first call is `IsStudyModeEnabledAsync()` in `StartAsync`.
  `OpenConnectionAsync` then creates the directory (`Directory.CreateDirectory`), creates the file if missing
  (`SqliteOpenMode.ReadWriteCreate`) and runs `InitializeSchemaAsync` inside an IMMEDIATE (write) transaction:
  `CREATE TABLE IF NOT EXISTS` for the four tables, `ALTER TABLE allowed_folders ADD COLUMN allow_subfolders INTEGER
  NOT NULL DEFAULT 0` when the column is missing, `INSERT ... study_mode_enabled='0' ON CONFLICT DO NOTHING`.
  **So merely starting the agent can write to an existing database** (it adds the column to a pre-2026-04-20 file and
  inserts a missing settings row). Journal mode stays the default rollback journal (`focusmanager.db-journal`
  appears transiently during writes; no WAL).
* If the stored `study_mode_enabled` is `1` (or `true`), the agent calls `EnableStudyModeAsync` at startup:
  captures a process snapshot, starts the WMI process-start monitor and the Explorer monitor, and applies the site
  policy. The process monitor leads to `AppEnforcer` terminating non-allowed new processes
  (`process.Kill(entireProcessTree: true)`, `AppEnforcer.cs` line 206) and the folder enforcer redirecting Explorer
  windows. If it is `0`/missing, the agent stops both monitors and calls `SiteEnforcer.ClearAsync`.
* A tray icon (`TrayHost`, WinForms `NotifyIcon`) is created in the interactive session; the tray "Exit" calls
  `StopApplication` (graceful stop).
* IPC loop (`RunIpcLoopAsync`): see section 4.

## 3. Identity after elevation and effective LocalApplicationData

* `Agent/app.manifest` requests `requireAdministrator`, so the agent is always elevated even if started directly.
* The launcher uses `-Verb RunAs`. Consequence depends on who runs it:
  * **Split-token administrator (the owner's case on this machine):** UAC "consent" produces an elevated token of the
    *same* account (same SID, same profile, same `HKCU`). The elevated agent runs as the owner, and
    `Environment.SpecialFolder.LocalApplicationData` resolves to the owner's profile. Its database is therefore the
    owner's live `%LOCALAPPDATA%\FocusManager\focusmanager.db`, which exists on the machine inspected
    ([environment-inspection.md](environment-inspection.md)).
  * **Standard user:** UAC asks for administrator credentials; the elevated process then runs as *that* administrator,
    with that account's profile, `HKCU` and (by pipe ACL, below) a different SID than the un-elevated UI user. Here the
    only enabled administrator is the owner account, so a second standard account would require the owner's
    credentials (which no agent may supply) and would write into the owner's profile.
* `Environment.GetFolderPath(LocalApplicationData)` is resolved from the user's known-folder configuration, **not**
  from the `LOCALAPPDATA` environment variable. Checked in-process with Windows PowerShell 5.1: overriding the
  variable did not change the result. (.NET 8 uses the same Windows known-folder API; not re-run because no
  `dotnet` execution was allowed.) Copying the repository or setting `LOCALAPPDATA` therefore cannot redirect the
  database; only another Windows user profile/OS instance can.

## 4. Named pipe

* Name: `IpcProtocol.PipeName = "FocusManager.Agent.Pipe"` (`FocusManager.Contracts/IpcProtocol.cs` line 6) =>
  `\\.\pipe\FocusManager.Agent.Pipe`, the machine-global local pipe namespace, not per user or per session.
* Server: `IpcPipeFactory.CreateServerStream` -> `NamedPipeServerStreamAcl.Create(..., MaxAllowedServerInstances ...)`.
  ACL: current (elevated) user SID ReadWrite, `BUILTIN\Administrators` FullControl, `LocalSystem` FullControl. The
  `FirstPipeInstance` option is not used, so a second agent on the same OS instance would create another instance of
  the same pipe name instead of failing; a client connecting to the name may reach either server instance.
* Client: the un-elevated UI (`AgentClient.SendAsync`) connects to `.`/`FocusManager.Agent.Pipe` with a 3 s timeout.
  If the agent runs as a different account from the UI user, the UI is not covered by the ACL (its
  `Administrators` SID is deny-only in a filtered token) and cannot connect.
* Consequence for isolation: any agent run in the owner's OS instance, from any account, shares this pipe name with
  the owner's agent and UI. VM guests have their own pipe namespace unless a host-bridged serial/named-pipe device
  is configured (the runbook forbids that).

## 5. Registry writes

Only one writer exists: `ChromePolicyRegistry` (`Platform.Windows`), always **`HKEY_CURRENT_USER`** of the agent's
account (the elevated owner => the owner's hive), key `Software\Policies\Google\Chrome`:

* `ApplyWhitelistAsync` (study mode enabled, or a `SaveWhitelist` while study mode is on): `CreateSubKey` for
  `...\Chrome`, `...\Chrome\URLBlocklist`, `...\Chrome\URLAllowlist`; clears their values; writes `URLBlocklist\1 = "*"`
  and `URLAllowlist\1..n` = normalized host patterns. This blocks all browsing in Chrome except the list.
* `ClearWhitelistAsync` (called from `SiteEnforcer.ClearAsync`: agent start with study mode off, `AgentHostedService.cs` line 87, and when study mode is disabled, line 310; `StopAsync` does **not** clear it):
  `OpenSubKey(...\Chrome, writable)`; if present, **`DeleteSubKeyTree` of `URLAllowlist` and `URLBlocklist`**
  (best effort). Opening does not create missing keys. So on the owner's machine an agent start with study mode off
  would delete any URL allow/block list under that key, including one not written by FocusManager.
  State seen: the `Chrome` key exists, both subkeys do not (existence check only), so a start today would not
  delete anything, but this is not a safe assumption for a later date.
* No other `Registry.*` use in `src/` (searched). `SaveWhitelist` with study mode off does not touch the registry.

## 6. Process effects

* Launcher: forced stop of all processes named `FocusManager.Agent` (section 1).
* Agent with persisted study mode on: terminates new non-allow-listed processes and tree-kills them; this could hit
  tools running in the owner's session. Fixtures for T055 therefore always set `study_mode_enabled` to `0`.
* WMI process watcher and Explorer COM interop attach to the session only when study mode is on.

## 7. What a launch on the owner's session would touch (summary)

| Resource | Effect | Unsafe because |
| --- | --- | --- |
| Repository `bin/` `obj/` | Rebuild | Another agent may be building; changes evidence state. |
| UAC desktop | Interactive consent prompt | Requires the owner to click; elevation of the owner's token. |
| Processes | `Stop-Process -Force` of any `FocusManager.Agent`; new elevated agent; tray icon; new UI window | Name-based, session-agnostic kill; visible UI in owner's session. |
| Owner database `%LOCALAPPDATA%\FocusManager\focusmanager.db` (exists) | Open, possible `ALTER TABLE`/`INSERT` on start, journal files; UI saves **delete and re-insert all rows** | Owner's live data, and the test needs to write values into it. |
| `\\.\pipe\FocusManager.Agent.Pipe` | New server instances, global name | Collides with any other instance. |
| `HKCU\Software\Policies\Google\Chrome` | Possible subtree deletion at start (study mode off); blocklist `*` if study mode on | Owner's browser policy. |
| Study mode (persisted flag) | If `1`: process termination, Explorer redirects | Could kill owner's apps. |

## 8. Storage-level view relevant to T002 AC1 (not a substitute for the lifecycle run)

Historical schemas used for the runbook fixtures are those in `LegacySchemas.cs`
(`WithoutAllowSubfolders`, up to commit 5f2e7e7; `WithAllowSubfolders`, commits f7abfaf to 31e6e31). The
`git show 31e6e31:src/Shared/FocusManager.Persistence.Sqlite/SqliteWhitelistStore.cs` `InitializeSchema` text equals
`WithAllowSubfolders` (including the column definition); the old store added the column via
`EnsureColumnExists(... "INTEGER NOT NULL DEFAULT 0")`, which is the same DDL the new store issues. UI saves go
through `SaveAsync`: `DELETE` of all three list tables then re-`INSERT`, so row ids change after any save; compare
values and order, not ids.
