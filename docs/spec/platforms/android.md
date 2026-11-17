# Android platform (AND)

A primary platform (decision B1). Language — Kotlin; UI — Jetpack Compose (decision C1).
Mechanism details will be clarified by a prototype (task T008) wherever the status is `research`.

## Mechanisms

| Feature | Android mechanism | Limitations |
|---------|------------------|-------------|
| Detecting application launches | `AccessibilityService` (`TYPE_WINDOW_STATE_CHANGED`), fallback — `UsageStatsManager` | Accessibility is enabled manually by the user; Google Play requires a declaration and explicit disclosure |
| Blocking applications | A blocking screen over the application / returning to the home screen | Other applications' processes cannot be killed |
| Blocking websites | Both methods (decision E4): reading browser address bars through accessibility services **and** a local `VpnService` with DNS filtering | One VPN slot per device (conflicts with the user's VPN); address bars depend on the browser |
| Keywords, application sections | Accessibility services (reading screen elements) | Breaks when applications update |
| Other applications' notifications | `NotificationListenerService` | Separate permission |
| Time and launch limits, statistics | `UsageStatsManager` + custom tracking | “Usage access” permission |
| Location / Wi-Fi | Geofences (Google Play Services or `LocationManager`), Wi-Fi state | Location permissions (including background location) |
| Background operation | Foreground service with a persistent notification | Manufacturers' power saving may kill the service |
| Protection against uninstalling and disabling | Device administrator privileges; blocking Settings screens through accessibility services | Strict level only (PROT-007, PROT-008) |
| Storage | Local: Room (SQLite); synchronization — Firebase (see [synchronization](../sync.md)) | — |

## Requirements

### AND-001 A single application
The UI and agent form one application: the agent is implemented as a foreground service, with no interprocess IPC needed.

Platforms: Android — planned

### AND-002 Application identifier
An application in a list is identified by its package name. It is selected from a list of installed applications
with icons and names.

Platforms: Android — planned

### AND-003 System applications
The following are not blocked: FocusManager itself, the launcher, system UI, call and emergency call screens,
and the keyboard (APP-005).

Platforms: Android — planned
Note: Settings screens blocked at the strict level — PROT-008.

### AND-004 Permissions
On first launch, the application guides the user through granting the required permissions (accessibility,
usage history, notifications, VPN, location — as needed
for enabled features), and shows which are missing and which features do not work as a result.

Platforms: Android — planned

### AND-005 Choosing the website filtering method
The user can choose a website filtering method: accessibility services, local VPN,
or both (decision E4). If the user has their own VPN, the method without a VPN is suggested.

Platforms: Android — research

### AND-006 Minimum version
Android 8.0 (API 26) and later (GEN-004: covers the vast majority of devices).

Platforms: Android — planned

### AND-007 Resilience to power saving
The application checks whether power saving restricts it and guides the user through
exemption from battery optimization (with instructions for popular manufacturers).

Platforms: Android — planned
