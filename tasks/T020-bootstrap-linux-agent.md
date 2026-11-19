---
id: T020
title: Bootstrap the Linux agent
milestone: M4
status: planned
depends_on: ["T019", "T002", "T003", "T017"]
area: ["src/Linux/**", "tests/Linux/**", "FocusManager.Linux.slnf", "FocusManager.sln", "scripts/linux/**", "src/Desktop/**", ".github/workflows/ci.yml", "docs/spec/**"]
---

# T020 — Bootstrap the Linux agent

## Goal

Run a Linux systemd host using shared session logic, SQLite and authenticated Unix-socket control from Tauri.

## Sources

Legacy classification: type `feature`; platform `linux`; priority `medium`.

Requirement identifiers: `LNX-002`, `LNX-003`, `APP-001`, `SITE-001`.

Legacy ADR associations: ADR-0007.

[Linux requirements](../docs/spec/platforms/linux.md), [application rules](../docs/spec/apps.md), [site rules](../docs/spec/sites.md), and [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md).

The Linux agent uses C#/.NET, shares `src/Shared` with Windows, and keeps platform-specific code in `src/Linux`. T019 determines the mechanisms.

## Scope

In scope:
1. Create Linux platform/host projects, tests and FocusManager.Linux.slnf; add projects to FocusManager.sln.
2. Add systemd unit and development scripts, Linux storage-path configuration and user-restricted Unix-socket IPC through T017 transport.
3. Verify UI status/session control, persistence and reconnect using explicit test adapters for enforcement not yet implemented.
4. Add Linux host build/test CI using the current SDK.

Out of scope: production application/site enforcement (T042), folders/notifications/packaging research (T043). Do not describe a scaffold session as actual blocking.

Planning change (T026, 2026-11-17): Add the missing T017 dependency and separate host/IPC bootstrap from enforcement T042.

Scheduling hold (T053, 2026-11-18): [Owner decision J1](../docs/spec/decisions.md#j1-linux-development-hold) conserves this `planned` task. Do not continue Linux research or prototypes, implementation, environment/tool setup, platform tests, CI or packaging until the owner explicitly decides to resume Linux work and the task's other gates are met. Dependency completion or a general request to continue the backlog does not lift the hold. Existing scope, sources, findings and future Windows/Linux sharing remain preserved; Linux-only portions of mixed future tasks are also deferred.

## Acceptance Criteria

1. On the researched Linux environment, the host runs as a service; authorized-user UI control, reconnect and persisted state are verified.
2. Unauthorized-user IPC access is rejected and tests cover storage/host lifecycle.
3. Linux CI covers the host; specification records only verified capabilities, leaving actual blocking planned until T042.

## Checks

- Run `dotnet test FocusManager.Shared.slnf` and, after this task creates it, `dotnet test FocusManager.Linux.slnf` on Linux.
- Use the new systemd/development commands documented by this task on Ubuntu LTS; verify UI control, authorized/unauthorized IPC, reconnect, and persisted data.
- Inspect Linux CI and specification statuses; record exact Linux commands once created.

## Open Questions

When will the owner explicitly decide to resume Linux work? This scheduling gate remains unresolved independently of the listed dependencies; an agent may not resolve it on the owner's behalf.

## Result

Not started. No implementation or checks have been completed for this task.
