# FocusManager notifications (NTF)

Notifications displayed by FocusManager itself. Hiding other applications' notifications — CNT-003.

### NTF-001 Blocking notification
When something is blocked, the user receives a notification stating what was blocked and why.
Notifications are enabled by default and can be disabled in settings (decision E8).

Platforms: Android — planned · Windows — partial · Linux — planned · iOS — deferred
Note: on Windows, a tray icon notification exists (`TrayNotifier`), but the agent
registers `SilentNotifier` — notifications are disabled and there is no setting. A task of type `change` is required.

### NTF-002 Suppressing repeats
Blocking the same item again within 2 seconds does not produce another notification/log entry.

Platforms: Android — planned · Windows — partial (folders only) · Linux — planned · iOS — deferred
