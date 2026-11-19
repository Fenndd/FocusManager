# Windows platform (WIN)

A primary platform (decision B1), developed in parallel with Android (decision B2).
Desktop architecture — ADR-0007: a C#/.NET agent plus a Tauri UI (decision C3). Linux remains on owner hold.

Owner answers [K1](../decisions.md#k1-windows-service-and-session-behavior), recorded on 2026-11-18, govern
new behavior. [T058](../../../tasks/T058-reconcile-service-design-with-owner-decisions.md) must reconcile
ADR-0009 and its tasks with them. ADR-0009 remains Proposed; product answers do not accept that architecture
or establish implementation success. T054/T055/T056 are concurrently owned and are not rewritten by this update.

## Components

| Component | Project | Role |
| --- | --- | --- |
| UI | Currently `FocusManager.App` (WinUI 3); target Tauri (T017/T018) | Profiles, session control, settings, tray and account UI |
| Agent | Currently elevated console `FocusManager.Agent` with WinForms tray; target one Windows service for all protection levels | Monitoring, rule enforcement, storage and lifecycle coordination |
| IPC | `FocusManager.Contracts` | UI-agent named pipe `FocusManager.Agent.Pipe` (current JSON line framing) |
| OS integrations | `FocusManager.Platform.Windows` | WMI, Explorer COM and browser policies |

## Requirements

### WIN-001 Agent with administrator privileges
The current agent runs elevated (`requireAdministrator` manifest) for its Windows mechanisms, including
WMI process-start monitoring. The target service must keep privileged operations separate from user UI;
the exact service identity/privilege set remains subject to accepted design and verification.

Platforms: Windows — done (current elevated agent; service host not implemented)

### WIN-002 IPC access
Each authenticated Windows user can access only their own FocusManager data and session controls.
No application/API administration panel lets an administrator view/change another user's profiles or end
another user's session (K1 Q2/Q8). Privileged service identity does not authorize an arbitrary caller.
This is application authorization, not a promise to defeat administrators acting through OS powers.

Platforms: Windows — partial (current console-agent ACL exists; service-model authorization is planned)

### WIN-003 Ignored processes and session boundaries
Restrictions affect only the associated Windows OS session; another user's session and another simultaneous
session of the same account must not be restricted implicitly (K1 Q3/Q15). Data is partitioned by Windows
user; runtime enforcement scope also identifies the associated session. Switching accounts is currently an
accepted protection boundary; optional switching restriction is PROT-012.

System/essential process exceptions remain necessary. The current implementation ignores the Windows
directory, `Program Files\WindowsApps`, unresolved paths and known system names; these are implementation
facts, not authority to terminate a process whose owner has not been established. FocusManager's own
components must be exempted by verified installed identity/location rather than filename alone (K1 Q12).

Platforms: Windows — partial (current session filtering/exceptions exist; verified own-component identity and service/session reconciliation are planned)

T061 selects File Explorer, Clock and Calculator as removable ordinary defaults in
[Default Exceptions](../apps.md#app-007-basic-essential-applications). The current Windows/WindowsApps
directory exemptions are broader than this desired set; future enforcement tasks must reconcile them so
removing an ordinary utility really removes its default permission. Do not claim that changing a settings
list alone changes current enforcement, or terminate Explorer's desktop-shell role as a side effect.
> ❓ Question (T058): what safe bounded retry/reporting behavior applies when process ownership is not yet known? Do not guess a different user's rules or claim enforcement complete.

### WIN-004 Application blocking method
A forbidden process is terminated together with its child process tree, subject to verified scope and
system/essential exceptions. When the service starts or recovers with active rules it also checks
already-running forbidden applications and terminates them; recovery creates no blanket new snapshot
exemption (K1 Q5; SM-002/SM-003).

Platforms: Windows — partial (new-launch termination exists; startup/recovery sweep is planned)

### WIN-005 Website blocking method
Supported-browser restrictions must apply only in the associated Windows OS session (K1 Q15). Machine-wide
HKLM policies are not acceptable if they restrict other sessions; this supersedes the conflicting part of
historical I5. The previous implementation approach uses each browser's managed policies, currently only
Chrome via HKCU. Per-SID HKCU/HKEY_USERS targeting alone does not establish isolation between simultaneous
sessions of one Windows account. T058 must validate the mechanism or present alternatives before implementation.

Restrictions are removed at session end, allowed service stop and service failure under WIN-010/SM-004;
only policies owned by FocusManager may be restored/removed.

Platforms: Windows — partial (Chrome HKCU enforcement exists; required session isolation and cleanup are not verified)

### WIN-006 Storage
Settings, profiles and statistics are stored locally in SQLite (C6); synchronization uses Firebase when
needed (K3/SYNC). User data must survive application updates. Current development databases are entirely
test data and may be recreated if migration cost is disproportionate; this exception does not cover real
future user data (K1 Q11). No database deletion is performed by this specification update.

Platforms: Windows — partial
Note: allow-lists and the session flag currently use `%LOCALAPPDATA%\FocusManager\focusmanager.db`
(T002); profile/statistics schemas are not implemented. Service-owned storage and any needed import remain
design/implementation tasks. A privileged service must not open untrusted profile paths merely to migrate data.

### WIN-007 Tray, exit and application startup
Closing/hiding the UI leaves active enforcement running. Ending a session follows its protection-level
check. A controlled service stop is separate and must first satisfy the applicable allowed-exit conditions
(K1 Q4/Q16). One user's ordinary UI exit must not disable other users' enforcement; T058 reconciles this
with the machine-wide service host.

The application starts at OS login by default, with a setting to disable that startup. A separate setting
controls automatically starting blocking when the application launches; it is disabled by default (K1 Q7).
Recovering an already-active session under SM-002 is distinct from starting a new session through this
setting. Do not force a deliberately closed main UI to reopen; background notification delivery remains
required when enabled.

Platforms: Windows — partial (tray exists; exit semantics and startup settings are planned)

### WIN-008 Development launch script
`Start-FocusManager.cmd` builds `FocusManager.Windows.slnf`, restarts the current console agent with
elevation, and launches the UI. This development launcher is not evidence of the target service lifecycle.

Platforms: Windows — done

### WIN-009 Minimum version
The current declared minimum is Windows 10 version 1809 (build 17763), x64. The owner allows a higher minimum
for Store/service packaging in principle (K1 Q14). Select the actual minimum only after packaging/install/
update/uninstall evidence; acceptance of that direction alone does not change this minimum.

Platforms: Windows — done (current target; Store/service variant not verified)

### WIN-010 Agent as a Windows service
One Windows service serves all protection levels (K1 Q1). A running/installed service alone does not activate
blocking. The target UI/tray runs in the user's session; the service has no user-facing windows in session 0.

After an allowed controlled stop, all ongoing FocusManager restrictions are removed. After a crash or forced
termination, a separate mechanism must also remove ongoing restrictions (owner follow-up to K1 Q16).
This requirement applies regardless of strict level; controlled-stop authorization is checked before stopping.
Recovery of an active session then reapplies its rules and checks running applications under SM-002.

Platforms: Windows — planned
> ❓ Question (T058): define and verify the independent cleanup mechanism, detection/cleanup latency, failure reporting, and ordering against service recovery. Stale cleanup must not remove newly recovered rules or another session's rules. No implementation proof or zero-delay guarantee exists yet.
