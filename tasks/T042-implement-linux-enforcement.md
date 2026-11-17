---
id: T042
title: Implement Linux application and website enforcement
milestone: M4
status: planned
depends_on: ["T019", "T020", "T012", "T022"]
area: ["src/Linux/**", "tests/Linux/**", "src/Desktop/**", "docs/spec/platforms/linux.md"]
---

# T042 — Implement Linux application and website enforcement

## Goal

Add researched application/site enforcement to the working Linux host and verify it through the desktop UI.

## Sources

[Linux](../docs/spec/platforms/linux.md) (LNX-003), [applications](../docs/spec/apps.md) (APP-001, APP-002, APP-005), [sites](../docs/spec/sites.md) (SITE-001–SITE-006), T019 evidence and T020 host.

## Scope

In scope: selected process monitor/blocker, essential system exclusions, supported-browser policy apply/cleanup and unsupported-browser handling using shared session logic and vectors. Split browser and process implementation into child tasks before starting if T019 mechanisms require more than one session.
Out of scope: Linux folder mechanisms (T043/T032), content (T035) and protection (T034), unvalidated mechanisms and profile-mode features assigned by T032.

## Acceptance Criteria

1. Tests cover researched application identity (including supported packaging), exclusions, event-to-rule flow and browser apply/cleanup.
2. On a researched supported Linux environment, the Tauri UI controls a session and applications/sites are blocked; record versions and remaining supported-matrix gaps.

## Checks

- Run dotnet test FocusManager.Shared.slnf and dotnet test FocusManager.Linux.slnf on Linux after T020 creates the latter.
- Use T020 documented service/UI commands and T019 device/browser matrix to check blocking, cleanup and system exceptions; update only verified specification statuses.

## Open Questions

None

## Result

Not started. Created by T026 to make missing work or a split explicit. No implementation or checks have been completed.
