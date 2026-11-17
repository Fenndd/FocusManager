---
id: T019
title: Research Linux blocking mechanisms
milestone: M4
status: ready
depends_on: []
area: ["docs/architecture/decisions/**", "docs/spec/platforms/linux.md", "tasks/**", "ROADMAP.md"]
---

# T019 — Research Linux blocking mechanisms

## Goal

Validate Linux enforcement, service, and packaging mechanisms, then turn platform hypotheses into requirements and implementation tasks.

## Sources

Legacy classification: type `research`; platform `linux`; priority `medium`.

Requirement identifiers: `LNX-001`, `LNX-002`, `LNX-003`, `APP-001`, `SITE-005`, `FLD-001`, `CNT-003`, `GEN-006`.

Legacy ADR associations: ADR-0007.

[Linux requirements](../docs/spec/platforms/linux.md), [application rules](../docs/spec/apps.md), [site rules](../docs/spec/sites.md), [folders](../docs/spec/folders.md), [content](../docs/spec/content.md), [product requirements](../docs/spec/product.md), and [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md).

Linux is a main platform with Windows-equivalent functionality, including folder blocking (owner decisions B1, B5). `docs/spec/platforms/linux.md` is a draft containing hypotheses.

## Scope

In scope:
1. Compare process detection/termination and application identities including sandboxed packaging on a bounded Ubuntu LTS sample; identify untested distro/kernel gaps.
2. Prototype supported-browser policies and root systemd/user-restricted IPC as foundation mechanisms.
3. Record evidence and a proposed Linux mechanisms ADR. Obtain owner acceptance for supported distribution/environment decisions before dependent implementation; do not replace hypotheses with unsupported requirements.
4. Update T020/T042 using the selected mechanisms and create child research tasks for remaining core matrices. T043 owns folder, notification and packaging research; broader deployment verification is planned through T041.

Out of scope: production Linux code, folder/notification/package prototypes moved to T043.

Planning change (T026, 2026-11-17): Split broad Linux matrices into core feasibility T019 and integration/packaging research T043; production host and enforcement are T020/T042.

## Acceptance Criteria

1. A Linux foundation mechanisms ADR records tested versions, access boundaries, application/browser outcomes, limitations and explicit acceptance status.
2. T020/T042 have implementation-ready boundaries based on accepted findings; unresolved distribution decisions gate dependent work.
3. T043 retains folder/notification/package work and named tasks cover remaining research; untested hypotheses are not marked verified.

## Checks

- Execute the bounded process/browser/IPC sample, recording OS/kernel/browser versions, permissions and limitations.
- Review ADR acceptance, owner distribution decision, updated T020/T042 scopes and T043 handoff. Record actual prototype commands; none exist yet.

## Open Questions

None

## Result

Not started. No implementation or checks have been completed for this task.
