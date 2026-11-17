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

Platforms: Android — planned · Windows — done · Linux — planned · iOS — deferred

### SM-003 Already open items are not blocked
A snapshot is taken when a session starts. Applications launched before the start continue running.
Only **new** actions are blocked (launching, opening a folder, navigating to a website).

Platforms: Android — planned · Windows — done · Linux — planned · iOS — deferred
> ❓ Question: on Android, “an already running application” is barely meaningful (applications live in the background).
> We need to decide what counts as a snapshot on Android (for example, the foreground application at the start),
> or whether to omit snapshots on phones altogether.

### SM-004 Ending a session removes its restrictions
When a session ends, all its restrictions are removed (including browser policies), and the snapshot is cleared.
Restrictions from other active profiles remain in effect (PRF-005).

Platforms: Android — planned · Windows — done · Linux — planned · iOS — deferred

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

Platforms: Android — n/a (UI and agent are in one application — AND-001) · Windows — done · Linux — planned · iOS — n/a
