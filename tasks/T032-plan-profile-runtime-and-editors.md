---
id: T032
title: Plan profile runtime and platform editors
milestone: M5
status: planned
depends_on: ["T021", "T029"]
area: ["tasks/**", "ROADMAP.md", "docs/spec/**", "docs/architecture/**"]
---

# T032 — Plan profile runtime and platform editors

## Goal

Produce a bounded, source-linked implementation backlog and identify the decisions needed before coding this feature area.

## Sources

[profiles.md](../docs/spec/profiles.md), [apps.md](../docs/spec/apps.md), [sites.md](../docs/spec/sites.md), [folders.md](../docs/spec/folders.md), [study-mode.md](../docs/spec/study-mode.md). Requirement coverage: PRF-001–PRF-007, APP-001–APP-007, SITE-001–SITE-006, FLD-001–FLD-007, SM-001–SM-007.

## Scope

In scope: Create separate tasks for shared multi-profile/mode evaluation; desktop profile CRUD/editor and agent contract integration; Android profile CRUD/Room integration; platform enforcement integration, system/essential exceptions, installed-app discovery on desktop, all folder behaviors (block action, virtual folders and multiple tabs) and parity checks. Separate UI work from runtime work. Preserve initial-session snapshot and empty-list behavior, and the separate K1 recovery rules.
Apply the L1 owner answers: user-created profiles without seeded profiles; combined active-profile
restrictions under PRF-005; last-used automatic-blocking profile selection with an explicit-selection
option under PRF-007. Keep the new-session automatic-blocking setting default-off and separate
from recovery of already-active sessions. Use the confirmed APP-007 defaults and retain unresolved
OS-specific details as questions rather than inventing them. Linux-only work remains on the owner hold.
Out of scope: production implementation, external resource changes, or deciding unspecified product behavior.

## Acceptance Criteria

Default-exception integration (T061): [APP-007](../docs/spec/apps.md#app-007-basic-essential-applications)
defines the selected Android/Windows functions and the Default Exceptions view. Create bounded discovery,
editable-list UI and runtime tasks; every ordinary default must be removable and every technical/platform
default must be visible with its classification. Resolve installed identities rather than hardcoded OEM
names or whole-directory bypasses. Windows currently ignores Windows/WindowsApps broadly, so include a
test that removing Clock/Calculator/Explorer's ordinary permission has the intended effect without harming
critical OS functionality or bypassing folder rules. The owner confirmed a separately displayed Protected
System Components group; keep those technical components protected and ordinary entries removable.
Do not invent locked ordinary entries or perform Linux work.

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
