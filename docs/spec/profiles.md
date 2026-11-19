# Profiles and modes (PRF)

A profile is a named set of rules. Every session runs under a profile (decision E3).

### PRF-001 Multiple profiles
The user can create multiple profiles (for example, “Study”, “Sleep”, “Work”), rename,
edit, and delete them. Profiles are created by the user; no predefined or seeded profiles are provided
([L1 owner answers, 2026-11-18](decisions.md#l1-profiles-default-exceptions-and-protection)). This supersedes the earlier first-launch “Study” profile.

Platforms: Android — planned · Windows — planned (currently one implicit rule set) · Linux — planned · iOS — deferred

### PRF-002 Profile mode
A profile has a mode (decision A2):
- **allowlist** (default) — only items in the profile's lists are allowed, plus basic
  essential applications (APP-007);
- **blocklist** — only items in the profile's lists are blocked.

The mode applies to all profile categories (applications, websites, folders).

Platforms: Android — planned · Windows — partial (allowlist only) · Linux — planned · iOS — deferred

### PRF-003 Profile contents
A profile contains:
- lists of applications, websites, and folders (folders — desktop only);
- keywords and blocked application sections (`content.md`);
- whether notification blocking for blocked applications is enabled (CNT-003);
- ways to start sessions (`triggers.md`);
- the protection level and its parameters (`protection.md`).

Platforms: Android — planned · Windows — partial (application/website/folder lists only) · Linux — planned · iOS — deferred

### PRF-004 Empty list means no restrictions
If a category's list in a profile is empty, that category is **not restricted** — in either mode
(decision E7). For example, an allowlist profile containing only a website list restricts websites but not applications.

Platforms: Android — planned · Windows — partial (true for websites; applications currently behave the opposite way — see APP-004) · Linux — planned · iOS — deferred

### PRF-005 Multiple active profiles
Multiple profiles can be active simultaneously (for example, a “Sleep” schedule and a manual “Study” session).

Platforms: Android — planned · Windows — n/a currently · Linux — planned · iOS — deferred
Owner decision ([L1 owner answers, 2026-11-18](decisions.md#l1-profiles-default-exceptions-and-protection)): restrictions from active profiles combine: an item is blocked
if **any** active profile blocks it. For a category, nonempty allowlists intersect and blocklists
combine by union; a block from any profile takes precedence. An empty category contributes no
restriction, as required by PRF-004. Ending one session removes only its contribution (SM-004);
permitted live edits recompute the combined restrictions (SM-005, PROT-004).

[T028](../../tasks/T028-resolve-profile-composition.md) still owns the shared examples and model handoff;
recording this decision does not implement multiple profiles.

### PRF-006 Profiles are synchronized
A user's profiles are identical across all their devices when synchronization is enabled (`sync.md`).
Platform-specific profile parts (desktop folders, applications for a particular OS) are stored separately by platform.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred

### PRF-007 Profile selection for automatic blocking
When the separate automatic-blocking setting starts a new session on application launch, it uses
the last-used profile or profiles by default. The user may configure an explicit selection instead
([L1 owner answers, 2026-11-18](decisions.md#l1-profiles-default-exceptions-and-protection)). The new-session automatic-blocking setting remains **off by default** (K1 Q7).
This selection does not replace restoration of already-active sessions under SM-002.

Platforms: Android — planned · Windows — planned · Linux — planned (on owner hold) · iOS — deferred
