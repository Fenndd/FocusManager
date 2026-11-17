---
id: T004
title: Extract whitelist input validation from WinUI into Shared
milestone: M1
status: cancelled
depends_on: ["T001"]
area: ["src/Windows/FocusManager.App/Views/*WhitelistPage.xaml.cs", "src/Shared/FocusManager.Core/**"]
---

# T004 — Extract whitelist input validation from WinUI into Shared

## Goal

Historically, move whitelist input normalization and validation out of WinUI code-behind into shared code.

## Sources

Legacy classification: type `refactor`; platform `shared`; priority `medium`.

Requirement identifiers: `APP-006`, `SITE-003`.

Legacy ADR associations: ADR-0007.

[Application input](../docs/spec/apps.md), [site input](../docs/spec/sites.md), and [ADR-0007](../docs/architecture/decisions/0007-desktop-agent-and-tauri-ui.md).

Normalization and validation live in the WinUI page code-behind (`src/Windows/FocusManager.App/Views/*WhitelistPage.xaml.cs`).

## Scope

Historical scope: extract that input logic. No implementation is authorized under this cancelled task; the work is covered by T018 and common examples in T022.

## Acceptance Criteria

1. The cancellation reason and replacement tasks remain recorded.

## Checks

Inspect the cancellation record and links to T018 / T022. No implementation checks apply to this cancelled task.

## Open Questions

None

## Result

Cancelled on 2026-11-16. WinUI is being replaced by Tauri + React (ADR-0007). Input validation moves with the screens in T018 and is covered by common test vectors in T022.
