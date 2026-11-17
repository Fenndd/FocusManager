# iOS platform (IOS) — deferred

Development follows after the primary platforms are finalized (decision B1). There is currently no Mac or
Apple developer account (decision B4). Language — Swift + SwiftUI (decision C2).
iOS implements what the platform allows; impossible features are omitted (decision B5).

## iOS capabilities (analysis dated 2026-10-01; verify with a prototype)

Blocking on iOS is possible only through the Screen Time API (FamilyControls, ManagedSettings,
DeviceActivity): the application passes rules to the system; iOS enforces them.

| Feature | iOS | Comment |
|---------|-----|-------------|
| Blocking selected applications | ✅ | Applications are opaque tokens, selected in a system dialog |
| “Everything except…” allowlist | ✅ | The product's primary mode is possible |
| Blocking websites | ⚠️ | Behavior differs in Safari and other browsers; a limit of about 50 entries |
| Keywords, application sections | ❌ | No access to screen content |
| Other applications' notifications | ❌ | No access to other applications' notifications |
| Schedule | ✅ | |
| Time limit | ⚠️ | Available, but developers report counting errors |
| Launch limit | ❌ | No system mechanism |
| Statistics | ⚠️ | System report only; data cannot be exported |
| Blocking screen | ⚠️ | System template: icon, text, colors, up to two buttons |
| Protection against uninstalling and disabling | ⚠️ | The user can revoke Screen Time access in Settings |
| Synchronizing application lists | ❌ | Tokens cannot be transferred between devices |

## Requirements

### IOS-001 Prerequisites for starting development
Required: a Mac (or cloud Mac), an Apple Developer account ($99/year), and Apple's approval to use
Family Controls (entitlement) for publication.

Platforms: iOS — deferred

### IOS-002 Minimum version
iOS 16 and later (minimum for the Screen Time API in individual authorization mode).

Platforms: iOS — deferred
