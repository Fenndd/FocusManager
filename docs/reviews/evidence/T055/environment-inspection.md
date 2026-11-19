# T055 environment inspection (read-only)

Date: 2026-11-18. Purpose: establish, before any launch, whether a safe isolated Windows environment exists on the
machine that holds the repository. Nothing was started, installed, enabled, created or written; no `dotnet`
command, launcher script, agent, UI, kill, registry write, pipe connection or `%LOCALAPPDATA%\FocusManager` write
was performed. Machine and account names are replaced by labels. Linux items are listed only because they were
seen; none was used (owner hold J1).

## Commands run and results

| Question | Command (PowerShell 5.1, non-elevated) | Result |
| --- | --- | --- |
| Edition, hypervisor | `Get-ComputerInfo -Property WindowsEditionId,OsBuildNumber,HyperVisorPresent,...` ; `systeminfo` | Windows 11 Home (`WindowsEditionId` = Core), build 26200, x64. `HyperVisorPresent` = True (Windows' own hypervisor is active; `systeminfo`: Virtualization-based security Running; Hyper-V requirement rows suppressed because a hypervisor is already present). |
| Process elevation | `WindowsPrincipal.IsInRole(Administrator)` | False. This inspection ran un-elevated. |
| Optional features | `Get-WindowsOptionalFeature -Online` | **Failed: "The requested operation requires elevation."** Feature state (Hyper-V, Containers-DisposableClientVM, VirtualMachinePlatform) is therefore **not verified** by that command; the file/command checks below were used instead. |
| Hyper-V management | `Get-Command Get-VM`; `Test-Path %windir%\System32\vmms.exe` | `Get-VM` not found; `vmms.exe` absent. No Hyper-V manager role on Home edition. |
| Windows Sandbox | `Test-Path %windir%\System32\WindowsSandbox.exe`; `Get-Command WindowsSandbox` | Absent / not found. |
| Hyper-V platform pieces | `Get-Service` filtered | `vmcompute` and `HvHost` Running, `hns` Running: the host compute/hypervisor platform (used by WSL2/containers) is present; this is not a way to run a Windows guest. |
| Other virtualization | `Get-Command VBoxManage, vmrun, vmware, qemu-system-x86_64, multipass, docker, vagrant, packer` ; `Test-Path` of install dirs | Not on `PATH`: all of them. Installed on disk: **VirtualBox 7.1.10** (`...\Oracle\VirtualBox`; `VBoxManage --version` = `7.1.10r169112`). Not present: VMware, Docker, QEMU. |
| Existing VMs | `VBoxManage list vms`; `VBoxManage list runningvms`; `Get-VM` unavailable | 1 registered VM, a **Linux** guest; none running. It was not opened, started or inspected further (owner hold J1). **No Windows VM exists.** |
| Windows install media | Narrow name search (`win`/`windows`/`w10`/`w11`) for `*.iso, *.vhd, *.vhdx, *.wim, *.esd`, depth 2, in the user's Downloads, Desktop, Documents and `VirtualBox VMs` folders | 0 files. No Windows installation media or prebuilt Windows disk was found in those places. (Other folders and external drives were not searched.) |
| WSL | `Get-Command wsl` | `wsl.exe` exists. It is Linux-only and is not a Windows environment; not used. |
| Local accounts | `Get-LocalUser` (properties only) | 9 local accounts: the current account (a Microsoft-account-backed user), 3 other enabled local accounts, 5 disabled. |
| Administrators group | `Get-LocalGroupMember Administrators` | Exactly two members: the built-in Administrator (disabled) and the current owner account. **The only enabled administrator is the owner account.** |
| UAC policy | `HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System` (read) | `EnableLUA`=1, `ConsentPromptBehaviorAdmin`=5, `ConsentPromptBehaviorUser`=3 (credential prompt), `PromptOnSecureDesktop`=1, `FilterAdministratorToken`=1. |
| Current token | `whoami /groups` | Medium Mandatory Level; `BUILTIN\Administrators` is "Group used for deny only" => split-token administrator (Admin Approval Mode). |
| Capacity | `systeminfo`, `Get-PSDrive` | RAM 14 188 MB total / about 3 GB available at the time of the check; drive C: about 129 GB free. |
| Owner resources present | `Test-Path` only (no content read) | `%LOCALAPPDATA%\FocusManager\focusmanager.db` **exists** (owner's live data). `HKCU:\Software\Policies\Google\Chrome` exists; its `URLAllowlist` and `URLBlocklist` subkeys do not. No `FocusManager*` process is running. No `FocusManager.Agent.Pipe` is listed in `\\.\pipe\`. |
| Env-var override | In-process: set `$env:LOCALAPPDATA` to a bogus path, then `[Environment]::GetFolderPath('LocalApplicationData')` | Returned the real profile path unchanged. **Verified for the .NET Framework call used by PowerShell 5.1**; the .NET 8 call in `Program.cs` uses the same known-folder API but was not re-run (no `dotnet` execution allowed). So a `LOCALAPPDATA` override does not redirect the agent database. |

## Conclusions

1. **No Windows Sandbox, no Hyper-V.** Home edition; the required binaries are absent. Enabling anything (and the
   unsupported Hyper-V-on-Home workarounds) would change the owner's system and is outside this task.
2. **No existing disposable Windows VM or dedicated Windows test machine was found.** VirtualBox is installed, but it
   holds only a Linux guest, and no Windows ISO/disk was found in the searched user folders. Creating a Windows VM
   needs a Windows image and licence/evaluation choice, download, disk (about 60 GB) and RAM (about 4 GB, but only
   about 3 GB was free at inspection) and a decision to use VirtualBox on a host where the Windows hypervisor is
   already active; those are owner actions and decisions. Whether VirtualBox runs a Windows guest acceptably on this
   host (hypervisor-present mode) is **not verified**.
3. **A second Windows account on this machine is not isolation** (acceptance criterion rule) and here is also unsafe:
   the launcher elevates with `Start-Process -Verb RunAs`; a standard account would get a credential prompt, and the
   only enabled administrator is the owner account, so the elevated agent would run as the owner and use the owner's
   `LocalApplicationData` and `HKCU`. The pipe namespace and process-name stop are machine-wide. Details:
   [code-trace.md](code-trace.md).
4. **Outcome: BLOCKER.** Missing: a disposable Windows 10/11 x64 VM or a dedicated test machine. No substitute check
   was run.
