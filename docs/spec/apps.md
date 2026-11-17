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

Platforms: Android — planned · Windows — done · Linux — planned · iOS — deferred

### APP-006 Editing the list
An application is added by selecting it from installed applications (on desktop, also by choosing a file)
and can be removed from the list. Nonexistent applications and duplicates cannot be added.

Platforms: Android — planned · Windows — done · Linux — planned · iOS — deferred
Note: on Windows, the path must end in `.exe` and the file must exist.

### APP-007 Basic essential applications
In allowlist mode, basic essential applications that are not distracting are always allowed in addition to the profile's list
(decision E6). Calls and emergency calls are always allowed, without exception.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
> ❓ Question: the exact default list (proposal for phones: calls, SMS, clock/alarm,
> calculator, maps; for desktop — Explorer/file manager, calculator), and whether the user
> can change it.
