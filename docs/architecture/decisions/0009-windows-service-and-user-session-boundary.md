# ADR-0009: Windows Service and User-Session Boundary

- Status: Proposed
- Date: 2026-11-17 (revised 2026-11-18 after independent review; D6 and D10 clarified by T056 on 2026-11-18 and 2026-11-19)
- Extends: ADR-0007 (desktop architecture)
- Amends (if accepted): ADR-0007, "UI–Agent Communication": the line "access limited to the current user"
  is replaced by D4 and D5 (interactive local users, each limited to their own data by service-side
  authorization). It also refines the Components table entry "Windows service (administrator/root
  privileges)" to LocalSystem plus a per-session helper (D1, D2). WIN-002 ("accessible only to the current
  user, administrators, and SYSTEM") changes accordingly (Q8). ADR-0007 is not edited by this record.
- Acceptance: pending owner review (T027). This record proposes choices; none of them is accepted,
  and nothing in it is an approved requirement. Product questions are listed under
  [Open Questions for the Owner](#open-questions-for-the-owner) and must be answered in `docs/spec/`
  before an implementation relies on them. The implementation tasks T047–T052 and T016 do not start
  until this ADR is accepted.
- Post-review clarification ([T056 result](../../../tasks/T056-clarify-proposed-service-design-after-review.md#result),
  2026-11-18 and 2026-11-19): D6 now defines how the service-owned data tree is created and opened safely
  (rules R1–R8 and R2g, adversarial checks DT1–DT14 with the sub-rows DT1b, DT3b, DT8b, DT11b and DT13b, check V13);
  D10 states how the retained console tray is kept out of session 0, and which test seams that needs (checks V14,
  V15, M15, M16); Q17–Q18 and assumptions A21–A30 were added. An independent read-only review of 2026-11-19
  (verdict accept-with-fixes) was applied on 2026-11-19: its findings and what was done with each are listed in
  the T056 Result. These are
  technical clarifications of the proposal. The status stays Proposed, Q1–Q16 are not edited or answered by this
  revision (Q17 was extended; no other question was added), and nothing here has been built, run or tested. See
  [Source status for D6](#source-status-for-d6).
- Owner answers of 2026-11-18 (decisions K1 and L in [docs/spec/decisions.md](../../spec/decisions.md)) supersede
  conflicting proposals in this record, for example the "always or strict only" question Q1, the Q5 snapshot
  wording in D8, the Q11 import wording in D6 and the `HKLM` remarks in D7. They are product decisions, not
  acceptance of this ADR. T058 reconciles the design with them; T056 did not edit those passages.

## Context

WIN-010 and decision I5 move the Windows agent into a Windows service so that an ordinary user cannot
simply terminate it. ADR-0007 gives screens, tray and notifications to a Tauri UI in the user's session.
Today the agent is an elevated console process that `Start-FocusManager.cmd` starts in the user's own
session (`Start-FocusManager.ps1:98-100`). The code assumes that the agent's session, user account,
`HKEY_CURRENT_USER` and desktop are the user's. Under the service control manager (SCM) these assumptions
fail without an error:

- All services run in session 0. Since Windows Vista, services cannot interact with the user directly, and a
  LocalSystem service must not create windows on the interactive desktop ([Interactive Services]).
- For LocalSystem, `HKEY_CURRENT_USER` is associated with the default user, not the logged-on user
  ([LocalSystem account]), and the mapping is cached for the whole process ([RegOpenCurrentUser]).
- A program running in the context of a service has session ID 0 ([WTSQueryUserToken]).

Constraints: a refactoring preserves behavior unless the specification changes first (AGENTS.md); blocking
does not depend on a running UI, and the UI talks to the agent only through `contracts/` (architecture
rules 4 and 5); SQLite storage receives its path from the Windows agent (T002); session orchestration moves
to shared code before the host changes (T003); preventing service termination at the strict level belongs
to M7 (planned by T034).

### Session-0 hazards in the current source

Line numbers refer to the working tree checked on 2026-11-18 (H2, H4, H6 and H10 were re-checked against the
files in the review round). Uncommitted T001/T002 changes to `Program.cs` may move its lines.

| Short name | Path |
| --- | --- |
| `AppEnforcer.cs`, `FolderEnforcer.cs` | `src/Windows/FocusManager.Agent/Enforcement/` |
| `AgentHostedService.cs`, `IpcPipeFactory.cs`, `SessionSnapshotService.cs` | `src/Windows/FocusManager.Agent/Services/` |
| `TrayHost.cs` | `src/Windows/FocusManager.Agent/Tray/` |
| `TrayNotifier.cs` | `src/Windows/FocusManager.Agent/Notifications/` |
| `Program.cs`, `FocusManager.Agent.csproj`, `app.manifest` | `src/Windows/FocusManager.Agent/` |
| `ExplorerInterop.cs`, `ChromePolicyRegistry.cs`, `WmiProcessWatcher.cs` | `src/Windows/FocusManager.Platform.Windows/` |
| `Messages.cs`, `IpcProtocol.cs` | `src/Windows/FocusManager.Contracts/` |
| `AgentClient.cs` | `src/Windows/FocusManager.App/Services/` |
| `Start-FocusManager.ps1`, `Start-FocusManager.AgentElevated.ps1` | `scripts/windows/` |

| # | Hazard | Source | Effect when the agent is a LocalSystem service |
| --- | --- | --- | --- |
| H1 | Application blocking compares a process's session with the agent's own session | `AppEnforcer.cs:12` caches `Process.GetCurrentProcess().SessionId`; `:101-104` ignore every process from another session; `:96-99` also ignore a process whose session cannot be read | The agent's session is 0, so every user process belongs to "another session": application blocking stops without an error. |
| H2 | Explorer interaction uses shell COM, window handles and ShellExecute | `ExplorerInterop.cs:149-167` and `:291-309` create `Shell.Application` and enumerate `Windows()`; window handles at `:193`, `:362`; 1-second poll at `:26`; `OpenExplorerWindow` at `:396-407` starts `explorer.exe` through ShellExecute and is the fallback at `:69-75`; `FolderEnforcer.cs:81-140` depends on these operations | Explorer windows belong to the user's session and desktop. From session 0 the shell window list is expected to be empty or unavailable (assumption A1), so folder blocking stops. The fallback would start `explorer.exe` as LocalSystem in session 0 (assumption A2). |
| H3 | The agent owns the tray and notifications | `TrayHost.cs:34-41` (STA UI thread), `:148-154` (`NotifyIcon`), `:166` (`Application.Run`), Exit item `:141-142`; lifecycle coupling in `AgentHostedService.cs:27,56,67-68,76,90,110,249,393`; tray Exit stops the agent at `:397-400`; `TrayNotifier.cs:9-28` requires `TrayHost`; `Program.cs:33` registers `SilentNotifier`; `FocusManager.Agent.csproj:4` sets `UseWindowsForms` | Nothing in session 0 is visible to users, and a LocalSystem window on an interactive desktop is an attack surface ([Interactive Services]). The tray, its Exit command and notification display must leave the agent. Until T016 removes the code, D10 keeps the retained console tray out of session 0. |
| H4 | IPC access is derived from the agent's own identity | `IpcPipeFactory.cs:13-20` grants the process's own user SID; `:21-28` grant `FullControl` to Administrators and SYSTEM; `:30-40` set neither first-instance nor remote-client rejection (the options are `PipeOptions.Asynchronous` only, `:35`); one listening instance at a time in `AgentHostedService.cs:127-156`; unbounded `ReadLineAsync` at `:166`; exception text returned at `:186`; no authorization in `:193-234`; the client does not verify the server (`AgentClient.cs:68-72`) | As LocalSystem the granted SID becomes S-1-5-18, so interactive users cannot connect. A non-elevated administrator is refused too, because the filtered token has its administrative SIDs removed ([How UAC works]). `FullControl` includes `FILE_CREATE_PIPE_INSTANCE`. A stalled or oversized request can block or exhaust a privileged process. |
| H5 | Browser policies are written to `HKEY_CURRENT_USER` | `ChromePolicyRegistry.cs:31`, `:71` (`Registry.CurrentUser`) | Policies land in the LocalSystem/default-user hive instead of the user's hive, so site blocking (WIN-005) stops without an error. |
| H6 | Storage is planned at a per-user profile path | T002 (status `review`) restores `%LOCALAPPDATA%\FocusManager\focusmanager.db` (`tasks/T002-restore-sqlite-storage.md:31`; WIN-006 note in `docs/spec/platforms/windows.md:55-56`); the uncommitted `Program.cs:21-29` resolves it from `Environment.SpecialFolder.LocalApplicationData` (`:25`) | Resolved as LocalSystem, the path points to the service's own profile. A user's folder can be resolved only with that user's token and a mounted hive ([SHGetKnownFolderPath]). File operations by LocalSystem in a user-writable directory can be redirected by user-created links (assumption A16), and the user can edit the data, which conflicts with PROT-004 later. |
| H7 | Privilege is attributed to the manifest | `app.manifest:7` (`requireAdministrator`); WIN-001 in `docs/spec/platforms/windows.md:18-22`; WMI trace query at `WmiProcessWatcher.cs:25` | The SCM does not use the manifest; privileges come from the service account. A helper started by the service must use `asInvoker`. |
| H8 | FocusManager's own user-session processes can be blocked | Exemption by file name only: `AppEnforcer.cs:17-37` (names at `:35-36`, check at `:114`); the snapshot is taken when the agent starts (`AgentHostedService.cs:75-81`, `:259`) | After a reboot the snapshot is taken before logon. The helper, the Tauri UI and its WebView2 processes start later and have new names, so in allowlist mode they would be terminated. A name-only exemption also lets any executable renamed to `focusmanager.app.exe` avoid blocking (existing behavior). |
| H9 | Monitoring fails open without retry | `AgentHostedService.cs:271-281` and `:283-293` continue "without app-launch blocking" or "without folder blocking"; an unresolved path becomes a bare name (`WmiProcessWatcher.cs:84-95`) and is then ignored (`AppEnforcer.cs:119-127`) | At boot the WMI service may not be ready when the agent starts; enforcement would then be missing for the whole session with only a log entry. |
| H10 | Diagnostics are not configured for a service | `FocusManager.Agent.csproj:10` references the console logger package; `Program.cs` configures no other sink | A service has no console. The .NET Windows service guidance logs to the Event Log, whose default minimum severity is Warning and whose source and level are configurable ([.NET Windows service], re-read 2026-11-18). Information-level diagnostics, such as the monitor and helper state changes proposed here, are lost unless S5 configures the level or another sink. Choosing a file or other durable sink is not part of this proposal. |
| H11 | Development scripts assume an elevated console process | `Start-FocusManager.AgentElevated.ps1:12-17` (`Stop-Process -Force` at `:15`), `:20`; `Start-FocusManager.ps1:82-83` (binary inside the repository), `:98-100` (`-Verb RunAs`) | Killing a service process is a crash for the SCM. A LocalSystem service whose binary sits in a user-writable directory lets that user's processes run code as LocalSystem. A binary path with spaces must be quoted ([CreateService]), and the repository path contains spaces. |
| H12 | The protocol has no version, no error codes and no agent-to-UI channel | `Messages.cs:32-54`; one request per connection in `AgentHostedService.cs:158-191` | The UI and the service can be different versions during an update. Tray state (`AgentHostedService.cs:249`) and notifications (T014) need events from the agent. |
| H13 | The snapshot is recaptured on every start | `SessionSnapshotService.cs:14-24` (process IDs only); `AgentHostedService.cs:75-81` | After a service crash or restart, processes started while the agent was down join the new snapshot and stay allowed. Whether that is intended is an SM-002/SM-003 question (Q5). |

## Decision

Proposed. D1–D10 below, together with the identifiers H1–H13, Q1–Q18, A1–A30 and the checks V1–V15 and
M1–M16, are stable references for tasks.

### D1. Components and responsibilities

| Component | Identity and placement | Responsibilities | Excluded |
| --- | --- | --- | --- |
| Agent service (`FocusManager.Agent`) | Windows service, LocalSystem, session 0, automatic start | Process-start monitoring and termination; rule evaluation and session state through the shared controller from T003, one instance per OS-user partition (D5); storage (D6); browser-policy application, ledger and cleanup (D7); session tracking and helper supervision (D3, D8); IPC server, caller identity and authorization (D4, D5); service logging (D2) | Windows, tray icons and message boxes; shell COM and ShellExecute; `Registry.CurrentUser` and `%LOCALAPPDATA%`; opening files in user profiles as LocalSystem; connecting as a client to an endpoint created in a user session |
| Session helper (`FocusManager.SessionHelper`, new) | One per enforced interactive session, running with that session user's token, `asInvoker`, no visible window, started by the service | Monitoring Explorer windows and applying corrective navigation (`ExplorerInterop`); reporting folder events; executing navigation commands; exiting when its service connection closes | Rules, storage and decisions; tray; notifications unless Q6 assigns them |
| Desktop UI (Tauri, T017) | User process at medium integrity, started by the user or by autostart (Q7) | Screens, tray, notifications (T014), later sign-in; IPC only from the Rust core and only after verifying the server (D4) | Being required for enforcement; IPC from WebView JavaScript |

### D2. Service account and installation constraints

- Account: LocalSystem. `WTSQueryUserToken`, which D3 needs, requires LocalSystem with `SE_TCB_NAME`
  ([WTSQueryUserToken]). The agent also terminates processes of any user, writes other users' hives and later
  `HKLM`, and uses the WMI process trace (WIN-001; assumption A8). LocalSystem has `SE_TCB_NAME`, `SE_DEBUG_NAME`
  and `SE_IMPERSONATE_NAME` enabled ([LocalSystem account]).
- Reduced privileges: after measuring actual use, declare the required privileges so that the SCM removes all
  others from the process token ([SERVICE_REQUIRED_PRIVILEGES_INFO]). T052 (S5) records the final list and the
  evidence for each entry. Inputs to that list, none of them final:
  - `SE_TCB_NAME` for `WTSQueryUserToken` ([WTSQueryUserToken]);
  - `SE_INCREASE_QUOTA_NAME`, and possibly `SE_ASSIGNPRIMARYTOKEN_NAME`, for `CreateProcessAsUser`. The
    documentation says the caller "must have" the first and "may require" the second "if the token is not
    assignable" ([CreateProcessAsUser], re-read 2026-11-18);
  - `SE_IMPERSONATE_NAME` for the impersonation proposed in D5 (identification level may need less; to be
    measured) and in D7 (writing the owner's hive);
  - rights to open and terminate processes of other users and of elevated processes (`PROCESS_TERMINATE` is
    subject to the target's DACL; whether `SE_DEBUG_NAME` is needed is to be measured);
  - whatever the WMI process trace and hive access need (assumption A8).
  Check M14 starts the helper and exercises every service function with the reduced set; a privilege is
  removed from the list only when M14 passes without it.
- Start type: automatic, not delayed. A delayed service has no specific time guarantee
  ([SERVICE_DELAYED_AUTO_START_INFO]), which would widen the unprotected window after boot. The service depends
  on the WMI service (assumption A8).
- Image: installed in a directory only administrators can write, registered with a quoted image path
  ([CreateService]), never registered from the repository build output (H11).
- Recovery: SCM restart actions plus `fFailureActionsOnNonCrashFailures`, so that a stop with a non-zero exit code
  also counts as a failure ([SERVICE_FAILURE_ACTIONS_FLAG]). A fatal error ends the process with a non-zero exit
  code, because a clean host stop does not trigger recovery ([.NET Windows service]). T016 sets the delays.
- Hosting: `Microsoft.Extensions.Hosting.WindowsServices` with `AddWindowsService` ([.NET Windows service]).
  Logging: the Event Log provider with a configured source name and level. Its default minimum severity is
  Warning, so Information-level diagnostics need explicit configuration (H10). Session-change notifications come through `ServiceBase.OnSessionChange` on a lifetime
  derived from `WindowsServiceLifetime`, which is a public, non-sealed `ServiceBase` subclass
  ([WindowsServiceLifetime]). The exact wiring is assumption A10.
- Standard users cannot manage services by default ([.NET Windows service]). Administrators can still stop the
  service; preventing that is M7 work.

### D3. Session helper placement and lifecycle

- Placement: a separate C# executable that the service starts with `CreateProcessAsUser`. This is Microsoft's
  documented pattern for a service that needs work done in the interactive user's context
  ([Interactive Services]). `ExplorerInterop` stays in `FocusManager.Platform.Windows`; only the helper creates it.
- When: in every interactive session whose owner (D5) has an active FocusManager session. The helper process
  exits when that FocusManager session ends, when the owner logs off, or when the service stops; the service
  ends it by closing the helper pipe (ordering constraint 6). It is launched again at the next FocusManager
  session start or logon. This mirrors the current start and stop of `ExplorerMonitor`
  (`AgentHostedService.cs:283-286`, `:307-308`). "Stopping the helper's monitoring" therefore means that
  the helper exits; the helper has no idle mode.
- Launch: `WTSQueryUserToken` for the session; the user's environment block; `CreateProcessAsUser` with the full,
  quoted application path inside the install directory, desktop `winsta0\default`, and no handle inheritance,
  because handles cannot be inherited across sessions ([CreateProcessAsUser]). Token handles are closed
  promptly ([WTSQueryUserToken]). Window station and desktop: the `CreateProcessAsUser` documentation says
  that "the DACLs for the window station and desktop must grant access to the user or the logon session
  represented by the *hToken* parameter" and that the caller must change the DACLs of the default interactive
  window station and desktop before calling ([CreateProcessAsUser], re-read 2026-11-18). This proposal expects
  that requirement to be met already, because the token belongs to the logged-on interactive session whose
  `winsta0\default` grants that session's logon SID, so the service needs no DACL change (assumption A4,
  not verified; A3 for the token type). If the S5 test shows otherwise, S5 stops and reports instead of
  editing a window-station or desktop DACL from LocalSystem, because widening those DACLs would be a
  new security decision for the owner.
- Channel: a per-session pipe `\\.\pipe\FocusManager.Agent.Helper.<sessionId>` that the service creates before
  launching the helper. Its DACL allows only LocalSystem and the logon SID of that session's token, which keeps
  other sessions out ([Named pipe security]). Remote clients are rejected and the first-instance flag is set.
  The service accepts one helper connection per session and checks two things: the client's session, and
  that the client process ID matches the launched process, together with a per-launch generation value. These
  checks reject stale or accidental clients but are not a secret: a process running as the same user can read or
  end the helper.
- Role: a thin sensor and actuator. The helper reports `FolderOpened(path, windowToken)`. The existing
  `FolderEnforcer` stays in the service and calls a session-routed `IExplorerNavigator`. The navigator's
  operations run in the helper: read the current folder, go back, close, navigate a window, open a folder.
  Window tokens are opaque to the service. Rules, decisions, repeat suppression (`FolderEnforcer.cs:163-187`),
  logging and notifications stay in one place, so FLD-001–FLD-007 behave as before.
- Supervision: when the helper exits or disconnects, the service relaunches it with exponential backoff while
  the session is logged on and still needs folder enforcement, and reports `helperUnavailable` to the UI
  meanwhile (D9). The service never relaunches a helper into a session that is logging off.
- Limit: the helper's default security descriptor comes from the user's token, so the user can end it
  ([CreateProcessAsUser]). Preventing that is protection work (M7, Q10).

### D4. IPC endpoints and access control

- The UI pipe keeps its name `FocusManager.Agent.Pipe` (`IpcProtocol.cs:6`) and its line-delimited JSON, so the
  WinUI client keeps working until T018.
- Amendment of ADR-0007: ADR-0007 says "access limited to the current user". Under a LocalSystem service there is
  no "current user" at the service, and several interactive users may use one computer. D4 and D5 replace the
  sentence with: any local interactive user may connect, and the service limits each caller to that caller's own
  data. This is a change of an accepted decision and of WIN-002, so it needs the owner's acceptance of this ADR
  (Q2, Q8).
- Security descriptor (illustrative SDDL; tests check the effective descriptor):
  `O:SYG:SYD:P(D;;GA;;;NU)(A;;GA;;;SY)(A;;0x12019b;;;IU)S:(ML;;NW;;;ME)`.
  - The owner is LocalSystem, set explicitly because the LocalSystem token also contains the Administrators SID
    ([LocalSystem account]).
  - NETWORK is denied.
  - Only LocalSystem has full access.
  - INTERACTIVE ([Well-known SIDs]) receives read and write data, attributes, `READ_CONTROL` and
    `SYNCHRONIZE`, but never `FILE_CREATE_PIPE_INSTANCE`. `FILE_GENERIC_WRITE` would include that right
    ([Named pipe security]). In .NET this is `PipeAccessRights.ReadWrite` plus `Synchronize`, never `FullControl`
    or `CreateNewInstance` for anyone but LocalSystem.
  - A medium no-write-up label stops low-integrity and AppContainer processes from writing
    ([Mandatory Integrity Control]; assumption A15).
- Administrators are not listed separately. Interactive administrators are covered by INTERACTIVE, and no
  current request needs non-interactive administrative access. WIN-002, which names administrators, needs
  rewording (Q8).
- Instances: the first instance is created with `FILE_FLAG_FIRST_PIPE_INSTANCE` (`PipeOptions.FirstPipeInstance`),
  so that an existing pipe with the same name is detected ([CreateNamedPipe], [PipeOptions]). Remote clients are
  rejected (`PIPE_REJECT_REMOTE_CLIENTS`; assumption A9). At least one instance is always listening. Each
  connection has a read deadline, a maximum request size and a place under a connection cap. Failures return
  error codes, not exception text. If the first instance cannot be created because the name is already held (by a
  console agent, or by a process a standard user started to squat the name), the server never serves on a
  non-first instance; the service-side reaction is stated in D10, item 5, and T051 delivers the failure result.
- Clients (the Tauri Rust core, the helper, and the WinUI `AgentClient` until T018) request identification-level
  impersonation only. Before sending data, a client verifies that the pipe owner is LocalSystem, which the
  `READ_CONTROL` right allows; a mismatch counts as "agent unavailable". This check starts with the service host,
  because in console mode the pipe owner is the console process's user.
- The service never connects as a client to a pipe or other endpoint created in a user session.

### D5. Caller identity, authorization and enforced sessions

For every request the service:

1. Reads one bounded request line. Impersonation uses the security context of the last message read
   ([ImpersonateNamedPipeClient]).
2. Impersonates the client. If impersonation fails, it executes nothing ([ImpersonateNamedPipeClient]).
3. Reads from the thread token the user SID, the session ID, the integrity level, membership of INTERACTIVE and
   NETWORK, the AppContainer flag, the elevation type and the logon ID, then reverts. The session is cross-checked
   with `GetNamedPipeClientSessionId` ([GetNamedPipeClientSessionId]). The client process ID is used for
   diagnostics and helper correlation, never for authorization (assumption A13).
4. Classifies the caller:
   - an interactive user: INTERACTIVE present, NETWORK absent, integrity at least medium, not an AppContainer,
     session not 0;
   - LocalSystem: S-1-5-18;
   - anything else is refused.
5. Uses the caller's user SID as the data partition. Requests never carry a user or session identifier; the
   service derives both.

| Request | Interactive user | Another interactive user | LocalSystem | Other callers |
| --- | --- | --- | --- | --- |
| `GetAgentInfo` (D9) | Allowed | Allowed; no user data | Allowed | Refused |
| `GetStudyModeStatus`, `GetWhitelist` | Own partition | Own partition only | Diagnostics | Refused |
| `SetStudyMode`, `SaveWhitelist` | Own partition; enforcement in own sessions | Own partition only | Not used | Refused |
| Helper messages | Only on the helper pipe, from the helper of that session | Refused | Refused | Refused |
| Uninstall cleanup | Refused | Refused | Through the elevated cleanup command (D8), not through IPC | Refused |

Elevated administrators are interactive users with their own partition. Whether they may act on another user's
data is Q8. Protection-level checks (PROT-001–PROT-006) run in the service, never in the UI. They are M7 work,
and the contract must allow a "check required" result.

Unauthorized callers: a caller excluded by the DACL cannot connect and sees "access denied". A caller refused in
steps 2–4 receives `errorCode = Unauthorized` without data; the refusal is logged with rate limiting and the
connection is closed. How the UI presents these states is Q9.

Enforced sessions and rules:

- The owner of session S is the user SID in that session's token. It is cached per session ID and logon ID and
  refreshed on logon and logoff notifications, because session IDs are reused.
- A session ID that is not in the cache is never classified as "not enforced" by default. The service resolves
  it on demand, before classifying the process, with `WTSQuerySessionInformation` (state and user name) and
  `WTSQueryUserToken` ([WTSQueryUserToken]; this is a proposal, and assumption A20 covers the error behavior
  during logon). Reason: the process-start event for an auto-start application can arrive before the
  session-change notification for the same logon (the SM-002/Q5 window), and a cache filled only by
  notifications would let that process run. Outcomes:
  - the session has no logged-on user, or is session 0: not enforced (WIN-003);
  - an owner is found: the owner is cached and the event is evaluated with the owner's rules;
  - resolution fails for another reason, or the user is still logging on: the decision is retried for a short
    bounded time. If it stays unresolved, the process is not terminated, the failure is logged and the
    enforcement state is reported as `unavailable` (D9). Whether an unresolved owner should instead lead to
    termination is a product decision (fail open or fail closed), recorded in Q5 and not decided here.
- A process start in S is identified by the event's `SessionID` ([Win32_ProcessStartTrace]). It is evaluated with
  the owner's rules when the owner's partition has an active FocusManager session, whatever account the process
  runs as. This keeps the current session-based comparison (`AppEnforcer.cs:101`), which also covers processes
  started with `runas` or with another administrator's credentials in the user's session.
- Session 0 and sessions without a logged-on user are not enforced, as WIN-003 states for other sessions.
- With fast user switching or Remote Desktop, user A's rules apply in A's sessions and user B's rules in B's.
  Whether A's session must also restrict B is Q3.
- Site policies go to the owner's hive (D7); folder enforcement uses the helper in S (D3).

### D6. Service-owned storage

- The service owns `%ProgramData%\FocusManager\`, resolved as a known folder ([KNOWNFOLDERID]: `FOLDERID_ProgramData`,
  default `%ALLUSERSPROFILE%`; never from the `%ProgramData%` environment variable, see "Ancestors of the root"),
  with an explicit protected DACL: full access for LocalSystem and Administrators,
  no access for other users. The service creates the tree itself and never adopts a pre-existing object by
  resetting its DACL (rules R1–R8 below; assumption A12). The earlier wording "verifies the owner and resets the
  DACL if a user created the directory first" is withdrawn: see "Why resetting the root DACL is not enough".
- Each OS user SID has its own partition, `users\<SID>\focusmanager.db`, opened with the T002 store, whose path is
  already an argument; the schema does not change.
- The service never opens a file in a user profile as LocalSystem. If existing data must be carried over (Q11),
  a component running in the user's own context (the UI side; which component is decided in S3) reads the legacy
  `%LOCALAPPDATA%` database and submits it with the existing authenticated `SaveWhitelist` request. The helper is
  not involved, so it stays stateless, and the protocol gets no import message. `SaveWhitelist` validates the
  content like any other save. The alternative, an impersonated read by the service into a private copy that is
  opened defensively (assumption A14), is not proposed unless the UI-side import proves insufficient.
- Users can no longer edit rules or session state directly, which PROT-004 needs later.

#### Safe creation and opening of the data tree (T056 proposal)

This section is a proposal for review. It does not claim that a vulnerability exists in any implementation; no
service-owned storage exists yet. It defines a bounded strategy for a LocalSystem service that may start after
a standard user has already created directories or files under `%ProgramData%`.

Terms. *Trusted principals* are LocalSystem (`S-1-5-18`) and the built-in Administrators group
(`S-1-5-32-544`). Every other principal, including Users, Authenticated Users, INTERACTIVE, CREATOR OWNER and
each individual user SID, is untrusted for this tree. Administrators are inside the trust boundary: they can
already stop the service, and protection against them is M7 work. The *tree* is `root` =
`%ProgramData%\FocusManager`, `root\users`, `root\users\<SID>` and the database files in the partition.

**Why resetting the root DACL is not enough.** These are properties of the platform that a design must handle,
not findings about code:

1. An object's owner holds implicit `READ_CONTROL` and `WRITE_DAC` unless an OWNER RIGHTS ACE removes them. The
   source is the row `S-1-3-4` of [Security Identifiers (Windows Server)]: when an ACE with that SID is applied,
   "the system ignores the implicit READ_CONTROL and WRITE_DAC standard access rights for the object owner". The
   default is inferred from that sentence. A root pre-created by a standard user stays user-owned after its DACL
   is reset, so its owner can write the DACL again. Handles that the user opened before the reset are an
   additional concern that no fetched page states (not verified).
2. Descendants are separate objects. A user-created subdirectory, database file, hard link or sidecar keeps its
   own owner and DACL; resetting the root changes none of them. A hard link makes "more than one path reference a
   single file" with shared attributes ([Hard links and junctions]), and a database with several names has
   undefined behavior in SQLite because each name finds its own journal ([SQLite corruption], section 2.6).
3. A path component that is a reparse point is processed by the file system when a path is opened
   ([Reparse points]; junctions are implemented through them, [Hard links and junctions]). A LocalSystem write
   through a planted junction lands in its target, whatever the DACL of the planted directory says.
4. SQLite recovers from a hot journal or a WAL file at the next open and needs them kept with the database
   ([SQLite corruption], section 1.3; [SQLite WAL], sections 4 and 9), so the sidecar files are part of the
   database state. That a planted journal would be applied is inferred from those statements, not tested (A25).

**Rules.**

- **R1. Create, do not adopt.** Create the root, `users` and each partition with `CreateDirectoryW` and a
  `SECURITY_ATTRIBUTES` whose descriptor carries the protected DACL, so that the directory never exists with the
  DACL inherited from `%ProgramData%`. `CreateDirectoryW` applies the supplied descriptor to the new directory
  and returns `ERROR_ALREADY_EXISTS` when the directory exists ([CreateDirectoryW]); that return value is the
  pre-existence signal. Do not use `Directory.CreateDirectory` or
  `FileSystemAclExtensions.Create(DirectoryInfo, DirectorySecurity)`: the latter documents "If the directory
  already exists, nothing is done" ([FileSystemAclExtensions.Create]), which would adopt a pre-existing directory
  without applying any security. Illustrative descriptor (tests check the effective one):
  `D:P(A;OICI;FA;;;SY)(A;OICI;FA;;;BA)`: protected ([SECURITY_DESCRIPTOR_CONTROL], `SE_DACL_PROTECTED`), full
  control for LocalSystem and Administrators, inheritable, no other ACE. The owner is the creator's default owner;
  R2 accepts it only if trusted (A23). The managed .NET APIs do not expose these calls directly, so the
  implementation uses P/Invoke in `FocusManager.Platform.Windows` (A24). Shared code receives only the validated
  path, which keeps OS calls out of `src/Shared/*`.
  Volume precondition: the descriptor has an effect only if the target file system supports security on files and
  directories, which `GetVolumeInformation` reports as `FS_PERSISTENT_ACLS` ([CreateDirectoryW]). Before creating
  anything, the service queries the volume that holds the expected root and refuses (storage `unavailable`, same
  reporting as R2) if the flag is absent, so that a non-NTFS or redirected `ProgramData` cannot silently receive an
  unprotected tree (DT13b). Creation by the elevated install step: the tree is meant to be created, with the same
  code and rules, by the elevated install step (T052 scripts; the T041 installer later) and not first by the
  running service after boot, so that a refusal under R2 appears at install time and not at the first boot after
  it. This narrows the window; it does not remove the case of a root that a user created before the install, which
  R2 refuses and Q17 covers.
- **R2. Validate every pre-existing object; refuse, never repair.** The order is root, `users`, partition, then
  files, and a child is created or opened only after its parent passed R2 and is held (R4), so a create never runs
  through an unvalidated parent (a planted junction at `users` cannot receive a new partition). When `CreateDirectoryW` reports
  `ERROR_ALREADY_EXISTS`, or a file exists, open the object by handle with `FILE_FLAG_OPEN_REPARSE_POINT`, plus
  `FILE_FLAG_BACKUP_SEMANTICS` for a directory ([CreateFileW]), without enabling backup or restore privileges
  (R7). Adopt it only if all of the following hold:
  - a. It is not a reparse point (`FILE_ATTRIBUTE_REPARSE_POINT` clear in `dwFileAttributes`,
    [BY_HANDLE_FILE_INFORMATION]). Every reparse tag is refused: junction, symbolic link, mounted folder, cloud
    placeholder, unknown. There is no tag allow-list.
  - b. Its final path from `GetFinalPathNameByHandle` (normalized, DOS volume) equals the expected path,
    compared ordinal and case-insensitive ([GetFinalPathNameByHandle]: "the path that is returned when a path is
    fully resolved"). This is what detects a reparse point in an earlier component. The `CreateFile` page says the
    flag turns off "normal reparse point processing" and returns a handle to a symbolic link, but it does not say
    that earlier path components are exempt, so the strategy does not depend on the flag for them (A21). The page's
    example is a symbolic link; that a junction behaves the same is expected, and is tested (DT2, DT3).
  - c. The owner, read with `READ_CONTROL`, is a trusted principal.
  - d. The DACL is present and not NULL (a NULL DACL "grants full access to any user that requests it",
    [Null DACLs and empty DACLs]; the control-flag page says the same for a missing DACL,
    [SECURITY_DESCRIPTOR_CONTROL]) and equals the expected ACE set exactly. The directories and the main database
    file that the service creates must be protected (`SE_DACL_PROTECTED` "prevents the DACL ... from being
    modified by inheritable ACEs") and carry the R1 ACEs. A sidecar file that SQLite itself creates must carry
    only ACEs inherited from the partition, for LocalSystem and Administrators and nobody else (A26). An exact
    match is testable; an administrator-edited ACL is refused and handled under Q17. The comparison covers the
    DACL only; the SACL and any mandatory integrity label are neither compared nor changed.
  - e. For a file: it is a regular file and `nNumberOfLinks` is 1 ([BY_HANDLE_FILE_INFORMATION]).
  - f. For a directory: it contains only entries the service itself creates. Root: `users`. `users`: names
    that are canonical SID strings (R3). Partition: `focusmanager.db` and its SQLite sidecars
    `focusmanager.db-journal`, `-wal` and `-shm`. Any other entry refuses the directory. Each listed entry is
    validated by R2 in turn. Alternate data streams are not enumerated by directory listings and are not
    covered (not verified; no claim is made).
  - g. Object type. A directory object must have `FILE_ATTRIBUTE_DIRECTORY` set and a file object must have it
    clear ([BY_HANDLE_FILE_INFORMATION]). The `CreateDirectoryW` page documents `ERROR_ALREADY_EXISTS` for "the
    specified directory"; that a pre-existing regular file of that name gives the same result is expected, not
    verified. A file named `users` or like a partition must therefore be refused by this explicit check and not
    only incidentally by a failing enumeration (DT8b).
  Refusal means: the store is not opened, nothing is created inside the object, and its owner, DACL, name and
  contents are not changed, deleted or renamed. The service logs the failed sub-rule with rate limiting and
  reports storage `unavailable` (D9) for the affected partition, or for every partition when the root is
  refused. There is no fallback to a path outside the tree: a fallback would reintroduce H6. What happens next is
  Q17. Consequence to state plainly: a standard user who pre-creates `%ProgramData%\FocusManager` before the
  tree exists leaves the service without a rule store. Owner answer K1 Q5 requires active rules to apply
  immediately after a start or recovery, and an in-memory session does not survive a reboot, so after a reboot
  with an unavailable partition there is no enforcement for that user until the object is repaired. The elevated
  install step of R1 is what makes this a visible install-time failure instead of a boot-time one. This is
  recorded for T058 (reconciliation with K1 Q5) and Q17; it is not answered here.
- **R3. Partition names come from the service.** The SID is the authenticated caller's (D5, T051) or, in console
  mode, the process token's. It is parsed to a SID object and re-serialized, the directory name must equal that
  canonical string and match `S-1-<digits>(-<digits>)+`, and no client-supplied text is ever joined into a path.
  Traversal and alias forms (`..`, separators, `:`, trailing dot or space, short 8.3 names, other casing) are
  therefore rejected before any file-system call. Partitions are created by the service under R1 and are never
  granted to the user. A partition directory owned by anyone but a trusted principal, for example one created by
  a standard user, is refused by R2c even if its DACL looks correct. Under a root that already passed R2 a standard
  user cannot create such a partition at all, because the root's DACL grants them nothing (R1); the case is
  reachable only through a user-precreated root, which R2 refuses first, or through an administrator action. DT4
  therefore tests the control in isolation.
- **R4. Hold, then use.** After a tree object is created or adopted, the service keeps a directory handle to the
  root, `users` and each active partition for the process lifetime, opened without `FILE_SHARE_DELETE`. The
  `CreateFile` page says that without that flag no other open may request delete access, and that "Delete access
  allows both delete and rename operations", so the validated directories cannot be renamed or deleted while
  they are held. The page speaks of files and devices; its application to directories is A22. The database file
  is created by the service itself (`CREATE_NEW`, protected DACL), then closed, before SQLite first opens it,
  so SQLite never creates the main database file with a default descriptor; that SQLite accepts a zero-length
  file as a new database is assumed and tested (A25). Sidecar files are never pre-created.
- **R5. The check-to-open gap is closed by an invariant, with a detection step.** SQLite opens the database by
  name, and the application cannot hand it a validated handle (A24), so no application code can make
  "check then open" atomic. The gap is closed because every object on the path root, `users`, partition,
  database files has a trusted owner and the exact protected DACL (R1, R2c, R2d): an untrusted principal can
  neither create, rename, delete, link nor re-point anything inside the tree between check and open. This holds
  on the condition of A12, which is not verified: that `ProgramData` and `C:\` keep their default ACLs, so that
  a standard user cannot rename, delete or replace the `FocusManager` directory itself or an ancestor, and that no
  installer has loosened them. Under that condition swapping needs an administrator or SYSTEM, which is inside
  the trust boundary. The VM check records the actual ACL of `ProgramData` (and of `C:\`) before any DT result is
  relied on; the held handles of R4 cover the root and below, not the ancestors. As detection, not prevention: after the
  store reports ready, the service re-opens the database path under R2 and compares volume serial number and
  file index with the values taken before the open. A mismatch closes the store and reports `unavailable`.
  File IDs "are not guaranteed to be unique over time" ([BY_HANDLE_FILE_INFORMATION]), so a mismatch is
  conclusive but equality is not a proof.
- **R6. Re-validate on events, not on every query.** R2 runs at service start, whenever a partition is opened or
  reopened after an error, and after an unexpected storage exception.
- **R7. Privileges.** The data-tree code does not enable `SeBackupPrivilege`, `SeRestorePrivilege` or
  `SeTakeOwnershipPrivilege`, and the service never recursively deletes, takes ownership of or re-ACLs an object
  it did not create. `FILE_FLAG_BACKUP_SEMANTICS` is used only to obtain directory handles. The `CreateFile`
  page states that the system "overrides file security checks" when the process has `SE_BACKUP_NAME` and
  `SE_RESTORE_NAME` privileges, so for such a token an "access denied" result is not a validation signal; R2
  reads the owner and DACL explicitly instead. "Does not enable" means something only if the privileges start
  disabled in the service token; that is not verified (A30), and M14 records `whoami /priv` of the service
  process (state of each privilege, enabled or disabled). D2's privilege review should drop these privileges when
  M14 allows.
  Uninstall deletion of the tree (T041, Q13) follows the same rule: no path-based recursive delete that could
  cross a reparse point.
- **R8. Optional content defenses (T050 evaluates; no schema change).** The SQLite security page treats a
  database that "might have ever been writable by an agent in a different security domain" as suspect and lists
  `PRAGMA trusted_schema=OFF`, `SQLITE_DBCONFIG_DEFENSIVE` and a `quick_check` after open
  ([SQLite security]). They add depth only; none replaces R1–R5. Whether Microsoft.Data.Sqlite exposes them is
  not verified (A14, A24). They must not change the T002 storage tests. SQLite's temporary files are a further
  surface: their location is chosen by SQLite (for a LocalSystem process this is expected to be a system temporary
  directory, not verified) and cannot be placed inside the partition, because R2f allows no other entry there.
  Options for T050 to evaluate: `PRAGMA temp_store = MEMORY`, or a service-only temporary directory that is created
  and validated under the same rules (owner, exact DACL, no reparse point) outside the partition.

**Ancestors of the root.** The tree starts below `ProgramData`. The location is resolved with a known-folder call:
`SHGetKnownFolderPath(FOLDERID_ProgramData)` ([SHGetKnownFolderPath], [KNOWNFOLDERID]) or
`Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)`; the `%ProgramData%` environment
variable is never read, because the environment of a console-mode launcher can be influenced by the user
(the documentation of the second API was not read in this revision). The ancestors (`C:\` and `ProgramData` itself)
are not validated object by object. The expected path of the root is the final path of the opened `ProgramData`
directory plus `\FocusManager` (each descendant: its parent's expected path plus its own name), so a legitimate
redirection of the known folder does not fail R2b, while a reparse point at any level below it does. Ancestors are
treated as administrator-controlled by platform default; that standard users can create entries in `ProgramData`
but not rename or delete other users' entries is expected, not verified in a primary source (A12). No object in
the tree is accepted because its DACL was inherited from a parent: the root, `users` and each partition carry
their own protected DACL (R1) and a trusted owner (R2c), whatever the parent allows.

Console mode (T050) uses the same code in-process: the elevated agent creates or validates the same tree, with
the partition named for the process user's SID, and refuses a user-precreated root the same way.

**Adversarial checks (V13, T050).** Each row names the control it exercises and what a baseline that merely
resets the root ACL (create or open the root with `Directory.CreateDirectory`, set its DACL, open the database
path) would do. "Where": U = unit test of the decision logic with a faked file-security inspector; I =
integration on a real NTFS temp tree under an injected root, same account; VM = needs a second, standard-user
account (manual until automated). Tests use an injected root under a unique temp directory. Junction targets
are harness-created stand-in directories outside the tree, never real protected locations. No test touches the
real `%ProgramData%\FocusManager` or any user's data, and VM checks run on the VM only.

How to read the rows. The realistic attacker path is a root (or `users`) that a standard user created before the
service did: DT1 and DT1b. Under a root that passed R2, a standard user cannot create, rename or plant anything
(R1, R3, R5). DT3, DT3b, DT4, DT5 and DT7 therefore are control-isolation tests: they construct a tree as if an
administrator, or a process with the necessary rights, had modified a validated tree, or as if the root check had
failed, so that each control can be shown to work on its own. They do not describe a path open to a standard user
under a validated root, and a green result for them is not a claim that such a path exists.

Elevation. The integration rows (I) create trees whose DACL grants only LocalSystem and Administrators (R1, DT13).
A non-elevated shell, including an unelevated administrator whose Administrators SID is deny-only, cannot list,
write or clean up such a tree. The I rows therefore run elevated or in the VM, and the Result records the
elevation state of each run; an I row that ran non-elevated is reported "not verified", not "passed". Permitted
alternative: the trusted-principal set and the expected descriptor are constructor parameters of the data-tree
code, the I rows run with a set that includes the test account, and the production value is pinned by a separate
unit test of the descriptor builder (DT13 on the production descriptor still needs an elevated or VM run).
The Result records which of the two was used.

| # | Setup | Expected result | Baseline (reset root ACL only) | Where |
| --- | --- | --- | --- | --- |
| DT1 | Root pre-created by a standard user with a permissive DACL. Pair row: R2c and R2d both reject it | Root refused (R2c, R2d); owner, DACL and contents unchanged; nothing created inside; `unavailable` | Adopts and resets the DACL; the user, as owner, can write the DACL again | VM, U |
| DT1b | Root pre-created by a standard user with an untrusted owner but exactly the expected DACL (protected, SYSTEM and Administrators only). This is the load-bearing case: the DACL alone looks right, yet the owner keeps implicit `WRITE_DAC` (rule 1 above) and can reopen the root later | Root refused by R2c alone; R2d passes; owner, DACL and contents unchanged; nothing created inside; `unavailable` | Resetting the root DACL produces the same DACL and passes; the user still owns the root | U (fake inspector), VM |
| DT2 | Root pre-created as a junction to a stand-in "protected" directory | Refused (R2a, R2b); stand-in gets no file, no DACL or owner change | Creates through the junction and resets the target's DACL | I |
| DT3 | Control isolation. Valid root, but `users` or `users\<SID>` is itself a junction to a stand-in (junction under an existing descendant) | Refused (R2a; R2b may also fire, A21); nothing written to the stand-in | Root DACL reset does not touch it; the database lands in the stand-in | I |
| DT4 | Control isolation (R3, R2c). `users\<SID>` pre-created by a standard user and containing a planted `focusmanager.db`: (i) a valid database with a canary row, (ii) a database whose schema has a view and a trigger | Partition refused (R2c); the store never opens it (canary never read, file bytes unchanged) | Root DACL reset does not reach the partition; the planted database is opened | VM, U |
| DT5 | Control isolation. Trusted partition, but `focusmanager.db` is a hard link to a stand-in file on the same volume (a hard link cannot cross volumes or reference a directory, [Hard links and junctions]) | Refused (R2e); stand-in unchanged; no `-journal` or `-wal` appears beside it | Opens it | I |
| DT6 | `focusmanager.db` is a file symbolic link | Refused (R2a). If the test account cannot create symbolic links, record "not verified" instead of skipping silently | Follows the link | I |
| DT7 | Control isolation. Planted sidecars before the first open: `-journal` (hot-journal bytes), `-wal`, `-shm`, each as a regular file with an untrusted owner, as a hard link, as a symbolic link, and as a directory with that name; for both a new and an existing database. The WAL page describes `-shm` as a volatile index, so the effect of a planted `-shm` is not established; it is refused anyway | Refused (R2); database bytes unchanged; no recovery or rollback applied | SQLite recovers from the planted journal or WAL | I, VM |
| DT8 | Unexpected entry in root, `users` or a partition (for example `x.dll`, a non-SID name) | Refused (R2f) | Ignored | U, I |
| DT8b | Wrong object type: a regular file named `users`, a file named like a partition, and a directory named `focusmanager.db`, each in an otherwise valid tree | Refused by R2g with the object type as the reported sub-rule, not by a failing enumeration; nothing created, changed or deleted | Treats the existing name as present and fails later, or opens it | U, I |
| DT9 | Deterministic swap between check and open, by a hook in the opener: after R2 passes, the partition or database is replaced by a junction or hard link; separately, rename and delete attempts on root, `users` and the partition while handles are held | The swap is detected by the post-open recheck (R5) or the open fails; nothing is written to the stand-in. Rename and delete fail with a sharing violation; record A22 as verified, failed or not verified | No recheck, no held handles | I |
| DT10 | Stress: a thread flips the database path between a regular file and a hard link or junction while the opener runs N times (N chosen by T050 and recorded, for example at least 1,000) | Every run refuses or opens the validated object; none writes to the stand-in. This is a probe, not a proof | May write to the stand-in | I |
| DT11 | DACL drift on root or partition: an added Users ACE, a NULL DACL, an unprotected DACL (inherits from `%ProgramData%`) | Refused (R2d) | Reset silently accepts the drift | U, I |
| DT11b | Trusted owner (Administrators or SYSTEM) with a drifted DACL: an added Users ACE, or an unprotected DACL. Isolates R2d from R2c | Refused by R2d alone; R2c passes; nothing changed | Resets the DACL and adopts | U, I |
| DT12 | Partition names: `..`, `a\b`, `S-1-5-21-1-2-3-1001.`, lowercase `s-1-…`, an 8.3 alias, an over-long string | Rejected before any file-system call (R3) | n/a | U |
| DT13 | Positive controls: first start creates the tree with the exact descriptor (effective DACL inspected, owner trusted); a restart adopts it; two SIDs are isolated (V10) | Pass; T002 storage tests unchanged | n/a | I (elevated, see above) |
| DT13b | Volume precondition (R1): a fake volume query that does not report `FS_PERSISTENT_ACLS` | Nothing is created; the whole tree is `unavailable` with the volume as the reported cause | Creates the tree on a volume where the descriptor has no effect | U |
| DT14 | Sensitivity, per control (rule below the table) | (a) `ResetRootAclOnlyOpener` fails DT1, DT1b, DT2, DT3, DT3b, DT4, DT5, DT7. (b) Each control, disabled alone in a test build, makes its isolating row fail | n/a | U, I |

**Sensitivity rule (DT14).** The earlier requirement that disabling each control makes at least one row fail is withdrawn: it cannot hold for controls that detect the same setup (R2a and R2b both see a junction at the object; R2c and R2d both reject a permissive user-owned root). Each control instead has an isolating row, which a build with every other control enabled must fail only because of that control:

| Control disabled alone | Row that must then fail | Why that row isolates it |
| --- | --- | --- |
| R2a (not a reparse point) | DT6 (file symbolic link opened with the reparse flag, final path equal to the expected path); DT2 only if R2b does not already catch an object-level junction (A21) | the final path of the link itself matches, so R2b cannot see it |
| R2b (final path equals expected) | DT3b | the object is not a reparse point, only an ancestor is |
| R2c (trusted owner) | DT1b | the DACL is exactly right |
| R2d (exact DACL) | DT11b | the owner is trusted |
| R2e (one link, regular file) | DT5 | single control |
| R2f (only expected entries) | DT8 | single control |
| R2g (object type) | DT8b | single control |
| Sidecar validation | DT7 | single control |
| Post-open recheck (R5) | DT9 swap hook | single control |
| Volume precondition | DT13b | single control |
| R3 (partition names) | DT12 | single control |

DT3b setup: the validator is called directly on a descendant (for example a partition path) whose ancestor (`users`) is a junction to a stand-in, while the final component has no reparse attribute and the stand-in holds a matching directory, so that only the final-path comparison can detect it. A row that still fails when its control is disabled shows a redundant control: record the pair, and require the row to fail when both are disabled together. Record in the Result which row catches which control and which pairs were found redundant. A row that cannot be constructed on the test account is "not verified", not skipped silently.

#### Source status for D6

Pages read on 2026-11-18 (first T056 pass) and read again on 2026-11-19 (T056 continuation). Microsoft pages were
read as served by the documentation site; the SQLite pages through a summarizing fetch, so their wording is
paraphrased unless in quotation marks. "Not verified" marks an assumption or an inference, not a fact from a source.

| Claim used above | Source | Status |
| --- | --- | --- |
| `CreateDirectoryW` applies the supplied security descriptor to the new directory when the file system supports security, gives a default descriptor inherited from the parent when none is supplied, creates only the final directory of the path, and returns `ERROR_ALREADY_EXISTS` | [CreateDirectoryW] | verified |
| The supplied descriptor has an effect only if the target file system supports security on files and directories, "indicated when GetVolumeInformation returns FS_PERSISTENT_ACLS" (basis of the R1 volume precondition) | [CreateDirectoryW], re-read 2026-11-19 | verified for the statement. The `GetVolumeInformation` page itself was not read, and what the flag reports for a redirected `ProgramData` volume was not tested |
| A directory created with a descriptor never exists with an inherited DACL, not even briefly | inferred from the line above | not verified |
| `FileSystemAclExtensions.Create(DirectoryInfo, DirectorySecurity)`: "If the directory already exists, nothing is done" | [FileSystemAclExtensions.Create] | verified |
| `FILE_FLAG_OPEN_REPARSE_POINT`: "Normal reparse point processing will not occur"; for an existing symbolic link the returned handle is to the link, not the target. `FILE_FLAG_BACKUP_SEMANTICS` is required to obtain a directory handle and "appropriate security checks still apply" without the backup and restore privileges; with them the system overrides file security checks. `FILE_SHARE_DELETE`: "Delete access allows both delete and rename operations" | [CreateFileW] | verified |
| The reparse flag affects only the last path component; reparse points in earlier components are still followed | not stated on the page | not verified (A21); not relied on |
| A directory handle opened without `FILE_SHARE_DELETE` blocks rename and delete of that directory | the page speaks of "a file or device" | not verified (A22) |
| A final path is "the path that is returned when a path is fully resolved" (example: a symbolic link); `FILE_NAME_NORMALIZED` and `VOLUME_NAME_DOS` are the defaults | [GetFinalPathNameByHandle] | verified for symbolic links; the junction and mount-point cases are not stated, and are tested (DT2, DT3) |
| Reparse points implement NTFS links and mounted folders; opening a file that has one is processed by a file system filter; a reparse point can be set on a directory only if the directory is empty | [Reparse points] | verified. That a user therefore cannot turn the validated, non-empty partition into a junction without first emptying it is inferred and not tested |
| A junction is implemented through reparse points and may link directories on different local volumes; a hard link is more than one path to one file in the same volume, cannot reference a directory or another volume, and shares attributes with the other links | [Hard links and junctions] | verified |
| `nNumberOfLinks`; volume serial number plus file index identify a file; file IDs "are not guaranteed to be unique over time" | [BY_HANDLE_FILE_INFORMATION] | verified |
| Implicit owner rights `READ_CONTROL` and `WRITE_DAC` | [Security Identifiers (Windows Server)], row `S-1-3-4` | the quoted sentence is verified; the implicit default is inferred from it. The Win32 pages "Security Identifiers" and "Well-known SIDs" do not list `S-1-3-4` (checked 2026-11-19) and are not cited for it |
| A NULL DACL grants full access; an empty DACL grants none; `SE_DACL_PROTECTED` prevents modification by inheritable ACEs | [Null DACLs and empty DACLs], [SECURITY_DESCRIPTOR_CONTROL] | verified |
| System is a hidden member of Administrators | [Security Identifiers (Windows Server)], row `S-1-5-18` | verified |
| Default owner of an object created by a LocalSystem service, or by an elevated Administrators token | the page states the default owner only for files created by members of Domain Admins (Administrators group) | not verified (A23) |
| `CreateFile` without the reparse flag returns a handle to the target; `CreateHardLink` on a symbolic link links the target | [Symbolic link effects] | verified. The privilege needed to create symbolic links was not found on the page |
| `FOLDERID_ProgramData`, default path `%ALLUSERSPROFILE%` (`C:\ProgramData`) | [KNOWNFOLDERID] | verified |
| `Environment.GetFolderPath(CommonApplicationData)` returns the same location as the known folder | no page read in this revision | not verified; either API may be used only after T050 checks that they agree on the VM |
| Whether `SeBackupPrivilege` and `SeRestorePrivilege` are present and enabled in the token of a LocalSystem service | no page read | not verified (A30); M14 records `whoami /priv` |
| Location of SQLite temporary files for a LocalSystem process | no page read | not verified (R8) |
| Default ACL of `ProgramData`: who may create, rename or delete there | no primary page found; a search surfaced community posts with `icacls` output, which were not relied on | not verified (A12) |
| Journal files "must" be seen by SQLite for crash recovery and are named after the database with `-journal` or `-wal`; a database with several names (hard or soft links) has "undefined" behavior; the WAL "is part of the persistent state of the database and should be kept with the database"; the first new connection after a crash runs recovery; a database that "might have ever been writable by an agent in a different security domain" is suspect, with `trusted_schema=OFF`, `SQLITE_DBCONFIG_DEFENSIVE` and `quick_check` as optional defenses. The WAL page describes `-shm` as the wal-index backing file, deleted on the last disconnect, not as persistent state | [SQLite corruption] (1.3, 2.6), [SQLite WAL], [SQLite security] (1.1, 1.2) | verified from the extracts. That a planted journal or WAL would be applied is inferred and not tested (A25) |
| Access checks at open time versus handles opened before an ACL change | no page fetched | not verified |
| `NtCreateFile` with `OBJ_DONT_REPARSE`-style options | no page fetched | not verified; not used |

Limits of this strategy. It does not protect against an administrator or SYSTEM (M7). It does not cover alternate
data streams. It cannot make SQLite's own open-by-name atomic (R5). It is untested: no part of it has run.

### D7. Enforcement targeting and cleanup state

- Applications: the agent uses the event's `SessionID`, `Sid` and `ParentProcessID` ([Win32_ProcessStartTrace])
  instead of querying a process that may already have exited, and resolves enforced sessions through D5. A failed
  monitor start is retried with backoff and reported, instead of continuing silently (H9).
- Self-exemption: FocusManager's own processes (agent, helper, UI) and the WebView2 runtime processes of the UI
  are exempt by verified full path. This changes WIN-003, which lists file names (Q12). The minimal alternative,
  adding the new file names, keeps the rename bypass described in H8.
- Browser policies target the owner's hive. Preferred: the service writes while impersonating the owner and opens
  the hive through `RegOpenCurrentUser`, never through `Registry.CurrentUser`, which is cached per process
  ([RegOpenCurrentUser]). No privilege then crosses the boundary. This requires that users can write their own
  `Software\Policies` browser keys (assumption A5). Otherwise LocalSystem writes `HKEY_USERS\<SID>\...` and refuses
  user-created registry links (assumption A6). Either way, the hive must be loaded, which means the owner is logged on.
- Policy ledger: a durable record in service storage of what was written (SID, browser key, values, state). S1
  (T048) defines the ledger as a port with the reconciliation logic and tests it against a fake durable store (V5);
  the durable SQLite implementation, a new table in the user's partition, is delivered by S3 (T050). Until then
  the console agent has no durable ledger. To apply,
  the service records the intent, writes the registry, then marks the entry applied. To clear, it deletes the values,
  then removes the record. Reconciliation runs at service start, at logon, and at session start and end. `HKLM`
  policies for the strict level remain M7 work (out of scope for T013; T034).

### D8. Lifecycle and failure handling

| Event | Proposed handling |
| --- | --- |
| Service start (boot or recovery) | Open storage and partitions. Reconcile the ledger for loaded hives. Start process monitoring, with retry, if any partition has an active session. Enable session-change notifications, then enumerate existing sessions ([WTSEnumerateSessions]) and launch the helpers that are needed. Open the UI pipe. Report running. The SCM delivers a logon notification only if the service is fully loaded before the logon ([HandlerEx]), so enumeration is mandatory and duplicate handling is idempotent. The control handler returns at once and work runs on another thread ([HandlerEx]). |
| Logon of user U in session S ([WM_WTSSESSION_CHANGE]) | Refresh the owner of S (it may already be cached by an on-demand resolution, D5; the refresh is idempotent). Reconcile U's policies immediately, because browsers may start automatically (assumption A7). Launch the helper if U has an active session. Process events that arrive before this notification are handled by on-demand resolution, not ignored. |
| Lock or unlock | No change. |
| Console or remote disconnect and reconnect | Processes in a disconnected session keep running, so enforcement and the helper continue; nothing is recreated. UI clients reconnect on demand. |
| Fast user switching | Each session is enforced with its owner's state (D5). |
| Logoff | Mark S as ending so that no helper is relaunched, then drop the cached owner. Policies stay in U's hive while U's FocusManager session is active, so they apply again at the next logon. |
| UI start, exit or restart | Enforcement does not need the UI. After any IPC failure the UI retries with backoff, shows "unknown" (SM-007) and refreshes all state after reconnecting. |
| Helper crash or termination | Relaunch with backoff, report degraded folder enforcement, log. |
| Service crash | The SCM restarts the service (start path above). Policies written before the crash remain and are reconciled. Old helpers exit when their pipe breaks and new ones are launched. The snapshot is recaptured, as today (Q5). |
| Service stop (administrator, update) | Stop accepting IPC, close helper pipes so that helpers exit, stop monitoring, flush storage, keep the policies of active sessions, report stopped with exit code 0. Keeping policies matches the current `StopAsync` (`AgentHostedService.cs:96-125`). It also means that site restrictions can outlast the stopped service while application and folder blocking end, which is Q16 (b). |
| Shutdown or reboot during a session | Same as a stop, within the shutdown limit of about 20 seconds ([HandlerEx]), without preshutdown handling. At the next boot SM-002 restores the session before any logon. |
| FocusManager session start | Persist the intent, take the snapshot, start monitoring, apply policies for logged-on owner sessions through the ledger, launch helpers. This keeps the current order (`AgentHostedService.cs:238`, `:252-295`). |
| FocusManager session end | Persist the intent, stop the process monitor when no session needs it, end the owner's helper (it exits when its pipe is closed, D3), clear policies through the ledger. If the owner's hive is not loaded, the entry stays pending and is cleared at the next logon. |
| Fatal error | Log, then exit with a non-zero code so that SCM recovery runs. |
| Uninstall | An elevated cleanup command of the agent clears every ledger entry: loaded hives directly, unloaded hives either loaded and cleaned or reported (method for T041). Then the service is stopped and deleted and its files are removed. Whether user data is deleted is Q13. |

Ordering constraints:

1. Storage and the ledger are reconciled before the UI pipe accepts requests.
2. Session-change notifications are enabled before existing sessions are enumerated; duplicates are harmless.
3. A session change is persisted before it is applied, so reconciliation after a crash converges.
4. At logon, policies are reconciled before the helper is launched.
5. The snapshot is taken before monitoring starts (current order; the gap between them is an existing limitation).
6. A helper never outlives its service connection.
7. Cleanup never depends on the UI, and stopping the service does not end a FocusManager session.
8. (Proposal.) The owner of a session is resolved on demand before a process event from that session is
   classified (D5). A process event may therefore be evaluated before the logon notification for its session,
   and the later notification only refreshes the cached owner. Without this rule, a session with an unknown
   owner would be "not enforced" and fail open during the SM-002/Q5 window.

### D9. Contract additions and ownership

| Change | Location | Delivered by | Needed before |
| --- | --- | --- | --- |
| Envelope `protocolVersion` (absent means 1) with compatibility rules. Additive optional fields are compatible; removals or semantic changes raise the major version. Unknown fields are ignored, and unknown commands return `UnknownCommand`. | `contracts/` JSON Schema, `contracts/README.md` | **T047** (new; depends on T005). T005 is `ready` and excludes format changes, so it is not extended. | T017, T051 |
| `GetAgentInfo`: agent version, supported protocol range, platform, capabilities | same | Schema and types: T047. Server implementation: T051 (S4). | T017 (schema); T051 |
| `errorCode`: `InvalidRequest`, `UnknownCommand`, `UnsupportedProtocolVersion`, `Unauthorized`, `Unavailable`, `Internal`; the `error` text is sanitized | same | Schema and types: T047. Sanitizing server code: T051. | T017, T051 |
| Status: whether the caller's session is enforced; the state of application, folder and site enforcement (`active`, `inactive`, `unavailable`, `helperUnavailable`) | same | Schema: T047, after the owner answers Q16 (vocabulary). Values produced by T048–T052. | T017 indicator, T018 |
| Reserved framing for events: `kind` (response or event) and `requestId` | same | T047 (schema only) | T014 |
| Subscription and events (`SessionStateChanged`, `EnforcementStatusChanged`, `Blocked`) | same | T014 | T014 UI |
| Helper protocol, versioned, C# only, because both ends ship together | `src/Windows/FocusManager.Contracts` (or `contracts/windows-helper/` if the owner wants schema-first) | T049 (S2) | T052 (S5) |
| Transport limits: maximum message size, deadlines, connection model | `contracts/README.md` | T047 (documents); T051 enforces | all consumers |

Compatibility checks (T047, building on the T005 validation test): schema validation of agent output, rejection of an unsupported major version,
tolerance of unknown fields in both directions, and the WinUI client, which sends no version, until T018.

### D10. Tray ownership in console and service mode (T056 proposal)

Until T016 removes it, the agent keeps its WinForms tray in console mode. The service host (T052, S5) must not
start that tray in session 0 (H3; [Interactive Services]: a LocalSystem service should not create a window on the
interactive desktop). Console mode is otherwise unchanged; the one deliberate difference is a console agent in
session 0, which gets no tray (table below). This section states how that is done and checked. It does not change the final owner of the
tray (the Tauri UI, T017), the meaning of tray Exit (owner answer K1 Q4: closing or hiding the UI leaves
enforcement running; T016 applies it) or the single-icon acceptance (M13, T016).

**Trace of the current startup** (working tree checked 2026-11-19; uncommitted T001/T002 edits may move lines):

| Step | Source | Effect |
| --- | --- | --- |
| Host build | `Program.cs:14-19`, `:58-59` | Generic host and `host.Run()`. There is no `AddWindowsService` and no notion of a host mode. |
| Registration | `Program.cs:53`; `:33` | `TrayHost` is a singleton in every run. `SilentNotifier` is the registered `INotifier`; `TrayNotifier` is not registered anywhere. |
| Construction | `AgentHostedService.cs:42-69` | The hosted service depends on the concrete `TrayHost` and subscribes to its `StudyModeToggleRequested` and `ExitRequested` events (`:67-68`). |
| Start | `AgentHostedService.cs:71-94`, call at `:76` | `StartAsync` calls `_trayHost.Start(...)` unconditionally, after reading the persisted study-mode flag (`:75`), before enabling study mode (`:78-88`) and before the IPC loop starts (`:92-93`). |
| Message loop | `TrayHost.cs:22-43`, `:124-166` | `Start` creates a background STA thread named `FocusManager.TrayHost` (`:34-41`). That thread creates a WinForms synchronization context (`:128-132`), an `ApplicationContext` (`:134`), a context menu, a `NotifyIcon` (`:148-154`) and runs `Application.Run` (`:166`). The loop is not on the host's main thread: `StartAsync` starts it, so any host that calls `StartAsync` starts it. |
| Later coupling | `AgentHostedService.cs:90`, `:249`, `:393` (`SetStudyModeState`); `:110` (`Stop`); `:384-395` (tray toggle); `:397-400` (tray Exit calls `StopApplication`) | Tray state follows study mode, and a tray command can stop the host. |
| Project | `FocusManager.Agent.csproj:4` | `UseWindowsForms` is true. The agent is a Worker SDK project. |

Consequence for a service host, from reading the source and not from a run: if S5 added `AddWindowsService` and
nothing else, `StartAsync` would run under the SCM and start the tray thread in session 0. What WinForms and
`NotifyIcon` do in a non-interactive session was not observed (A27). The design does not depend on that: it keeps
the call from happening.

**Proposal.**

1. One explicit host mode, decided once. `Program.cs` computes `AgentHostMode` (`Console` or `Service`) before it
   registers services. The decision is a pure function of two inputs, so it can be tested without a service:
   `WindowsServiceHelpers.IsWindowsService()` ([IsWindowsService]) and the process session ID (already read at
   `AppEnforcer.cs:12`).

   | `IsWindowsService` | Session ID | Mode | Tray |
   | --- | --- | --- | --- |
   | true | any | `Service` | none |
   | false | 0 | `Console` | none; a warning is logged, because a console agent in session 0 is the H1 situation and has no visible desktop |
   | false | 1 or higher | `Console` | `TrayHost`, as today |

   `Environment.UserInteractive` is `false` for a process that runs without a user interface ([UserInteractive]),
   but it is not an input: the rule rests on the two inputs of the table, and T052 may log the property.
   `AddWindowsService` is documented as "context aware" and activates only when the process runs as a Windows
   service ([AddWindowsService]), so T052 can call it in both modes. The explicit mode, not that call, decides the
   tray, so that the tray stays absent even if detection and registration ever disagree (A28).
2. A seam instead of a concrete class. `AgentHostedService` depends on a small interface (working name
   `IAgentTray`: `Start(bool)`, `Stop()`, `SetStudyModeState(bool)` and the two events). `TrayHost` implements it
   without a change of behavior. `NullAgentTray` (no thread, no window, never raises an event) implements it for
   service mode. `Program.cs`, through a registration helper that tests can call, registers exactly one of them:
   `TrayHost` in `Console` mode with the tray enabled, `NullAgentTray` otherwise. In service mode `TrayHost` is
   never registered or constructed and no WinForms type is touched (A29). This is a behavior-preserving refactor of
   console mode in sessions other than 0 (AGENTS.md): the order of calls in `StartAsync` and `StopAsync` stays as
   traced above. The console-only registrations (`TrayHost` and anything else that names a WinForms type) live in
   a separate method marked `[MethodImpl(MethodImplOptions.NoInlining)]`, called only from the `Console` branch.
   Reason: if `AddSingleton<TrayHost>()` stayed in the body of the method that also runs the service path, the
   runtime could load `TrayHost`, and with it `System.Windows.Forms`, when it compiles that method, even though the
   branch never executes. Which of these happens is not observed (A29). The evidence for "no
   `System.Windows.Forms` module loaded in the service process" is M15 (the module list of the service process),
   not V14: the test process may load the assembly for other reasons.
3. No other path to the tray. `TrayNotifier` stays unregistered and `SilentNotifier` stays the `INotifier`; showing
   blocking notifications while the UI is closed (K1 Q6) is a separate path that T014 and T016 own. T052 adds no
   code that constructs `TrayHost` in service mode. As a required second layer, independent of the registration
   code, `TrayHost.Start` refuses to run in session 0 (session ID through an injectable provider; it does nothing
   and logs), so that a future registration of `TrayNotifier` or of `TrayHost` by T014 or by a test host cannot
   start a message loop there. T052 delivers this with a unit test (V14).
4. Service-mode behavior. Study mode changes only through IPC, there is no tray Exit command, and the host stops
   only through the SCM (D8).
5. Coexistence. A console agent and the service must not both enforce. The first-instance pipe option of D4
   (delivered by T051, on which T052 depends) makes a second pipe server fail to start; T052 defines that the
   console agent then exits with a non-zero code and that the scripts report an installed service (H11). The
   reverse cases are service-side: the service starts while a console agent holds the pipe name, or a standard user
   process holds it before the service starts (squatting). Proposed behavior: the service does not serve on a
   non-first instance and does not stop enforcing because of it, since enforcement does not depend on the IPC
   endpoint (architecture rule 5); it logs the cause with rate limiting, retries creating the first instance with
   backoff, and has no client-visible state until it succeeds. Whether a persistent failure should instead end the
   process with a non-zero code (D8, fatal error) is a detail for T051/T052 to settle with the D4 pipe
   descriptor; it needs no owner decision unless it changes what a user sees. The client-side owner check of D4
   protects clients only.
6. What T016 still owns: removing `TrayHost`, `TrayNotifier`, `UseWindowsForms` and, if it has no further purpose,
   the interface and `NullAgentTray`; the handoff of the tray to the Tauri UI; the single-icon acceptance (M13);
   applying the Exit semantics. Two transitional states follow and are deliberate: between T052 and T016 a
   service-mode agent has no tray (the WinUI app or the Tauri UI is the control surface), and between T017 and T016
   a console-mode agent can show its own icon beside the Tauri one. T016 also removes the seam and `NullAgentTray`
   if nothing else uses them, and re-runs M15 and M16 on the final build (T016 scope).
7. Test seams (T052 scope). `AgentHostedService` today takes sealed concrete classes (`ProcessStartMonitor`,
   `ExplorerMonitor`, `AppEnforcer`, `FolderEnforcer`, `SiteEnforcer`, `SessionSnapshotService`;
   `AgentHostedService.cs:42-52`), and `StartAsync` has side effects that a test must never cause: with the
   persisted flag false it calls `SiteEnforcer.ClearAsync` (`AgentHostedService.cs:87`, `SiteEnforcer.cs:31-34`),
   which deletes the Chrome policy keys under `Registry.CurrentUser` (`ChromePolicyRegistry.cs:71`), and it always
   starts `RunIpcLoopAsync` (`AgentHostedService.cs:93`) on the fixed pipe name `FocusManager.Agent.Pipe`
   (`IpcProtocol.cs:6`). Today's pipe has no first-instance option (H4), so a test host would register a second
   server instance beside a live agent of the owner and could answer that agent's UI requests. V14 and V15
   therefore need seams, which T052 adds as behavior-preserving changes before it writes those tests: interfaces
   for the monitors and enforcers used by `AgentHostedService` (or an injectable composition that supplies them);
   an injectable pipe name, or an IPC-loop factory that tests replace with a no-op; an injectable registry root
   and policy path for `ChromePolicyRegistry` (T048's injected registry root for V5 is the same seam, reused if
   T048 delivered it, otherwise added here); and the tray interface of item 2. Rule: no V14 or V15 test starts the
   real IPC loop, the WMI watcher or `ExplorerMonitor`, touches `HKCU`, or opens the real pipe name; a test that
   needs any of these is an M check on a VM. V15 is a characterization test. Because the current constructor takes
   concrete classes it cannot be compiled against the code as it is now, so the order is: (1) extract the
   interfaces mechanically with no other change; (2) write V15 against that unchanged behavior with fakes and see
   it pass; (3) only then add host mode, `AddWindowsService` and the registration helper, keeping V15 green. No
   source file in `tests/` references `AgentHostedService`, `TrayHost`, `AddHostedService` or `Host.Create`
   (searched 2026-11-19), so "existing Agent tests pass unchanged" is not evidence that console mode is
   preserved; V15 and M16 are.

**Checks** (assigned in T052): V14 for the decision table and the composition; V15 for the order of calls and the
events in console mode; M15 for a service session; M16 for console mode. Limits: no run has observed WinForms in
session 0; a composition test shows what is registered, not what Windows does and not whether
`System.Windows.Forms` is loaded in the service process (M15 shows that); the window probe of M15 is manual
and needs a positive control (it must find the tray's windows in console mode before its empty result in service
mode means anything).

## Alternatives

### Service account

| Option | Assessment |
| --- | --- |
| LocalSystem (proposed) | Only option with `SE_TCB_NAME` for `WTSQueryUserToken`; has every local right needed. Largest blast radius, reduced by D2, D4 and D6. |
| LocalService or NetworkService | Not administrators: cannot terminate other users' processes, cannot write other users' or machine policies, and (per WIN-001) cannot use the WMI trace. No `SE_TCB_NAME`, so no helper launch (assumption A19). Rejected. |
| Virtual account `NT SERVICE\FocusManager` | Same rights as LocalService plus a service SID. Would need administrator membership and still lacks `SE_TCB_NAME` (assumption A19). Rejected. |
| Dedicated local account | Password management, an account visible to the user, no `SE_TCB_NAME` by default (assumption A19). Rejected. |
| Elevated logon task instead of a service | Does not meet WIN-010; any user who can elevate can end it. Rejected. |
| Privileged core plus an unprivileged front service | Smaller IPC attack surface, but an extra process and hop. Deferred to the M7 threat review. |

### Helper placement

| Option | Assessment |
| --- | --- |
| Separate helper started by the service with `CreateProcessAsUser` (proposed) | Documented pattern ([Interactive Services]). The service controls start, stop, relaunch and a per-session channel. Requires LocalSystem and one more project. |
| Explorer COM inside the Tauri UI process | No extra process, but folder blocking would stop whenever the UI is closed, contradicting architecture rule 5 and ADR-0007. `ExplorerInterop` and `FolderEnforcer` would be rewritten in Rust, with a risk of behavior drift. Rejected. |
| Helper started from `HKEY_LOCAL_MACHINE\...\Run` | Documented for multi-user systems ([Interactive Services]) and needs no `SE_TCB_NAME`. Starts only at logon, cannot be relaunched by the service or started mid-session, and users may be able to disable startup entries (assumption A17). Fallback if `CreateProcessAsUser` proves unusable, for example because of packaging. |
| Scheduled task at logon | Can restart on failure, but adds persistent configuration and still cannot be started by the service on demand. Rejected. |
| Cross-session COM from the service | Undocumented for shell windows; a LocalSystem process calling objects owned by a user session is an elevation-of-privilege surface. Rejected. |
| File-system minifilter driver | Behaves differently from FLD-005 (navigate back) and brings driver signing and kernel risk. Rejected for M2; M7 may revisit it. |

### Helper role

| Option | Assessment |
| --- | --- |
| Thin sensor and actuator (proposed) | Rules, decisions, suppression and notifications stay in one privileged place; the helper holds no state. Costs more local round trips per blocked folder (assumed negligible, assumption A18). |
| Local enforcer with rules pushed by the service | Fewer round trips, but rule evaluation and suppression would be duplicated in a process the user controls, and status would be split. Rejected. |

### IPC endpoint and caller identity

| Option | Assessment |
| --- | --- |
| One well-known pipe with token-based authorization (proposed) | Stable name for T017 and the WinUI client. Separation is enforced by service code from the caller's token; requests never carry identity. |
| Per-session UI pipes with a logon-SID DACL | The operating system enforces separation ([Named pipe security], [Interactive Services]). Needs dynamic pipe creation; the client derives the name from its session ID. Used for the helper; a possible later hardening of the UI pipe. |
| `PipeOptions.CurrentUserOnly` | Requires the same user and elevation level at both ends ([PipeOptions]), which is impossible between LocalSystem and a user. Rejected. |
| RPC, ALPC or a COM server | Richer security callbacks, but a poor fit for Rust/TypeScript and for Linux parity (ADR-0007 uses a named pipe and a Unix socket). Rejected. |
| Localhost TCP or HTTP | No operating-system caller identity. Rejected. |

### Storage location

| Option | Assessment |
| --- | --- |
| Service-owned per-user partitions under `%ProgramData%` (proposed) | Cannot be edited by the user, is available while users are logged off, and needs no LocalSystem access to user files. Needs a migration decision (Q11). |
| User profile path (the T002 default) | Resolvable only with the user's token and a loaded hive ([SHGetKnownFolderPath]). Editable by the user, and LocalSystem would operate on files in a user-controlled directory. Rejected for service mode. |
| One shared database for all users | Simpler, but mixes users' data and decides multi-user behavior (Q2). Not proposed. |
| Data held by the UI | Contradicts architecture rule 5. Rejected. |

### Session ownership rule

| Option | Assessment |
| --- | --- |
| Rules of the session owner (proposed) | Keeps WIN-003 and the session-based check at `AppEnforcer.cs:101`, including `runas` and processes started with other credentials. |
| Rules of the process's user | A process started as another account would escape the rules. Rejected. |
| Console session only (`WTSGetActiveConsoleSessionId`) | Wrong with fast user switching and Remote Desktop. Rejected. |
| Every session on the machine | A product decision (Q3). Not proposed. |

### Site-policy target

| Option | Assessment |
| --- | --- |
| The owner's hive (proposed) | Matches WIN-005 and decision I5 for non-strict levels. The hive is available only while the owner is logged on; the ledger handles the rest. |
| `HKLM` at every level | Changes behavior: the policy applies to every OS user and a standard user cannot remove it, which I5 reserves for the strict level. Not proposed; M7. |

## Consequences

- Easier: standard users can use FocusManager without elevation. Enforcement does not depend on the UI, and there
  is one privileged decision point. Users' data is isolated per OS user and cannot be edited directly, which
  PROT-004 needs. The UI can show precise failure states.
- Harder: a new project and protocol. Native interop for sessions, tokens, `CreateProcessAsUser` and security
  descriptors in `FocusManager.Platform.Windows`. LocalSystem code is a high-value target and needs the
  independent review that the orchestration policy recommends for privileged IPC. Development needs an elevated
  install step, and many checks are manual on real Windows sessions; a virtual machine is recommended.
- Specification updates before implementation, each made by the step that changes the behavior:
  - WIN-001: source of privilege.
  - WIN-002: principals.
  - WIN-003: definition of "other sessions" and the exemption list.
  - WIN-006: storage location note.
  - WIN-007: tray ownership and Exit.
  - WIN-008: script behavior.
  - WIN-010: status and its existing question.
  - SM-007: UI states, if Q9 or Q16 ask for them.
- `docs/architecture/README.md` is not changed by this proposal and gets no ADR-0009 entry until the ADR is
  accepted. After acceptance, S2 (T049) adds `FocusManager.SessionHelper`, which references
  `FocusManager.Platform.Windows` and `FocusManager.Contracts`, and states that folder blocking on Windows needs
  the helper in the user's session.
- Proposed implementation sequence. Each step keeps the agent runnable, so S1–S4 can be verified in console mode
  with `Start-FocusManager.cmd` before S5 changes the host. The steps are planned as tasks T047–T052 and T016
  (all `planned`, none ready until this ADR is accepted):
  - D9 contract: **T047** (schema, rules, types; before T017 and S4).
  - S1, **T048**: session-aware targeting (D5 ownership and on-demand owner resolution; D7 application filter,
    policy target and ledger port, monitor retry, self-exemption).
  - S2, **T049**: session helper (new project, helper protocol, session-routed navigator, console-mode launch).
  - S3, **T050**: service-owned storage (D6, including the safe creation and opening of the data tree, the answer
    to Q11 and the durable ledger implementation).
  - S4, **T051**: IPC hardening and authorization (D4 server side, D5, error codes, `GetAgentInfo`).
  - S5, **T052**: service host (D2, D3 launch and supervision, D8, scripts, recovery, privilege list,
    client-side server verification, and the host mode that keeps the console tray out of session 0, D10).
  - S6, **T016**: tray handoff (remove WinForms and `TrayHost` after T017 provides the Tauri tray; needs Q4).
    T016 keeps S6 as its own bounded scope and is the end-to-end acceptance gate for the whole chain, so S1–S5
    plus the D9 contract are the six child tasks.
- Ordering and overlap (a proposal; the main agent integrates it into `ROADMAP.md`). "Depends" means accepted
  first (TASK_FORMAT). "Sequential" means no dependency edge, but the declared `area` patterns overlap, so the
  tasks must not be edited in parallel.

  | Tasks | Relationship | Overlapping area | Resolution |
  | --- | --- | --- | --- |
  | T005 → T047 | T047 depends on T005 | `contracts/**`, `src/Windows/FocusManager.Contracts/**` | T047 builds on T005's schema and generation command. T005 is not changed. |
  | T047 → T017 | T017 depends on T047 (planning change recorded in T017) | none (`src/Desktop/**` vs `contracts/**`) | T017 consumes `protocolVersion`, `GetAgentInfo`, `errorCode`. Until T051, T017 verifies these against a schema-conformant fake; the real-agent check is in T016's gate. |
  | T047 → T051 | T051 depends on T047 | Contracts project | Server code follows the schema. |
  | T003 → T048 | T048 depends on T003 | `src/Windows/FocusManager.Agent/**`, `src/Shared/FocusManager.Core/**` | S1 builds on the shared controller and ports. |
  | T013 → T048 | T048 depends on T013 | `ChromePolicyRegistry`, `SiteEnforcer`, `src/Windows/FocusManager.Platform.Windows/**` | T013 generalizes the browser list first; S1 then retargets the hive and records the ledger for every listed browser. Reversing the order would need the ledger to take a browser-key abstraction first. Proposal; T013 is not edited by this planning. |
  | T003, T005, T013 (no edge between them) | Sequential | `AgentHostedService.cs`, `src/Windows/FocusManager.Agent/**`, `tests/Windows/**` | Run one at a time. T005 is the only one ready now (T003 waits for T001, T013 for T012). |
  | T048, T049, T050, T051, T052 | Sequential in step order | `Agent/**`, `Platform.Windows/**`, `tests/Windows/**` | One at a time in the order S1, S2, S3, S4, S5. Real dependency edges: T049←T048, T050←T002 and T048, T051←T047, T048 and T050, T052←T049, T050 and T051. S2 and S3 have no edge between them but stay sequential. |
  | T002 → T050 | T050 depends on T002 | `src/Shared/FocusManager.Persistence.Sqlite/**`, `Program.cs` | T002 is in `review`, so T050 stays `planned` until T002 is accepted or continuation is explicitly authorized. |
  | T017 and T048–T050 | May overlap in time | none | Different trees. T017 does not need the service. |
  | T052 → T016 | T016 depends on T052 (and on T047–T051, T017, T003) | `Agent/Program.cs`, `FocusManager.Agent.csproj`, `AgentHostedService.cs`, `tests/Windows/FocusManager.Agent.Tests/**` | T016 removes the tray after the service host exists. |
  | T016 → T014 | T014 depends on T016 and T017 (existing) | `contracts/**`, `Agent/**`, `src/Desktop/**` | T014 adds subscription and events on the T047 framing. |
  | T016 → T018 | T018 depends on T016 (existing) | `scripts/windows/**`, `Start-FocusManager.cmd`, `docs/spec/platforms/windows.md` | Resolved by dependency ancestry (T018 → T016 → T052). T018 is not edited by this planning. |
  | T052 → T041 | T041 comes later (M12) | `scripts/windows/**` (installer, uninstall) | T041 plans packaging after the dev scripts of T052 exist. |
  | T034 | Later (M7) | none now | Owns protection of the service and helper (Q10, PROT-001–PROT-008). |

  The existing dependency graph stays acyclic: T047→{T005, T027}; T048→{T003, T013, T027}; T049→{T048, T027};
  T050→{T002, T048, T027}; T051→{T047, T048, T050, T027}; T052→{T049, T050, T051, T027};
  T017→{T005, T047}; T016→{T027, T003, T017, T047–T052}; T014→{T016, T017}; T018→{T017, T016, T012, T022}.
- Other tasks:
  - T005 is not extended: it is `ready` and excludes format changes, and D9 depends on this ADR. T047 delivers D9.
  - T003 keeps the session controller instantiable per OS-user partition, with site and folder ports keyed by
    their target (note added in T003).
  - T017 depends on T047; it adds server verification and IPC from the Rust core only; its in-session checks
    depend on the H8 exemption.
  - T014 depends on the event framing from T047 and delivers the subscription and events.
  - T013 shares `ChromePolicyRegistry` and `SiteEnforcer` with S1 and precedes it (table above).
  - T041 owns the installer, uninstall cleanup and Store packaging.
  - T034 owns protection of the service and helper, `HKLM` policies and cross-account rules.
- Store: an MSIX package with services requires Windows 10 version 2004 and a restricted capability
  ([MSIX services]), while WIN-009 names version 1809 (Q14).
- Linux (T020): the authorization semantics carry over (partition by OS user, no client-supplied identity); the
  mechanism is Linux's own and is not verified here.

## Open Questions for the Owner

| # | Requirement | Question |
| --- | --- | --- |
| Q1 | WIN-010 (existing question) | Is the service always installed, or only for the strict level? This ADR follows the existing proposal "always". "Strict only" would need an unprivileged host for the other levels and a revision of this ADR. |
| Q2 | Specification silent (WIN-002, WIN-003) | Which OS users on a shared computer may use FocusManager, and does each have separate settings, profiles and sessions? The design assumes any interactive user with their own partition. |
| Q3 | Specification silent (WIN-003, PROT-004) | Does user A's active session restrict other OS users' sessions (fast user switching, a second account), at least at the strict level? |
| Q4 | WIN-007 | In the service model, what does tray Exit do: close only the UI while enforcement continues, end the session after the PROT-001 check, or stop enforcement? |
| Q5 | SM-002, SM-003 | After a reboot or an agent restart during a session, are processes already running when the agent restarts exempt (current behavior), and are applications started automatically at logon blocked like any new launch? If the owner of a session cannot be determined at the moment a process starts (D5, ordering constraint 8), should that process be left running with a reported state (proposal) or terminated? |
| Q6 | NTF-001 (silent on UI absence) | Must blocking notifications appear when the Tauri UI is not running? |
| Q7 | Specification silent | Does the UI start automatically at logon, and should anything restart it after it is closed? |
| Q8 | WIN-002 | May an administrator view or change another user's FocusManager data or end their session? WIN-002 currently names administrators as allowed principals. |
| Q9 | SM-007 | Should the UI distinguish "not authorized" and "incompatible version" from "unknown"? |
| Q10 | PROT-001, PROT-008 (silent) | Is termination of the helper only a degraded state (relaunch and status), or a bypass that triggers further measures at some protection levels? |
| Q11 | WIN-006, T002 (silent) | Must existing local data from the console agent be imported into the service's storage, or may development data be recreated? If it must be imported, the proposal is an import by a component in the user's context through the existing `SaveWhitelist` request (D6). |
| Q12 | WIN-003 | May FocusManager's own processes be exempted by verified install path instead of file name? This closes the rename bypass and changes the WIN-003 text. |
| Q13 | Specification silent | Is local data deleted or kept on uninstall? |
| Q14 | GEN-006, WIN-009 | For Microsoft Store, is a higher minimum Windows version acceptable for a packaged service, or should the Store listing use an unpackaged installer (T041)? |
| Q15 | WIN-005, decision I5 | Strict-level `HKLM` policies apply to every OS user of the computer. Is that acceptable (T034)? |
| Q16 | SM-007 (extends Q9) | (a) Which enforcement states may the UI show beyond "unknown": the proposal is per category (application, folder, site) `active`, `inactive`, `unavailable` and `helperUnavailable`, plus whether the caller's session is enforced. (b) If an administrator stops the service, site restrictions written to the user's hive stay in place while application and folder blocking end (D8, matching today's `StopAsync`). Is that acceptable, or must stopping the service also clear them, or is stopping a protection-level matter (PROT-001, M7)? |
| Q17 | WIN-006, WIN-010 (silent) | If the service finds an object of its data tree unsafe (D6, R2: unexpected owner or DACL, reparse point, extra link, unexpected entry) it refuses it, repairs nothing and reports storage `unavailable`. What then? (a) Proposal: it stays unavailable until an administrator runs an explicit repair command that moves the object aside, so nothing is deleted silently. (b) The service moves the object aside itself, which is automatic but means that the service renames an object it did not create. (c) Only documented manual steps. Also: while a partition is unavailable the service can neither load nor persist that user's rules; should an already active in-memory session keep enforcing (see Q5)? Consequences to weigh (D6, R1 and the refusal paragraph): the elevated install step creates the tree, so a user-precreated root makes the install fail visibly instead of the first boot after it; but if a root appears or drifts later, an unavailable partition after a reboot means no enforcement for that user until it is repaired, because an in-memory session does not survive a reboot, which is in tension with owner answer K1 Q5 (active rules apply immediately after a start or recovery). That tension is noted for T058 and is not answered here. |
| Q18 | WIN-002, PROT-004 | D6 gives LocalSystem and the built-in Administrators group full control of the data tree, because administrators are inside the trust boundary and need repair and uninstall access. Owner answer K1 Q8 promises no FocusManager panel for another user's data and no resistance to an administrator acting outside FocusManager, which is consistent with this. Is Administrators access to the files acceptable, or should the tree be LocalSystem only (an administrator could still take ownership, and repair and uninstall would be harder, Q13)? |

## Verification Plan

A successful build or unit-test run is not evidence of session-0 behavior, reboot recovery or access isolation.
Those need the manual checks below on Windows; record the Windows build and the accounts used.

Automated checks run on Windows with `dotnet test FocusManager.Windows.slnf`, except where noted.

| # | Check | Step |
| --- | --- | --- |
| V1 | The pipe security descriptor has the expected owner and ACEs, no create-instance right for anyone but LocalSystem, a NETWORK deny and a medium label | S4 |
| V2 | Authorization decisions for synthetic callers: the user, another user, an administrator, network, low integrity, AppContainer, session 0, failed impersonation | S4 |
| V3 | Framing: oversized request, stalled client, concurrent clients, error codes without exception text | S4 |
| V4 | Application filtering by session with a fake owner resolver; the event's `SessionID` is used | S1 |
| V5 | Policy writer against an injected registry root; ledger order and reconciliation after a simulated crash at each step | S1 |
| V6 | Monitor start retry and reported state | S1 |
| V7 | Helper protocol: versions, unknown messages, size limits, session routing, helper exit on disconnect | S2 |
| V8 | Lifecycle decisions for synthetic session events: logon, logoff, disconnect, reconnect, duplicates, enumeration at start | S5 |
| V9 | Contract schema validation and compatibility, including the WinUI client without a version | T047 |
| V10 | Storage partitions are isolated, and their path is outside every user profile | S3 |
| V11 | Optional integration test: a low-integrity child process cannot open the pipe; a medium-integrity process in the same session can | S4 |
| V12 | Owner-resolution race (proposal, D5, ordering constraint 8), with a fake resolver and synthetic events: a process event whose `SessionID` is not cached arrives before the logon notification and is evaluated with the owner's rules; a session with no user and session 0 are not enforced; a resolver failure is retried, the process is not terminated and `unavailable` is reported (pending Q5); a later logon notification for the same session changes nothing | S1 |
| V13 | Adversarial checks of the data tree, DT1–DT14 in D6 with the sub-rows DT1b, DT3b, DT8b, DT11b and DT13b, and the per-control sensitivity rule of DT14 against a "reset the root ACL only" double. The integration rows run elevated or in the VM (D6, "Elevation"); the Result records the elevation state of each run | S3 |
| V14 | Host mode and composition (D10), using the seams of D10 item 7 and no real IPC loop, WMI watcher or `HKCU` access: the decision table for every row; a registration test per mode with fake monitors, enforcers and store, where `Service` mode never registers or constructs `TrayHost`, does not register `TrayNotifier`, `IAgentTray` resolves to `NullAgentTray`, and starting and stopping `AgentHostedService` raises no tray event; `Console` mode resolves `TrayHost`; re-adding the `TrayHost` registration to service mode in a disposable copy makes the test fail; `TrayHost.Start` does nothing when the injected session provider returns 0. V14 does not claim anything about whether `System.Windows.Forms` is loaded; M15 does | S5 |
| V15 | Console mode preserved (D10), a characterization test written against the unchanged behavior after the mechanical interface extraction and before any other T052 change (D10 item 7), with a recording fake `IAgentTray` and fakes for monitors, enforcers, store and IPC loop: `StartAsync` calls `Start` with the persisted flag before study mode is enabled and `SetStudyModeState` after it; an IPC study-mode change calls `SetStudyModeState`; a raised toggle event changes the persisted state; a raised `ExitRequested` calls `StopApplication`; `StopAsync` unsubscribes and calls `Stop`. The expected sequence is the trace of D10. The existing Agent tests do not reference these types and are not evidence | S5 |

Manual checks need a Windows virtual machine with one standard and one administrator account.

| # | Check | Step |
| --- | --- | --- |
| M1 | After the install script: `sc qc` shows LocalSystem, automatic start, a quoted image path and the WMI dependency; `sc qfailure` and `sc qfailureflag` show recovery; `icacls` shows that users cannot write the install and data directories | S5 |
| M2 | Standard user: the UI connects and starts a session; a new forbidden application is terminated; folder navigation is corrected by the helper; the site policy exists under `HKEY_USERS\<SID>` and the browser enforces it | S5 |
| M3 | Administrator with a non-elevated UI: same as M2 | S5 |
| M4 | A second local user (fast user switching) cannot read or change the first user's data; the first user's rules do not apply in the second session (pending Q3) | S5 |
| M5 | Opening `\\<computer name>\pipe\FocusManager.Agent.Pipe` is rejected | S4 |
| M6 | Logoff during a session leaves the service running; after logon the helper is relaunched and blocking resumes without user action | S5 |
| M7 | Reboot during a session: the service runs before logon and blocks after logon; the UI and helper are not terminated (H8); behavior of applications started at logon, including an auto-start entry of a forbidden application that may start before the logon notification is processed (race of D5), is recorded for Q5 | S5 |
| M8 | Ending the helper process leads to a relaunch; the degraded state is visible meanwhile | S5 |
| M9 | Killing the service process as an administrator leads to an SCM restart, restored state and no duplicate helpers | S5 |
| M10 | Lock, unlock, user switching and Remote Desktop where available: enforcement continues and the UI reconnects | S5 |
| M11 | No FocusManager window, tray icon or `explorer.exe` runs in session 0 (session column of `Get-Process`) | S5 |
| M12 | The uninstall script leaves no FocusManager policy values in loaded hives, handles or reports unloaded hives, and deletes the service | S5, T041 |
| M13 | The Tauri tray icon is the only FocusManager tray icon | S6 |
| M14 | With the reduced privilege set of D2 declared: the helper still launches in a standard-user session and an administrator session, and M1–M3 and M8 still pass. Also record `whoami /priv` (or an equivalent token query) of the service process before and after the reduction, including whether `SeBackupPrivilege` and `SeRestorePrivilege` are present and whether they are enabled (D6 R7, A30) | S5 |
| M15 | Service mode creates no window or tray (D10). On the VM run the service from the T052 scripts: the process is in session 0; the log shows `HostMode=Service` with the tray disabled; a probe created by T052 lists the top-level and message-only windows owned by the process and finds none; the module list of the service process contains no `System.Windows.Forms` assembly (the evidence for A29; V14 cannot provide it); no FocusManager tray icon appears in any user session. Positive control first: the same probe, run against a console-mode agent with its tray in a user session, must find the tray's window, otherwise an empty result proves nothing | S5 |
| M16 | Console mode keeps its tray (D10), in an isolated user session or VM and never against the owner's live data, agent, pipe or registry: the tray icon appears, the Study Mode item toggles enforcement, tray Exit stops the agent, as before the change | S5 |

## Assumptions to Validate

These were not confirmed in Microsoft documentation and must be checked by the step named.

| # | Assumption | Step |
| --- | --- | --- |
| A1 | `Shell.Application` created in session 0 does not reach the Explorer windows of a user session | S2 (optional negative check) |
| A2 | ShellExecute of `explorer.exe` from LocalSystem starts an invisible LocalSystem process in session 0 | S2 (by inspection; not to be executed) |
| A3 | For a UAC administrator, `WTSQueryUserToken` returns the filtered token, so the helper runs at medium integrity | S5 |
| A4 | The session's `winsta0\default` already grants that session's logon SID, so no DACL change is needed. The `CreateProcessAsUser` documentation says the DACLs "must grant access" to the token's user or logon session and that the caller must change them (D3). That the interactive session already satisfies this is expected but not verified; M2, M3 and M14 test it, and a failure stops S5 for an owner decision | S5 |
| A5 | Whether a standard user can write their own `HKEY_CURRENT_USER\Software\Policies` browser keys | S1 |
| A6 | Whether Windows stops a privileged process from following registry links that a user created in their own hive | S1 |
| A7 | Supported browsers apply changed registry policies without a restart | S1, T013 |
| A8 | `Win32_ProcessStartTrace` requires administrative rights (WIN-001), and the WMI service to depend on is `Winmgmt` | S5 |
| A9 | Whether .NET `NamedPipeServerStream` sets `PIPE_REJECT_REMOTE_CLIENTS`; otherwise native creation is needed | S4 |
| A10 | A lifetime derived from `WindowsServiceLifetime` receives `OnSessionChange` under the generic host | S5 |
| A11 | Remote Desktop session tokens contain INTERACTIVE (S-1-5-4) | S4 |
| A12 | Standard users can create directories under `%ProgramData%` by default, so pre-creation must be handled; and `ProgramData` and `C:\` keep their default ACLs, so that a standard user cannot rename, delete or replace the `FocusManager` directory or an ancestor and no installer has loosened them (the condition of the R5 invariant). S3 records the actual ACL of `ProgramData` and `C:\` on the VM before DT results are relied on | S3 |
| A13 | A local client's process ID and session ID reported by the pipe are not trustworthy enough for authorization | S4 (design rule regardless) |
| A14 | Microsoft.Data.Sqlite can open an untrusted copy defensively (needed only for the fallback in D6) | S3 |
| A15 | An explicit medium mandatory label can be set when the pipe is created, and medium-integrity clients can then write | S4 |
| A16 | LocalSystem file operations in a user-writable directory can be redirected by links the user creates | S3 (design rule regardless) |
| A17 | A standard user can disable a per-machine `Run` startup entry (for example through the per-user startup approval list), which would make the `HKEY_LOCAL_MACHINE\...\Run` helper option unreliable. Not verified here | only if that option is ever used |
| A18 | The extra local round trips of the thin-helper design are negligible for FLD-001–FLD-007 (a few per blocked folder over a local pipe). Not measured | S2 (measure with the folder tests) |
| A19 | LocalService, NetworkService, a virtual service account and a dedicated local account lack `SE_TCB_NAME` and administrator rights by default. Not re-verified in Microsoft documentation in this revision (the LocalSystem page is cited; the others are not) | S5 |
| A20 | While a user is logging on, `WTSQuerySessionInformation` and `WTSQueryUserToken` return distinguishable results for "no user", "user still logging on" and "owner known", so the retry rule of D5 can tell them apart. Not verified | S1 |
| A21 | `FILE_FLAG_OPEN_REPARSE_POINT` affects only the last path component, so a reparse point in an earlier component is still followed and only the final-path comparison (R2b) detects it. The `CreateFile` page does not say. The strategy does not rely on the flag for earlier components | S3 (DT2, DT3) |
| A22 | A directory handle opened without `FILE_SHARE_DELETE` stops other processes from renaming or deleting that directory. The `CreateFile` page speaks of files and devices | S3 (DT9) |
| A23 | The owner of a directory or file created by the service (LocalSystem token, or an elevated Administrators token in console mode) is LocalSystem or Administrators. R2c accepts both. If another owner appears, R1 sets the owner explicitly | S3 (record the observed owner) |
| A24 | The managed APIs do not create a directory with a descriptor atomically or report that it pre-existed, and SQLite cannot be handed a validated handle, so the data-tree code needs P/Invoke in `FocusManager.Platform.Windows` and the check-to-open gap is closed by the ownership invariant of R5. Whether Microsoft.Data.Sqlite exposes the optional defenses of R8 is also unknown | S3 |
| A25 | SQLite accepts a zero-length main database file created by the service as a new database, and a journal or WAL file that exists at the first open would be applied as recovery data. Both are inferred from the SQLite pages | S3 (DT7) |
| A26 | Sidecar files that SQLite creates in the partition inherit only the partition's inheritable ACEs (LocalSystem and Administrators) | S3 (DT13) |
| A27 | The behavior of WinForms `Application.Run` and `NotifyIcon` in a LocalSystem service session (hidden window, failure, hang) is not observed. D10 does not depend on it | S5 (optional negative check in a VM) |
| A28 | `WindowsServiceHelpers.IsWindowsService()` returns true for the installed service and false for a console, debugger or `Start-Process` launch, including an elevated one. The page read states only "Check if the current process is hosted as a Windows Service"; the mechanism is not documented there | S5 (M1, M15, M16) |
| A29 | With no `TrayHost` registered or constructed, and the console-only registrations in a separate non-inlined method, no `System.Windows.Forms` type is loaded in the service process and no tray window class is registered. Whether the runtime would load `TrayHost` when compiling a method that merely contains its registration is not observed | S5 (M15 module list; the probe) |
| A30 | `SeBackupPrivilege` and `SeRestorePrivilege` are disabled, or absent, in the token of the service process unless code enables them, so that R7's "does not enable" holds. A LocalSystem token contains privileges that are not enabled by default; which ones were not checked in a primary source | S5 (M14 `whoami /priv`) |

## References

Microsoft documentation used for the platform statements above. The draft retrieved all pages on 2026-11-17.
The review revision re-read only [CreateProcessAsUser] and [.NET Windows service] on 2026-11-18 (quoted or
paraphrased where marked); the other pages were not independently re-verified in that revision. The T056
revision read the pages listed after [MSIX services] (the data-tree and tray sources) on 2026-11-18 and 2026-11-19;
see [Source status for D6](#source-status-for-d6). Statements about APIs without a cited page (for example
`WTSQuerySessionInformation`) are not verified.

- [Interactive Services]
- [LocalSystem account]
- [WTSQueryUserToken]
- [CreateProcessAsUser]
- [Named pipe security]
- [CreateNamedPipe]
- [ImpersonateNamedPipeClient]
- [GetNamedPipeClientSessionId]
- [Win32_ProcessStartTrace]
- [RegOpenCurrentUser]
- [SHGetKnownFolderPath]
- [HandlerEx]
- [WM_WTSSESSION_CHANGE]
- [WTSEnumerateSessions]
- [How UAC works]
- [Mandatory Integrity Control]
- [Well-known SIDs]
- [CreateService]
- [SERVICE_DELAYED_AUTO_START_INFO]
- [SERVICE_FAILURE_ACTIONS_FLAG]
- [SERVICE_REQUIRED_PRIVILEGES_INFO]
- [.NET Windows service]
- [WindowsServiceLifetime]
- [PipeOptions]
- [MSIX services]
- [CreateDirectoryW]
- [CreateFileW]
- [GetFinalPathNameByHandle]
- [Reparse points]
- [Hard links and junctions]
- [BY_HANDLE_FILE_INFORMATION]
- [Symbolic link effects]
- [Security Identifiers (Windows Server)]
- [Null DACLs and empty DACLs]
- [SECURITY_DESCRIPTOR_CONTROL]
- [FileSystemAclExtensions.Create]
- [KNOWNFOLDERID]
- [SQLite corruption]
- [SQLite WAL]
- [SQLite security]
- [IsWindowsService]
- [AddWindowsService]
- [UserInteractive]

[Interactive Services]: https://learn.microsoft.com/en-us/windows/win32/services/interactive-services
[LocalSystem account]: https://learn.microsoft.com/en-us/windows/win32/services/localsystem-account
[WTSQueryUserToken]: https://learn.microsoft.com/en-us/windows/win32/api/wtsapi32/nf-wtsapi32-wtsqueryusertoken
[CreateProcessAsUser]: https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-createprocessasuserw
[Named pipe security]: https://learn.microsoft.com/en-us/windows/win32/ipc/named-pipe-security-and-access-rights
[CreateNamedPipe]: https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-createnamedpipea
[ImpersonateNamedPipeClient]: https://learn.microsoft.com/en-us/windows/win32/api/namedpipeapi/nf-namedpipeapi-impersonatenamedpipeclient
[GetNamedPipeClientSessionId]: https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getnamedpipeclientsessionid
[Win32_ProcessStartTrace]: https://learn.microsoft.com/en-us/previous-versions/windows/desktop/krnlprov/win32-processstarttrace
[RegOpenCurrentUser]: https://learn.microsoft.com/en-us/windows/win32/api/winreg/nf-winreg-regopencurrentuser
[SHGetKnownFolderPath]: https://learn.microsoft.com/en-us/windows/win32/api/shlobj_core/nf-shlobj_core-shgetknownfolderpath
[HandlerEx]: https://learn.microsoft.com/en-us/windows/win32/api/winsvc/nc-winsvc-lphandler_function_ex
[WM_WTSSESSION_CHANGE]: https://learn.microsoft.com/en-us/windows/win32/termserv/wm-wtssession-change
[WTSEnumerateSessions]: https://learn.microsoft.com/en-us/windows/win32/api/wtsapi32/nf-wtsapi32-wtsenumeratesessionsw
[How UAC works]: https://learn.microsoft.com/en-us/windows/security/application-security/application-control/user-account-control/how-it-works
[Mandatory Integrity Control]: https://learn.microsoft.com/en-us/windows/win32/secauthz/mandatory-integrity-control
[Well-known SIDs]: https://learn.microsoft.com/en-us/windows/win32/secauthz/well-known-sids
[CreateService]: https://learn.microsoft.com/en-us/windows/win32/api/winsvc/nf-winsvc-createservicew
[SERVICE_DELAYED_AUTO_START_INFO]: https://learn.microsoft.com/en-us/windows/win32/api/winsvc/ns-winsvc-service_delayed_auto_start_info
[SERVICE_FAILURE_ACTIONS_FLAG]: https://learn.microsoft.com/en-us/windows/win32/api/winsvc/ns-winsvc-service_failure_actions_flag
[SERVICE_REQUIRED_PRIVILEGES_INFO]: https://learn.microsoft.com/en-us/windows/win32/api/winsvc/ns-winsvc-service_required_privileges_infow
[.NET Windows service]: https://learn.microsoft.com/en-us/dotnet/core/extensions/windows-service
[WindowsServiceLifetime]: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.windowsservices.windowsservicelifetime
[PipeOptions]: https://learn.microsoft.com/en-us/dotnet/api/system.io.pipes.pipeoptions
[MSIX services]: https://learn.microsoft.com/en-us/windows/msix/packaging-tool/convert-an-installer-with-services
[CreateDirectoryW]: https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createdirectoryw
[CreateFileW]: https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilew
[GetFinalPathNameByHandle]: https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-getfinalpathnamebyhandlew
[Reparse points]: https://learn.microsoft.com/en-us/windows/win32/fileio/reparse-points
[Hard links and junctions]: https://learn.microsoft.com/en-us/windows/win32/fileio/hard-links-and-junctions
[BY_HANDLE_FILE_INFORMATION]: https://learn.microsoft.com/en-us/windows/win32/api/fileapi/ns-fileapi-by_handle_file_information
[Symbolic link effects]: https://learn.microsoft.com/en-us/windows/win32/fileio/symbolic-link-effects-on-file-systems-functions
[Security Identifiers (Windows Server)]: https://learn.microsoft.com/en-us/windows-server/identity/ad-ds/manage/understand-security-identifiers
[Null DACLs and empty DACLs]: https://learn.microsoft.com/en-us/windows/win32/secauthz/null-dacls-and-empty-dacls
[SECURITY_DESCRIPTOR_CONTROL]: https://learn.microsoft.com/en-us/windows/win32/secauthz/security-descriptor-control
[FileSystemAclExtensions.Create]: https://learn.microsoft.com/en-us/dotnet/api/system.io.filesystemaclextensions.create
[KNOWNFOLDERID]: https://learn.microsoft.com/en-us/windows/win32/shell/knownfolderid
[SQLite corruption]: https://www.sqlite.org/howtocorrupt.html
[SQLite WAL]: https://www.sqlite.org/wal.html
[SQLite security]: https://www.sqlite.org/security.html
[IsWindowsService]: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.windowsservices.windowsservicehelpers.iswindowsservice
[AddWindowsService]: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.windowsservicelifetimehostbuilderextensions.addwindowsservice
[UserInteractive]: https://learn.microsoft.com/en-us/dotnet/api/system.environment.userinteractive
