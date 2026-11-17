---
id: T012
title: Align Core rules with the new specification
milestone: M1
status: planned
depends_on: ["T001"]
area: ["src/Shared/FocusManager.Core/Rules/RuleEvaluator.cs", "tests/Shared/FocusManager.Core.Tests/**", "tests/Windows/FocusManager.Agent.Tests/**", "test-vectors/**", "docs/spec/apps.md", "docs/spec/profiles.md", "docs/spec/sites.md"]
---

# T012 — Align Core rules with the new specification

## Goal

Apply the owner-approved application and site matching semantics in Core and record the corresponding Windows implementation statuses.

## Sources

Legacy classification: type `change`; platform `shared`; priority `high`.

Requirement identifiers: `APP-003`, `APP-004`, `PRF-004`, `SITE-002`, `SITE-004`.

[Application rules](../docs/spec/apps.md), [profiles](../docs/spec/profiles.md), [site rules](../docs/spec/sites.md), and [owner decisions](../docs/spec/decisions.md).

Owner decisions change current `src/Shared/FocusManager.Core/Rules/RuleEvaluator.cs` behavior:

| Requirement | Current behavior | Required behavior |
| --- | --- | --- |
| APP-003 removed, decision I1 | An application can be allowed by its `.exe` name | Full path only (APP-002) |
| APP-004 / PRF-004, decision E7 | Empty application lists block all new applications | Empty lists impose no restriction |
| SITE-002, decision I3 | `example.com` matches only that exact host | `example.com` matches the domain and all subdomains, as does `*.example.com` |

Chrome policy already treats `example.com` as including subdomains, so the site change applies only to `RuleEvaluator`.

## Scope

In scope:
1. Change `RuleEvaluator` according to the table.
2. Update `RuleEvaluatorTests` and agent tests in `tests/Windows/FocusManager.Agent.Tests` that assume the old behavior, for example `EvaluateApplicationStart_Allows_ByFileNameFallback_WhenFullPathUnavailable`.
3. Mark Windows implementation statuses `done` in `docs/spec/apps.md`, `profiles.md`, and `sites.md` once verified.
4. Follow the shared-rule test-vector policy (ADR-0005): create/update the minimal common examples for every changed matching rule even if test-vectors/ does not yet exist; T022 supplies the complete suite and loader.

Out of scope: WinUI warning text (SM-006); screens move to Tauri in T018.

Planning change (T026, 2026-11-17): Remove the conditional vector obligation: ADR-0005 requires examples with every shared matching-rule change.

## Acceptance Criteria

1. Behavior matches the table and is covered by tests.
2. `dotnet test FocusManager.Shared.slnf` passes; Windows tests pass on Windows.
3. The specification records the new implementation state.

## Checks

- Run `dotnet test FocusManager.Shared.slnf`, including the full-path-only, empty-list, domain/subdomain, and wildcard cases.
- Run `dotnet test FocusManager.Windows.slnf` on Windows.
- Inspect updated requirement statuses and shared examples against the table.

## Open Questions

None

## Result

Not started. No implementation or checks have been completed for this task.
