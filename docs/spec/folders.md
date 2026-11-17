# Folders (FLD)

Desktop only (decision B5): Windows and Linux. On phones — `n/a`.

### FLD-001 Blocking folder opening
During a session, opening a folder forbidden by the profile's rules (PRF-002) is blocked.

Platforms: Android — n/a · Windows — partial (allowlist only) · Linux — research · iOS — n/a

### FLD-002 Folder matching
A folder matches a list entry if its path matches (Windows — case insensitive;
without a trailing separator).

Platforms: Android — n/a · Windows — done · Linux — research · iOS — n/a

### FLD-003 Subfolders
An entry has an “include subfolders” flag. When set, the rule also applies to all subfolders;
otherwise, it applies only to the folder itself.

Platforms: Android — n/a · Windows — done · Linux — research · iOS — n/a

### FLD-004 The system drive root is always allowed
The system drive root (`C:\`, without subfolders) is always present in the allowlist
and cannot be removed.

Platforms: Android — n/a · Windows — done · Linux — n/a · iOS — n/a
Note: confirmed by decision I2.

### FLD-005 Action on blocking
When a forbidden folder is opened, the window (tab) navigates back to the nearest allowed folder;
if that is impossible, the window closes; if that is also impossible, the window is redirected to the first
allowed folder.

Platforms: Android — n/a · Windows — done · Linux — research · iOS — n/a

### FLD-006 Virtual folders are not blocked
Virtual shell locations (for example, “This PC”, `shell:` paths) are not blocked.

Platforms: Android — n/a · Windows — done · Linux — research · iOS — n/a

### FLD-007 Multiple tabs
Rules apply separately to each tab in a file manager window.

Platforms: Android — n/a · Windows — done · Linux — research · iOS — n/a
