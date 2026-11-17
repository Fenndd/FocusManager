# Profiles and modes (PRF)

A profile is a named set of rules. Every session runs under a profile (decision E3).

### PRF-001 Multiple profiles
The user can create multiple profiles (for example, “Study”, “Sleep”, “Work”), rename,
edit, and delete them. A ready-made “Study” profile is available on first launch.

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
> ❓ Question: how should rules be combined? Proposal: an item is blocked if **at least one**
> active profile blocks it (the strictest rule wins).

### PRF-006 Profiles are synchronized
A user's profiles are identical across all their devices when synchronization is enabled (`sync.md`).
Platform-specific profile parts (desktop folders, applications for a particular OS) are stored separately by platform.

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred
