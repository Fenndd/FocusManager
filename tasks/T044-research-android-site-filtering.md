---
id: T044
title: Research Android website filtering methods
milestone: M3
status: planned
depends_on: ["T008"]
area: ["docs/architecture/decisions/**", "docs/spec/**", "tasks/**", "ROADMAP.md"]
---

# T044 — Research Android website filtering methods

## Goal

Produce evidence and bounded follow-up tasks for the capabilities split from the foundational platform research.

## Sources

[Android](../docs/spec/platforms/android.md) (AND-005), [websites](../docs/spec/sites.md) (SITE-001–SITE-006), owner decision E4.

## Scope

In scope: prototype address-bar Accessibility in Chrome, Samsung Internet, Opera and Firefox, then local VpnService/DNS filtering and browser-owned encrypted DNS bypass; check method choice and coexistence with another VPN. Review official Accessibility/VpnService store rules. Keep the two methods and their browser matrices separate in evidence, creating child research tasks if the matrix exceeds one session. Coordinate Opera-specific results with T011 rather than repeating its experiments.
Out of scope: production code, accepting limitations or selecting product defaults for the owner.

## Acceptance Criteria

1. Tested mechanisms, platform/browser versions, permissions, bypasses and remaining unknowns are recorded in a proposed ADR; untested hypotheses retain their status.
2. Each capability has a separate implementation or remaining research task, with explicit acceptance/decision gates and checks.
3. Owner-dependent decisions and ADR acceptance are recorded before dependent implementation starts.

## Checks

- Execute one bounded prototype matrix at a time; record actual commands and tested versions. Split additional independent matrices before execution when necessary.
- Verify current official platform/store sources at research time.
- Review evidence, ADR status, specification changes and follow-up task dependencies. No prototype commands exist yet.

## Open Questions

None to collect evidence/options. Research does not grant authority to change product requirements or external resources.

## Result

Not started. Split by T026; no feasibility claims have been verified.
