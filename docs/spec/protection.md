# Protection against circumvention (PROT)

Levels of protection against the user removing their own blocking (decisions F1–F6, I5).
The level and its parameters are configured in the profile in advance (PRF-003).

### PROT-001 Four protection levels
For each profile, the user chooses a protection level (decision F1):
1. **Normal** — the session can be ended at any time.
2. **With friction** — a task must be completed before ending the session (PROT-002).
3. **Password** — the session can be ended only with a code (PROT-003).
4. **Strict** — the session cannot be ended before its scheduled end, except through emergency exit (PROT-004, PROT-005).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
Note: on iOS, the strict level can be bypassed through Settings (PROT-009).

### PROT-002 Friction
The task required before ending a session is configured in advance: a pause of N seconds and/or entering a specified phrase.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
> ❓ Question: are other tasks needed (solve an arithmetic problem, wait with the screen open, etc.)?

### PROT-003 Password
A session ends only after entering a code that the user set when configuring the profile.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
Note: the “partner holds the code” option is deferred (PROT-011).

### PROT-004 Strict level
While a strict-level session is active:
- the session cannot be ended early (except under PROT-005);
- profile rules and the protection level cannot be changed;
- FocusManager cannot be uninstalled (PROT-007);
- selected system settings screens that can disable protection are blocked (PROT-008).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred

### PROT-005 Emergency exit
The strict level has a **limited** emergency exit: no more than N times per period, with a delay of M minutes.
Parameters are configured in advance (decision F2).

- Emergency exit is **enabled** by default.
- It can be **disabled entirely** — this is an option when choosing the strict level.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
Note: emergency exit through a partner is deferred (PROT-011).
> ❓ Question: default values of N, the period, and M (proposal: once a week, with a 10-minute delay).

### PROT-006 Weakening protection
Lowering the protection level or changing protection parameters for a profile with an active session requires
passing the current level's check (friction task, code); at the strict level, this is impossible until the session ends.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred

### PROT-007 Uninstall protection
At the strict level, FocusManager cannot be uninstalled until the session ends. This is enabled only with the user's
explicit consent (decision F3).

Platforms: Android — planned (device administrator privileges) · Windows — research (agent as a service, WIN-010) · Linux — research · iOS — deferred (`denyAppRemoval`, unreliable)

### PROT-008 Blocking system settings
At the strict level, individual system settings screens that disable protection can be blocked.
Only those screens are blocked, rather than all of Settings. The option can be enabled or disabled (decision F4).

Android defaults: accessibility settings, FocusManager's page in the application list,
date and time settings.

Platforms: Android — planned · Windows — research (Task Manager, services) · Linux — research · iOS — n/a

### PROT-009 Warning about possible circumvention
If protection can be disabled on a platform by bypassing the application (on iOS — revoking Screen Time
access in Settings), the user is warned about this in advance in the UI (decision F5).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
Note: notifying a partner about circumvention is deferred (PROT-011).

### PROT-010 Time that cannot be adjusted
Session and emergency exit timers use time that the user cannot adjust:
changing the system clock does not shorten a session (decision F6).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred

### PROT-011 Accountability partner
**Deferred by the owner (2026-11-16).** A partner (friend, parent) holds the unlock code, can
authorize emergency exit, and receives a notification when protection is bypassed. The way to communicate with the partner is undecided —
this determines whether a cloud server is needed.

Platforms: Android — deferred · Windows — deferred · Linux — deferred · iOS — deferred
