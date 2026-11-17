# Ways to start blocking (TRG)

Users choose the ways that suit them; a profile can have several (decision E2).

### TRG-001 Manual
The user manually starts a session under a profile — for a specified duration (timer) or until manually ended.
Ending it early depends on the protection level (`protection.md`).

Platforms: Android — planned · Windows — partial (on/off without a timer) · Linux — planned · iOS — deferred

### TRG-002 Scheduled
A session starts and ends automatically according to a schedule: days of the week and time intervals
(for example, weekdays 9:00–13:00).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred

### TRG-003 Time limit
A daily time limit is set for an application, website, or group. After the limit is exhausted, they
are blocked until the end of the day (or until the limit resets).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
Note: iOS has limits in the Screen Time API, but they work unreliably (see `platforms/ios.md`).

### TRG-004 Launch count limit
A maximum daily launch count is set for an application; after it is reached, the application is blocked until the end of the day.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — n/a (no system mechanism)

### TRG-005 Location or network
A session is active while the device is in a specified location (geofence) or connected to a specified Wi-Fi network
(for example, “at university”).

Platforms: Android — planned · Windows — planned (Wi-Fi only) · Linux — planned (Wi-Fi only) · iOS — deferred

### TRG-006 Combining methods
A profile can have several ways to start; the profile is active if any of them triggers.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
