---
id: T034
title: Plan protection levels and bypass handling
milestone: M7
status: planned
depends_on: ["T021"]
area: ["tasks/**", "ROADMAP.md", "docs/spec/**", "docs/architecture/**"]
---

# T034 — Plan protection levels and bypass handling

## Goal

Produce a bounded, source-linked implementation backlog and identify the decisions needed before coding this feature area.

## Sources

[protection.md](../docs/spec/protection.md), [platforms/windows.md](../docs/spec/platforms/windows.md), [platforms/linux.md](../docs/spec/platforms/linux.md), [platforms/android.md](../docs/spec/platforms/android.md). Requirement coverage: PROT-001–PROT-010, PROT-012, WIN-007, WIN-010.

## Scope

In scope: Create separate tasks for normal/friction/code exit checks, active-session weakening rules, strict mode and preconfigured emergency exit, platform uninstall/settings feasibility and capability warnings. Separate shared policy from each OS integration/UI. Apply the L1-confirmed friction pause/phrase task types and emergency-exit defaults
(once per day after a five-minute wait, enabled by default and disableable). Apply the owner-confirmed
PROT-002 visible FocusManager waiting-screen condition, including pause/resume checks. Resolve
PROT-005 daily-reset/allowance scope, code handling and missing timer/reboot
semantics before their affected implementation. Account-switch prevention remains a separate optional
setting; emergency exit removes it with the session’s other restrictions. Linux-only work remains on hold. Keep partner features deferred.
Out of scope: production implementation, external resource changes, or deciding unspecified product behavior.

Planning change (T026, owner milestone request on 2026-11-17): moved from M5 to M7 to give this feature area a separate acceptance milestone. Task ID and requirement meaning are unchanged.

Owner update (T057, 2026-11-18): [K1](../docs/spec/decisions.md#k1-windows-service-and-session-behavior)
requires session-local restrictions, protected controlled-stop authorization and cleanup even after service
failure. Machine-wide HKLM behavior that affects other sessions is no longer approved. [K2](../docs/spec/decisions.md#k2-android-protection-and-validation)
accepts only a compliant, truthfully limited Android strict model. Include PROT-012 and
[T059](T059-research-optional-windows-account-switch-restriction.md) in the option/feasibility coverage;
it must not be mistaken for implemented account-switch prevention. T058 owns the prerequisite Windows service
protocol reconciliation; do not overlap its service/ADR edits.

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
