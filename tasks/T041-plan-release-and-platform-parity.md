---
id: T041
title: Plan release packaging and platform acceptance
milestone: M12
status: ready
depends_on: []
area: ["tasks/**", "ROADMAP.md", "docs/spec/**", "docs/architecture/**"]
---

# T041 — Plan release packaging and platform acceptance

## Goal

Produce a bounded, source-linked implementation backlog and identify the decisions needed before coding this feature area.

## Sources

[product.md](../docs/spec/product.md), [platforms/windows.md](../docs/spec/platforms/windows.md), [platforms/linux.md](../docs/spec/platforms/linux.md), [platforms/android.md](../docs/spec/platforms/android.md), [monetization.md](../docs/spec/monetization.md), [telemetry.md](../docs/spec/telemetry.md). Requirement coverage: GEN-001–GEN-007, WIN-001–WIN-010, LNX-001–LNX-003, AND-001–AND-007, PAY-006, TEL-005.

## Scope

In scope: Create separate tasks for supported-version/browser matrix, Windows service installer/update/uninstall and both distribution channels, owner-approved Linux packages, Android signed testing/release, bilingual accessibility/onboarding acceptance, cross-platform offline/feature parity and operational rollback checks. Resolve Linux distributions/formats and signing/access prerequisites with the owner. Reverify official store/testing requirements at execution; no publication, paid services or account setup in this planning task. Include first-public-release paid-tier/legal gates without registering a business now.
Out of scope: production implementation, external resource changes, or deciding unspecified product behavior.

Planning change (T026, owner milestone request on 2026-11-17): moved from M6 to M12 to give this feature area a separate acceptance milestone. Task ID and requirement meaning are unchanged.

## Acceptance Criteria

1. Every capability in scope is assigned to a one-session implementation or research/decision task, with sources, dependencies, precise areas and observable acceptance checks.
2. Open owner questions stay in the owning specification and gate affected implementation tasks; new tasks follow TASK_FORMAT.md.
3. Roadmap and coverage links include all resulting tasks; completing this planning task does not complete the feature or milestone.

## Checks

- Compare task coverage with each linked specification and prerequisite output.
- Validate task metadata, dependency cycles, readiness and area conflicts.
- Review proposed execution checks against actual project tooling; commands not yet created must be marked unavailable.

## Open Questions

None

## Result

Not started. Created by T026 to make missing work or a split explicit. No implementation or checks have been completed.
