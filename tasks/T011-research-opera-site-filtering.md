---
id: T011
title: Find a site-filtering mechanism for Opera
milestone: M2
status: ready
depends_on: []
area: ["docs/architecture/decisions/**", "docs/spec/sites.md", "docs/spec/platforms/*.md", "tasks/**", "ROADMAP.md"]
---

# T011 — Find a site-filtering mechanism for Opera

## Goal

Determine an Opera site-filtering mechanism for each main platform and plan implementation without implementing it in this research task.

## Sources

Legacy classification: type `research`; platform `all`; priority `medium`.

Requirement identifiers: `SITE-001`, `SITE-005`, `SITE-006`.

Legacy ADR associations: ADR-0007.

[Site requirements](../docs/spec/sites.md), [platform requirements](../docs/spec/platforms/), and [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md).

Opera support is required by the owner decision of 2026-11-16 (SITE-005). The original context cites StatCounter October 2026 at approximately 3% desktop / 2% mobile share. Opera uses Chromium, but user/administrator reports say it ignores the enterprise `URLBlocklist` / `URLAllowlist` policies used for other desktop browsers. Until a mechanism exists, it is blocked as unsupported (SITE-006). Treat those reports as a research hypothesis, not a verified mechanism limitation.

## Scope

In scope:
1. Test on Windows and Linux whether current Opera and Opera GX ignore Chromium registry / JSON policies.
2. Compare desktop alternatives, including a browser extension (installation, removal protection, Opera extension-store rules), system filtering (local DNS / proxy, WFP on Windows) for Opera only or all browsers, and address reading through OS accessibility (Windows UI Automation, Linux AT-SPI).
3. Test Android Opera address-bar Accessibility and VPN filtering; T008 investigates the general Android case.
4. Assess reliability, bypasses, load, and required rights.

Out of scope: implementation.

## Acceptance Criteria

1. A chosen method is documented for each platform, with an ADR for a new mechanism such as an extension or DNS filter.
2. Implementation tasks exist; SITE-005 and `docs/spec/platforms/*.md` are updated.

## Checks

- Test Opera / Opera GX policies and each candidate mechanism on the platforms above; record versions, permissions, results, bypasses, and load.
- Inspect the selected methods, any ADR, linked implementation tasks, and updated requirements. No prototype command exists yet; record the actual commands/manual steps used during research.

## Open Questions

None

## Result

Not started. No implementation or checks have been completed for this task.
