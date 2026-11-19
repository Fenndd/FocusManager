# Folders (FLD)

Desktop only (decision B5): Windows and Linux. On phones — `n/a`.

### FLD-001 Blocking folder opening
During a session, opening a folder forbidden by the profile's rules (PRF-002) is blocked.

Platforms: Android — n/a · Windows — partial (allowlist only) · Linux — research · iOS — n/a

### FLD-002 Folder matching
A folder matches a list entry if its path matches (Windows — case insensitive;
without a trailing separator).

Platforms: Android — n/a · Windows — done · Linux — research · iOS — n/a

Implementation state (T054, clarification only — no change of requirement): on Windows the agent keeps the
matching it had before the T001 path refactor for values the UI already persists. The UI saves a selected
`D:\` as `D:`; a not fully qualified stored value is resolved against the agent process's Windows context
(current drive and per-drive current directory, as `Path.GetFullPath` did), so `D:` means the current directory of
drive D: (its root unless the process changed it) and `\Study` means `Study` at the root of the current drive.
Trailing dots and spaces follow Win32 normalization: from the final component of a path that does not end in a
separator, all trailing dots and spaces are removed (`C:\Study. ` is `C:\Study`), including when that component is
the UNC share (`\\srv\share.` is `\\srv\share`) or, with no share, the server; from a component followed by a
separator, exactly one trailing dot is removed (`C:\Study.\Math` is `C:\Study\Math`), while two or more dots and
trailing spaces are kept, and a UNC server or share followed by a separator is kept as written
(`\\srv\share.\x` stays). A trailing separator protects the final component from trimming. The first two segments of a
UNC path are the server and share, taken literally: `.` and `..` there are names, not dot segments
(`\\srv\..\share\x` is not under `\\srv\share`), and a later `..` cannot remove them. A `\\.\` device path is
normalized like a rooted path, a path with the literal verbatim prefix `\\?\` is compared as written (Win32 does
not normalize it), and 8.3 short names (`~`) are expanded by the host. T063 compatibility clarification: three or more
leading separators preserve the empty UNC server slot, so `\\\srv\share\x` is distinct from
`\\srv\share\x`. With exactly three leading separators, `srv` occupies the literal share slot; with four or
more, both server and share slots are empty and subsequent names are ordinary segments. Extra separators after
the empty root slots collapse as Win32 does. This applies to candidate and stored paths; it does not introduce
new folder-matching requirements. This context-dependent resolution lives in the
Windows platform adapter, not in shared code; how an arbitrary drive-relative value such as `D:Study` should be
interpreted is not specified here and keeps this previous behavior. Examples: `test-vectors/windows-path-compat.json`.

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
