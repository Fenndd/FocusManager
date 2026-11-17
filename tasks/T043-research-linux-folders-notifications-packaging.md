---
id: T043
title: Research Linux folders notifications and packaging
milestone: M4
status: planned
depends_on: ["T019"]
area: ["docs/architecture/decisions/**", "docs/spec/**", "tasks/**", "ROADMAP.md"]
---

# T043 — Research Linux folders notifications and packaging

## Goal

Produce evidence and bounded follow-up tasks for the capabilities split from the foundational platform research.

## Sources

[Linux](../docs/spec/platforms/linux.md) (LNX-001), [folders](../docs/spec/folders.md) (FLD-001–FLD-007), [content](../docs/spec/content.md) (CNT-003), [product](../docs/spec/product.md) (GEN-006).

## Scope

In scope: prototype folder enforcement via fanotify/file-manager integration, selective notification hiding on GNOME/KDE, and root-service-compatible packaging on owner-selected distributions/environments. Do not equate global Do Not Disturb with the specified selective CNT-003 behavior. Resolve distribution/format choices with the owner using evidence, then create separate production tasks for folder behavior, notification suppression and packaging.
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
