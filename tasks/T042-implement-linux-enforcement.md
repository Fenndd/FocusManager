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

Scheduling hold (T053, 2026-11-18): [Owner decision J1](../docs/spec/decisions.md#j1-linux-development-hold) conserves this `planned` task. Do not continue Linux research or prototypes, implementation, environment/tool setup, platform tests, CI or packaging until the owner explicitly decides to resume Linux work and the task's other gates are met. Dependency completion or a general request to continue the backlog does not lift the hold. Existing scope, sources, findings and future Windows/Linux sharing remain preserved; Linux-only portions of mixed future tasks are also deferred.

## Acceptance Criteria

1. Tests cover researched application identity (including supported packaging), exclusions, event-to-rule flow and browser apply/cleanup.
2. On a researched supported Linux environment, the Tauri UI controls a session and applications/sites are blocked; record versions and remaining supported-matrix gaps.

## Checks

- Run dotnet test FocusManager.Shared.slnf and dotnet test FocusManager.Linux.slnf on Linux after T020 creates the latter.
- Use T020 documented service/UI commands and T019 device/browser matrix to check blocking, cleanup and system exceptions; update only verified specification statuses.

## Open Questions

When will the owner explicitly decide to resume Linux work? This scheduling gate remains unresolved independently of the listed dependencies; an agent may not resolve it on the owner's behalf.

## Result

Not started. Created by T026 to make missing work or a split explicit. No implementation or checks have been completed.
