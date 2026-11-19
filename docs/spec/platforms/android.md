# Android platform (AND)

A primary platform (decision B1). Language — Kotlin; UI — Jetpack Compose (decision C1).
Mechanism details will be clarified by a prototype (task T008) wherever the status is `research`.

## Owner decisions (2026-11-18)

[Decision K2](../decisions.md#k2-android-protection-and-validation) records the accepted product direction:

- Use the Google Play compliant distribution model. Android protection levels must describe their actual limits; strict mode must not promise prevention of disabling or uninstalling without validated, policy-compliant evidence.
- Evaluate Accessibility for each concrete feature purpose, with the required policy review, disclosure, consent and approval. This is not blanket authorization to prevent disabling or uninstalling FocusManager.
- A persistent foreground-service notification is acceptable.
- Do not carry the desktop snapshot of running applications into Android automatically. Android uses explicitly allowed applications and essential exceptions; any special foreground-application exception needs a separate decision and prototype evidence.
- Essential exceptions must preserve functional phone operation and emergency access. Settings, Google Play Store and package installers do not receive an automatic blanket exemption. This does not authorize blocking those screens through a prohibited mechanism.
- Require evidence from both a real device and emulators before accepting an enforcement mechanism. These decisions do not accept an implementation, default package list or mechanisms ADR.

The mechanisms below remain candidates for research and validation; owner acceptance of this direction does not establish that they work or that Google Play will approve a particular use.

## Mechanisms

| Feature | Android mechanism | Limitations |
|---------|------------------|-------------|
| Detecting application launches | Candidates: `UsageStatsManager`; `AccessibilityService` (`TYPE_WINDOW_STATE_CHANGED`) for a justified feature purpose | Selection and fallback require prototype evidence; Accessibility is enabled manually and requires applicable Google Play declaration, disclosure, consent and approval |
| Blocking applications | A blocking screen over the application / returning to the home screen | Other applications' processes cannot be killed |
| Blocking websites | Both methods (decision E4): reading browser address bars through accessibility services **and** a local `VpnService` with DNS filtering | One VPN slot per device (conflicts with the user's VPN); address bars depend on the browser |
| Keywords, application sections | Accessibility services (reading screen elements) | Breaks when applications update |
| Other applications' notifications | `NotificationListenerService` | Separate permission |
| Time and launch limits, statistics | `UsageStatsManager` + custom tracking | “Usage access” permission |
| Location / Wi-Fi | Geofences (Google Play Services or `LocationManager`), Wi-Fi state | Location permissions (including background location) |
| Background operation | Foreground service with a persistent notification | Manufacturers' power saving may kill the service |
| Protection against uninstalling and disabling | No accepted mechanism; research under PROT-007 and PROT-008 | Google Play compliant self-control model only; no unvalidated anti-disable or anti-uninstall promise, and no blanket Accessibility-based Settings blocking authorization (decision K2) |
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

Decision K2 preserves this essential set while requiring functional phone operation and emergency access.
Settings, Google Play Store and package installers are not automatically exempt as whole applications;
strict-level protection remains subject to [protection research](../protection.md) and policy-compliant validation.

T061's owner-delegated ordinary default set is Contacts, the default SMS application, Clock and Calculator
([APP-007](../apps.md#app-007-basic-essential-applications)); all ordinary defaults are removable in
Default Exceptions. The owner confirmed a separate Protected System Components group for the critical
APP-005/AND-003 functions; these are not removable ordinary exceptions. Phone and emergency paths need
functional device-specific handling, including the preloaded emergency dialer where applicable.
> ❓ Question: resolve actual role/default-app packages and device-specific mappings, including Android 8–9 fallbacks. Selection of the ordinary functions is decided; technical mapping is not a new owner choice.

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
