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
Owner decision [K1 Q10](decisions.md#k1-windows-service-and-session-behavior), 2026-11-18: helper termination
is a degraded technical state, with recovery/retry and truthful capability status. Do not infer malicious
intent or add punitive reactions. Stronger behavior requires a separate owner decision.

Controlled service stop requires the applicable allowed-exit check before stopping. Once stopped, ongoing
restrictions are removed; independent cleanup must also handle crashes/forced termination (K1 Q16).
This does not silently authorize an otherwise-forbidden controlled stop during a strict session.

### PROT-002 Friction
The task required before ending a session is configured in advance: a pause of N seconds and/or entering a specified phrase.
These two task types are sufficient; no additional task type is required ([L1 owner answers, 2026-11-18](decisions.md#l1-profiles-default-exceptions-and-protection)).
Owner clarification (2026-11-18): exit waiting progresses only while the **FocusManager waiting screen
is visible**. An illuminated device display alone is insufficient. Hidden, closed or minimized waiting UI,
device lock and display-off states pause the countdown. Returning to the visible waiting screen resumes
the accumulated wait; hidden time does not count and pausing does not reset the elapsed visible time.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred

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
Owner-approved defaults ([L1 owner answers, 2026-11-18](decisions.md#l1-profiles-default-exceptions-and-protection)): **once per day**, after a **5-minute wait**.
The exit-wait visibility/pause rule in PROT-002 applies to this waiting screen as well.
These replace the earlier weekly/10-minute proposal. Emergency exit removes the session’s
restrictions, including any enabled account-switch restriction (PROT-012); other active sessions
retain their restrictions under SM-004.
> ❓ Question: define the daily reset/time-zone rule and whether the allowance is shared between profiles, sessions or devices before implementation; these details are not specified by “once per day”.

### PROT-006 Weakening protection
Lowering the protection level or changing protection parameters for a profile with an active session requires
passing the current level's check (friction task, code); at the strict level, this is impossible until the session ends.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred

### PROT-007 Uninstall protection
At the strict level, FocusManager cannot be uninstalled until the session ends. This is enabled only with the user's
explicit consent (decision F3).

Platforms: Android — research (subject to K2 policy-compliant limits; no Device Admin guarantee) · Windows — research (agent as a service, WIN-010) · Linux — research · iOS — deferred (`denyAppRemoval`, unreliable)

Android clarification ([K2](decisions.md#k2-android-protection-and-validation)): the owner accepts truthful
strict-mode limitations for the Google Play self-control model. Do not promise prevention of uninstall or
disabling without compliant technical evidence; Accessibility is not blanket authorization for that purpose.
This supersedes any earlier unconditional Android mechanism claim, not the user's right to end a permitted session.

### PROT-008 Blocking system settings
At the strict level, individual system settings screens that disable protection can be blocked.
Only those screens are blocked, rather than all of Settings. The option can be enabled or disabled (decision F4).

Android defaults: accessibility settings, FocusManager's page in the application list,
date and time settings.

Android constraint (K2, 2026-11-18): the listed settings are historical target inputs for feasibility research,
not permission to use a prohibited anti-disable mechanism. Apply only policy-compliant, verified capabilities,
disclose limitations and do not exempt or block all Settings/Play Store/installer indiscriminately.

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

### PROT-012 Optional Windows account switch restriction
Switching to another Windows account is an accepted protection boundary. A user may opt in to an additional
restriction against switching accounts during a protected session (K1 Q3). It is a separate optional
setting ([L1 owner answers, 2026-11-18](decisions.md#l1-profiles-default-exceptions-and-protection)); emergency exit removes this restriction together with the session’s other restrictions.
The mechanism must respect the
session-isolation requirement in WIN-003/WIN-005; do not silently apply a machine-wide policy affecting others.

Platforms: Android — n/a · Windows — research · Linux — n/a · iOS — n/a
Note: [T059](../../tasks/T059-research-optional-windows-account-switch-restriction.md) owns feasibility,
consent, emergency/recovery semantics and bounded implementation planning under T034. No switch policy was
changed or implementation accepted by recording the feature.
> ❓ Question: which protection levels and supported Windows environments can provide the option while preserving isolation? Report unavailable capabilities rather than claiming an unverified guarantee.
