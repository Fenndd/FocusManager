---
id: T027
title: Design the Windows service and user-session boundary
milestone: M2
status: ready
depends_on: []
area: ["docs/architecture/decisions/**", "tasks/T016*", "contracts/**"]
---

# T027 — Design the Windows service and user-session boundary

## Goal

Define a reviewable service/user-session design before moving privileged enforcement into session 0.

## Sources

[Architecture](../docs/architecture/README.md), [task lifecycle](../docs/engineering/TASK_FORMAT.md), [Windows requirements](../docs/spec/platforms/windows.md) (WIN-001, WIN-007, WIN-010), [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md).

## Scope

In scope: inspect current AppEnforcer session filtering, ExplorerInterop COM, IpcPipeFactory ACLs and tray ownership; compare helper placement, identity/authentication, disconnect/reconnect, logoff and policy cleanup. Record a proposed ADR and split T016 further if the selected implementation exceeds one session. Define required contract additions with compatibility checks.
Out of scope: service implementation, choosing new protection behavior, installing a service.

## Acceptance Criteria

1. The ADR identifies service/helper responsibilities, authenticated user identity, IPC permissions and lifecycle/failure handling with alternatives and consequences.
2. T016 has exact boundaries and checks derived from the design; required contract changes are assigned before consumers.
3. ADR acceptance is recorded explicitly; proposed choices are not described as accepted.

## Checks

- Trace the three existing session-0 hazards in source and review proposed handling for an unauthorized user, reconnect, logoff and reboot.
- Inspect ADR status and T016 dependency/readiness; documentation-only checks apply.

## Open Questions

None

## Result

Not started. Created by T026 to make missing work or a split explicit. No implementation or checks have been completed.
