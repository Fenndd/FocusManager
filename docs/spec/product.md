# Product (GEN)

Working name: **FocusManager** (decision A4).

## Goal

FocusManager helps users concentrate — primarily on studying. Users configure **profiles**
(“Study”, “Sleep”, “Work”…) with rules defining what is allowed or forbidden and ways to start them
(manually, on a schedule, through limits). While a profile is active, distractions are blocked
across all the user's devices.

This is a **self-control** tool: users restrict themselves (not parental control).

## Audience (decision A1)

1. University and school students — the primary product and marketing focus.
2. Anyone who wants to concentrate: work, sleep, breaking the habit of endless scrolling.

## Concept (decision A2)

Two rule modes:
- **Allowlist** (primary, a differentiator from competitors): only selected items are allowed during a session.
- **Blocklist** (familiar to blocker users): selected items are blocked.

## Development directions (decision A3)

All directions must appear in the product; the roadmap determines their order.

| Direction | Summary |
|-------------|--------|
| Study sessions | Subjects, rule sets per subject, exam mode |
| Accountability partner | **Deferred.** A friend/parent holds the unlock code; notify the partner on circumvention (PROT-011) |
| One session across all devices | Starting on a phone also blocks the desktop, and vice versa (requires cloud) |
| Protection levels | Four levels of difficulty for removing blocking (`protection.md`) |
| Gentle friction | A pause or a “why are you opening this?” question instead of a hard prohibition |
| Gamification | Daily streaks, achievements |

## Glossary

| Term | Meaning |
|--------|----------|
| Profile | A rule set (mode, lists, ways to start, protection level). See `profiles.md`. |
| Allowlist mode | Only items in the profile's lists are allowed (+ basic essential applications). |
| Blocklist mode | Only items in the profile's lists are blocked. |
| Session | A period when a profile's rules apply. Starts manually or through a trigger. |
| Trigger | A way to start a session: manual, schedule, limit, location/Wi-Fi. See `triggers.md`. |
| Session snapshot | What was running when the session started (not blocked on desktop). |
| Blocking | A platform action that prevents a forbidden item from being opened. |
| Agent | A background component that monitors events and enforces blocking. |
| Primary platforms | Android, Windows, Linux. |

## General requirements

### GEN-001 Platforms
The primary platforms are Android, Windows, Linux; they have the same functionality, with folder
blocking additionally available on desktop (decisions B1, B5). iOS follows after the primary platforms are finalized, with only the feasible
subset. macOS is not planned (decision B3).

Platforms: Android — planned · Windows — partial · Linux — planned · iOS — deferred

### GEN-002 UI languages
English and Russian at launch. All UI strings must be localizable from day one (no
text embedded in code), allowing all popular languages to be added later (decision A5).

Platforms: Android — planned · Windows — planned (currently hardcoded strings, English only) · Linux — planned · iOS — deferred

### GEN-003 Regions
The application is distributed in all countries except China (decision A6).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred

### GEN-004 Supported OS versions
Supported OS versions cover the majority of devices currently in use (decision B6).
Specific minimum versions are in `platforms/*.md`.

Platforms: Android — planned · Windows — done · Linux — research · iOS — deferred

### GEN-005 Offline operation
All blocking features work without internet access and without an account. Cloud is needed only for synchronization
(see `sync.md`, decision D1).

Platforms: Android — planned · Windows — partial (currently requires a PostgreSQL server — see WIN-006) · Linux — planned · iOS — deferred

### GEN-006 Distribution channels
- Android — Google Play only (decision H1);
- Windows — the product's own website and Microsoft Store (decision H2);
- Linux — undecided (LNX-001).

Platforms: Android — planned · Windows — planned · Linux — research · iOS — deferred
Note: a new personal Google Play account requires closed testing (12 testers, 14 days)
before release. For Microsoft Store, whether an agent with administrator privileges is allowed needs to be checked.

### GEN-007 Product website
The product has a simple website: description, desktop downloads, privacy policy (decision H3, TEL-005).

Platforms: Android — planned · Windows — planned · Linux — planned · iOS — deferred

## Functional areas

| Area | File | Android | Windows | Linux | iOS |
|---------|------|---------|---------|-------|-----|
| Profiles and modes | [profiles.md](profiles.md) | planned | partial | planned | deferred |
| Ways to start | [triggers.md](triggers.md) | planned | partial | planned | deferred |
| Sessions | [study-mode.md](study-mode.md) | planned | done | planned | deferred |
| Applications | [apps.md](apps.md) | planned | partial | planned | deferred |
| Websites | [sites.md](sites.md) | planned | partial | planned | deferred |
| Folders | [folders.md](folders.md) | n/a | done | research | n/a |
| Content (keywords, sections, notifications) | [content.md](content.md) | planned | research | research | n/a |
| FocusManager notifications | [notifications.md](notifications.md) | planned | partial | planned | deferred |
| Statistics | [stats.md](stats.md) | planned | planned | planned | deferred |
| Account and synchronization | [sync.md](sync.md) | planned | planned | planned | deferred |
| Technical analytics | [telemetry.md](telemetry.md) | planned | planned | planned | deferred |
| Protection against circumvention | [protection.md](protection.md) | planned | planned | planned | deferred |
| Monetization | [monetization.md](monetization.md) | planned | planned | planned | deferred |

Platform details: [Windows](platforms/windows.md), [Linux](platforms/linux.md),
[Android](platforms/android.md), [iOS](platforms/ios.md).
