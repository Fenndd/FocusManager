---
id: T059
title: Research an optional Windows account switching restriction
milestone: M7
status: planned
depends_on: ["T034"]
area: ["docs/spec/protection.md", "docs/spec/platforms/windows.md", "docs/architecture/decisions/**", "tasks/T059-*"]
---

# T059 — Research an optional Windows account switching restriction

## Goal

Determine whether an opt-in restriction against switching Windows accounts can meet the owner's protection and session-isolation requirements, then define a bounded implementation proposal or honest limitation.

## Sources

- [Owner decision K1 Q3](../docs/spec/decisions.md#k1-windows-service-and-session-behavior).
- [L1 owner answers](../docs/spec/decisions.md#l1-profiles-default-exceptions-and-protection): separate optional setting; emergency exit removes the restriction with the session’s other restrictions.
- [PROT-012](../docs/spec/protection.md#prot-012-optional-windows-account-switch-restriction), [T034](T034-plan-protection-levels.md), [Windows requirements](../docs/spec/platforms/windows.md).

## Scope

In scope: compare supported Windows mechanisms and their privilege, scope and recovery behavior. Distinguish hiding a switching UI from actually preventing a switch; check lock/unlock, logoff, reboot, Remote Desktop, secondary account login and emergencies. A machine-wide switch policy may violate K1 Q15 and cannot be chosen silently. Define user consent, enabling/disabling of the separate setting, interaction with protection-level exit
and restoration after stop/crash before proposing implementation. Emergency exit must remove the
session’s account-switch restriction with its other restrictions; preserve other active sessions’ controls.

Out of scope: applying policies to this machine, production code, unsupported system patching, claiming complete protection from OS administrators, imposing the option on other users, or Linux work. Any active prototype must be a separately defined ready task in an isolated Windows environment.

## Acceptance Criteria

1. Primary-source evidence identifies mechanisms and precisely states each one's actual scope/bypasses/privileges and session-isolation consequences.
2. A reviewable option defines consent, exit/emergency/recovery and restoration requirements, with concrete tests or explicit feasibility limits.
3. Any unavoidable impact on other users/sessions or missing product setting is returned to the owner; no mechanism is marked supported without evidence.
4. T034 coverage and resulting bounded child tasks/ADR are linked, and implementation remains gated by accepted design/decisions.

## Checks

- Inspect current supported Windows documentation and K1/PROT-012 for contradictions.
- Review the proposal, sources, links, task dependencies and `git diff --check`.
- Record unavailable prototype commands honestly; this research task has no prescribed existing switching-test command.

## Open Questions

Exact supported OS/session scope, protection-level availability and mechanism feasibility are unresolved. The owner approved an optional capability, not a machine-wide policy or a guarantee that it can be implemented under the isolation requirement.

## Result

Not started. Planned under T034; no switching settings changed, no mechanism accepted and no implementation performed.
