# Sessions (SM)

A session is a period when a profile's rules apply (`profiles.md`). Historically, on Windows this
was called “study mode”, with a single rule set.

### SM-001 Starting and ending a session
A session under a profile starts and ends manually or through a trigger (`triggers.md`).
Whether it can end early depends on the profile's protection level (`protection.md`).

Platforms: Android — planned · Windows — partial (one implicit profile, manual on/off) · Linux — planned · iOS — deferred

### SM-002 Persisting state
Session state persists across restarts. If a session was active, it becomes active again after the
agent (or device) restarts, and restrictions apply.

Windows recovery clarification ([K1 Q5/Q7](decisions.md#k1-windows-service-and-session-behavior), owner 2026-11-18):
when the service starts or recovers with active rules, it applies them to already-running forbidden applications
as well as subsequent launches. Recovery must not create a new blanket exemption snapshot. Applications
started at logon are subject to the active rules. The separate default-off setting for starting a new blocking
session on application launch does not disable restoration of an already-active session.

Platforms: Android — planned · Windows — partial (session flag persists; required service-recovery sweep is planned) · Linux — planned · iOS — deferred
> ❓ Question (T058): define safe bounded behavior when process ownership is temporarily unavailable; the service must not guess another user's rules. The startup/recovery protocol remains unimplemented.

### SM-003 Already open items are not blocked
A snapshot is taken when a session starts. Applications launched before the start continue running.
Only **new** actions are blocked (launching, opening a folder, navigating to a website).

Platforms: Android — planned · Windows — done · Linux — planned · iOS — deferred
Android clarification ([K2](decisions.md#k2-android-protection-and-validation), owner 2026-11-18): desktop
snapshot exemptions do not automatically apply to background Android applications. Use explicit allowed/essential
applications; a special foreground-at-start exception remains an open prototype-backed choice.

Windows clarification (K1 Q5): the existing initial-session behavior above does not authorize recovery to
exempt forbidden applications. Service startup/recovery must check them under SM-002, rather than recapture
an exemption snapshot. T058 must map the initial-session and recovery paths explicitly before implementation.

### SM-004 Ending a session removes its restrictions
When a session ends, all its restrictions are removed (including browser policies), and the snapshot is cleared.
Restrictions from other active profiles remain in effect (PRF-005).

Windows service clarification (K1 Q16): controlled stop after the allowed-exit checks removes all ongoing
restrictions; a separate mechanism must also remove them after service crash/forced termination. This is
cleanup of ongoing controls, not reversal of an application termination already performed. Recovery of an
active session follows SM-002. Cleanup must not remove other sessions' or newly recovered restrictions.

Platforms: Android — planned · Windows — partial (session-end cleanup exists; independent crash cleanup is planned) · Linux — planned · iOS — deferred

### SM-005 Editing a profile during a session
Profile rules can be changed during a session; changes apply without restarting the session.

Platforms: Android — planned · Windows — done · Linux — planned · iOS — deferred
Restrictions: rules cannot be changed at the strict level (PROT-004); weakening protection follows PROT-006.
Note: on Windows, new application and folder rules apply to the next event;
website policies are reapplied immediately on saving.

### SM-006 Warning about empty categories
The UI shows which profile categories are populated and warns that empty categories
**will not be restricted** (PRF-004).

Platforms: Android — planned · Windows — partial (warns, but “empty” currently has a different meaning on Windows — APP-004) · Linux — planned · iOS — deferred

### SM-007 Agent status in the UI
On desktop, where the UI and agent are separate processes, the UI shows whether the agent is available and the current session state.
If the agent is unavailable, the state is “unknown” and an option to refresh the status is provided.

Platforms: Android — n/a (UI and agent are in one application — AND-001) · Windows — partial (connection/session state exists; richer enforcement/error states are planned) · Linux — planned · iOS — n/a
Owner decision K1 Q9/Q16(a): distinguish authorization failure and incompatible version from an unavailable
agent, and show truthful per-category enforcement state (applications, sites, folders) plus whether the
caller's OS session is actually enforced. UI wording should explain the affected capability; internal codes
need not be shown as labels. Current connection/session state exists; richer states are planned in T058 and
the resulting implementation tasks.
