# Applications (APP)

Application identifiers depend on the platform: Windows — path to an `.exe`; Linux — path to the executable
(to be clarified for Flatpak/Snap, see `platforms/linux.md`); Android — package name (`com.example.app`);
iOS — an opaque system token.

### APP-001 Blocking launch
During a session, launching an application forbidden by the profile's rules (PRF-002) is blocked:
in allowlist mode — anything absent from the list; in blocklist mode — anything present in the list.

Platforms: Android — planned · Windows — partial (allowlist only) · Linux — planned · iOS — deferred

### APP-002 Matching the full identifier
An application matches a list entry if its identifier matches
(Windows: full path, case insensitive, after path normalization).

Implementation state (T054, clarification only; this is not part of the APP-002 requirement). It describes the
legacy file-name fallback that the Windows implementation still has from before the T001 refactor and that APP-003's
removal (decision I1) will delete under T012. While that fallback exists, a file name is taken as `Path.GetFileName`
does: a colon is a file-name boundary only as the drive-prefix colon (in `C:\Apps\evil.exe:Code.exe` the final component
is `evil.exe:Code.exe`), and a component that belongs to the root (a UNC server or share such as `\\server\Code.exe`, a
device or verbatim volume) is never a file name. Examples: `test-vectors/windows-path-compat.json`.

Platforms: Android — planned · Windows — done · Linux — planned · iOS — deferred

### APP-003 Matching by filename
**Removed (2026-11-16, decision I1):** applications are compared only by their full identifier (APP-002).

Platforms: Android — n/a · Windows — partial (filename matching still works — a `change` task is needed) · Linux — n/a · iOS — n/a

### APP-004 Empty list
If the profile's application list is empty, applications are **not restricted** (PRF-004, decision E7).

Platforms: Android — planned · Windows — partial · Linux — planned · iOS — deferred
Note: **behavior change.** Currently on Windows, an empty application allowlist blocks all
new applications (except system applications) — a task of type `change` is required.

### APP-005 System applications are not blocked
System processes and OS components required for the device to work, and FocusManager itself,
are never blocked. The specific list is platform dependent (WIN-003, AND-003, LNX-*).

Owner clarification (L1/L6, 2026-11-18): show critical OS components separately as **Protected System
Components** / **«Защищённые компоненты системы»**, rather than mixing them with editable ordinary
defaults. They are not removable user exceptions; FocusManager must not terminate or disrupt them.
This does not authorize blanket exceptions for every app in a system directory/package group.

Platforms: Android — planned · Windows — done · Linux — planned · iOS — deferred

### APP-006 Editing the list
An application is added by selecting it from installed applications (on desktop, also by choosing a file)
and can be removed from the list. Nonexistent applications and duplicates cannot be added.

Platforms: Android — planned · Windows — done · Linux — planned · iOS — deferred
Note: on Windows, the path must end in `.exe` and the file must exist.

### APP-007 Basic essential applications
In allowlist mode, enabled default app exceptions supplement the profile's application list (E6,
[L1](decisions.md#l1-profiles-default-exceptions-and-protection)). Ordinary default exceptions are
removable on Android and Windows; they are not a blanket permission to override another active profile's
explicit block (PRF-005). Removing one does not prevent the user explicitly allowing it in a profile.
Empty categories still impose no restriction (PRF-004).

Under the owner's explicit delegation, T061 selects these ordinary starting defaults:

| Platform | Default ordinary exceptions | Resolution |
| --- | --- | --- |
| Android | Contacts; Messages (default SMS handler); Clock (alarms/timers); Calculator | Resolve one installed application per function; no duplicate Contacts entry if integrated into Phone, and no assumption of Google/OEM package names |
| Windows | File Explorer; Clock; Calculator | Resolve actual installed identity/path; do not grant every executable in Windows or WindowsApps a default app permission |

Do not automatically add browsers, maps, general messengers/social apps, email, media apps, Settings,
stores, installers, command shells or Task Manager. Users may configure other profile permissions themselves.
Do not install a missing utility or silently allow every installed app in a category. Allowing a utility
does not allow a separate app/site opened through an integration; folder rules still apply to Explorer.

The dedicated view is **Default Exceptions** / **«Исключения по умолчанию»**. Show every resolved ordinary
default entry with its actual name/identifier, reason and current enabled state; users can remove ordinary
entries. Display technical/system exceptions and existing non-app platform defaults transparently as well,
rather than hiding them from the inventory. Windows `C:\` without subfolders remains the existing fixed
folder rule under FLD-004/I2; this app-default decision does not silently remove it.

Calls/emergency functions and critical OS components follow APP-005/AND-003 and the owner-confirmed separate
Protected System Components group. Ordinary defaults remain removable; technical components are not ordinary
user exceptions. Explorer is
also the Windows desktop shell: removing its ordinary exception is not authority to terminate the shell.

Platforms: Android — planned · Windows — planned · Linux — deferred (owner hold J1; no defaults selected) · iOS — deferred
Note: T032 owns bounded UI/discovery/enforcement work; package/path mapping and the Windows broad legacy
exemptions need actual validation. These selected product defaults do not claim runtime blocking works.

Primary platform references informing the distinction (not a vendor-mandated default app list):
[Android dialer/emergency handling](https://developer.android.com/develop/connectivity/telecom/dialer-app),
[Android roles](https://developer.android.com/reference/android/app/role/RoleManager),
[Windows critical services](https://learn.microsoft.com/en-us/windows/win32/rstmgr/critical-system-services).
