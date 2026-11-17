# Specification: maintenance rules

The specification is the source of truth about FocusManager's **behavior**. Code that contradicts it is a bug
(or the specification is outdated, in which case it must be corrected within the task).

## Structure

- One file per functional area (`study-mode.md`, `apps.md`, …). These files are
  **platform independent**: they describe what the user sees and gets.
- `platforms/<platform>.md` describes how requirements are implemented on a particular platform,
  platform limitations, and platform-specific requirements.

## Platforms

The primary platforms are **Android, Windows, Linux** — they have the same functionality (decision B5).
iOS follows after the primary platforms are finalized, with the subset of features that iOS permits.
macOS is not planned.

## Requirement format

```markdown
### APP-003 Allow by filename
If a launch event provides only a path with a different directory, the application is allowed
when the executable filename matches the filename in the allowlist.

Platforms: Android — n/a · Windows — done · Linux — planned · iOS — deferred
Note: ... (optional)
```

- **ID**: `<PREFIX>-<NNN>`. IDs are never reused or changed.

  | Prefix | Area | File |
  |---------|---------|------|
  | `GEN` | General product requirements | `product.md` |
  | `PRF` | Profiles and modes | `profiles.md` |
  | `TRG` | Ways to start blocking | `triggers.md` |
  | `SM` | Sessions | `study-mode.md` |
  | `APP` | Applications | `apps.md` |
  | `SITE` | Websites | `sites.md` |
  | `FLD` | Folders (desktop) | `folders.md` |
  | `CNT` | Content: keywords, application sections, notifications | `content.md` |
  | `NTF` | FocusManager notifications | `notifications.md` |
  | `STAT` | User statistics | `stats.md` |
  | `SYNC` | Account and synchronization | `sync.md` |
  | `TEL` | Technical analytics and privacy | `telemetry.md` |
  | `PROT` | Protection against circumvention | `protection.md` |
  | `PAY` | Monetization | `monetization.md` |
  | `WIN`, `LNX`, `AND`, `IOS` | Platform-specific requirements | `platforms/*.md` |

- **Platform statuses**: `done` — implemented; `partial` — partially implemented (explain in a note);
  `planned` — required but not yet present; `research` — needed, but feasibility is unconfirmed;
  `deferred` — postponed: a platform (iOS) or a feature deferred by the owner; `n/a` — not applicable on the platform.
- Wording must be verifiable: the requirement must make clear which test would confirm it.
- Mark open questions with `> ❓ Question:` directly below the requirement — this signals the product owner.
  Product-level questions are collected in [decisions.md](decisions.md); requirements refer to them by code (`decision E7`).

## Document status

- Requirements with `Windows — done` were reconstructed from existing code (2026-09-30).
- On 2026-10-02, the specification was revised based on the owner's decisions (sections A–E in `decisions.md`).
- On 2026-11-16, decisions F–I were added: protection against circumvention (`protection.md`), monetization, distribution
  channels, Windows questions; desktop — SQLite; partner features deferred; desktop UI — Tauri;
  no file export (D5). All questions in `decisions.md` are closed.
